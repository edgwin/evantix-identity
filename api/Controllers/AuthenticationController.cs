using IdentityService.Dtos;
using IdentityService.Enums;
using IdentityService.ExtensionMethods;
using IdentityService.ExternalProvider;
using IdentityService.Models;
using IdentityService.Services;
using IdentityService.Utils;
using IdentityService.Utils.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace IdentityService.Controllers
{    
    [Produces("application/json")]
    [Route("api/Auth")]
    public class AuthenticationController : BaseController
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApiDbContext _db;
        private readonly IStringLocalizer<AuthenticationController> _localizer;
        private readonly IPasswordHistory _passwordHistory;
        private readonly AccountService _accountService;
        

        public AuthenticationController(SignInManager<ApplicationUser> signInManager,
                UserManager<ApplicationUser> userManager,
                ApiDbContext db,
                IStringLocalizer<AuthenticationController> localizer,
                IPasswordHistory passwordHistory,
                AccountService accountService)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _db = db;
            _localizer = localizer;
            _passwordHistory = passwordHistory;
            _accountService = accountService;
        }

        [HttpPost]
        [Route("Authentication")]
        [AllowAnonymous]
        public async Task<IActionResult> Authenticate(string username, string password, int appId)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return BadRequest(_localizer["El usuario y password no deben estar vacios"].Value);

            var result = await _signInManager.PasswordSignInAsync(username, password, false, lockoutOnFailure: false);
            if (!result.Succeeded)
            {
                return BadRequest(_localizer["Usuario o Password Invalidos"].Value);
            }
            var user = await _userManager.Users
                .SingleAsync(i => i.UserName == username);
            if (!user.IsEnabled)
            {
                return BadRequest(_localizer["El usuario esta deshabilitado"].Value);
            }
            if (!user.EmailConfirmed)
            {
                return BadRequest(_localizer["El usuario no esta confirmado, vea su cuenta de correo para encontrar el email para verificar su cuenta"].Value);
            }
            var response = GetLoginToken.Execute(user, _db, appId);
            return Ok(response);
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("LinkedInLogin")]
        public IActionResult LinkedInLoginAsync()
        {
            return Redirect("https://www.linkedin.com/oauth/v2/authorization?response_type=code&client_id=78p1nrxr7qwobe&redirect_uri=http://localhost:53055/api/Auth/LnkInAuthentication&state=1&scope=r_emailaddress%20r_liteprofile");
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("FacebookLogin")]
        public async Task FacebookLoginAsync()
        {
            await HttpContext.ChallengeAsync("Facebook", new AuthenticationProperties { RedirectUri = "/api/Auth/FbAuthentication" });
        }

        [HttpGet]
        [Route("FbAuthentication")]
        [AllowAnonymous]
        public async Task<IActionResult> FacebookLoginAuthenticationAsync([FromBody] ExternalProviderLoginResource resource)
        {
            if (string.IsNullOrEmpty(resource.Token))
                return BadRequest(_localizer["El token debe ser enviado"].Value);

            if (resource.AppId == 0)
                return BadRequest(_localizer["AppId no fue enviado"].Value);

            var authorizationTokens = await _accountService.ExternalLoginAsync(SocialMediaEnum.Facebook, resource, _db, _localizer);
            if (authorizationTokens.Ok)
                return Ok(authorizationTokens.Value);
            return BadRequest(authorizationTokens.Message);
        }

        [HttpGet]
        [Route("LnkInAuthentication")]
        public async Task<IActionResult> LinkedInLoginAsync(string code, string state, string error, string error_description)
        {
            if (string.IsNullOrEmpty(code))
            {
                return BadRequest(_localizer["El token debe ser enviado"].Value);
            }

            if (Int32.TryParse(state, out int AppId))
            {
                var authorizationTokens = await _accountService.ExternalLoginAsync(SocialMediaEnum.LinkedIn, code, _db, _localizer, AppId);
                if (authorizationTokens.Ok)
                    return Ok(authorizationTokens.Value);
                return BadRequest(authorizationTokens.Message);
            }
            return BadRequest(_localizer["El identificador de Aplicacion es invalido"].Value);            
        }

        [HttpPost]
        [Route("Create")]
        [AllowAnonymous]
        public async Task<IActionResult> CreateUser(UserModel model)
        {
            if (model == null)
                return BadRequest(_localizer["Se envio informacion invalida"].Value);

            if (model.Password == null)
                return BadRequest( _localizer["Password requerido"].Value);

            if (!_db.Applications.Where(c => c.AppId == model.AppId).Any())
                return BadRequest(_localizer["La applicacion no existe"]);

            var user = new ApplicationUser()
            {
                UserName = model.Email.Trim(),
                Email = model.Email.Trim(),
                FirstName = model.FirstName.Trim(),
                LastName = model.LastName.Trim(),
                PhoneNumber = model.PhoneNumber.Trim(),
                CellPhone = model.CellPhone.Trim(),                
                IsEnabled = model.IsEnabled
            };
            
            var addUserResult = await _userManager.CreateUserAsync(_db, user, model.Password, model.IsAdmin, model.AppId, _localizer);

            if (addUserResult.Success)
            {
                try
                {                   
                    //Add password to passwordHistory
                    var passwordModel = new Models.PasswordHistory()
                    {
                        CreateDate = DateTime.Now,
                        PasswordHash = user.PasswordHash,
                        UserId = user.Id
                    };
                    if (!await _passwordHistory.SavePassword(passwordModel))
                        throw new Exception(_localizer["Fallo al guardar el historial de la contraseña"].Value);
                    //Send email to confirm account
                    var emailToken = _userManager.GenerateEmailConfirmationTokenAsync(user).Result;
                    var confirmationLink = Url.Action("ConfirmEmail", "api/Auth", new
                    {
                        userid = user.Id,
                        token = emailToken
                    },
                    protocol: HttpContext.Request.Scheme);

                    var system = _db.UserApplications.Where(up => up.ApplicationUser.Id == user.Id)
                        .Include(a => a.Applications).ToList().Where(c => c.Applications.AppId == model.AppId).FirstOrDefault().Applications.Nombre;

                    var emailDto = new EmailDto()
                    {
                        Body = string.Format(_localizer["Hola {0} {1} <br />Gracias por tu interes en {2}.<br />Tu cuenta esta casi lista para ser usada. Solo un paso mas, por favor da click en la liga de abajo para activar tu cuenta.<br /><br />{3} <br /><br />Si la liga no funciona, copia y pegala en tu navegador de internet.<br/>El equipo de {2}."].Value, model.FirstName, model.LastName, system, confirmationLink),
                        FromAddress = Configuration.Config.GetSection("EmailSettings:FromEmail").Value,
                        FromName = Configuration.Config.GetSection("EmailSettings:FromName").Value,
                        ToAddress = model.Email,
                        ToName = $"{model.FirstName} {model.LastName}",
                        Subject = string.Format(_localizer["Por favor, confirme su cuenta de {0}"].Value, system)
                    };
                    await EmailSender.SendAsync(emailDto);
                    return Ok();
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }            

            return BadRequest(ErrorHelper.GetErrors(addUserResult.IdentityResult, _localizer));
        }

        [HttpGet]
        [Route("ConfirmEmail")]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string userId, [FromQuery] string token)
        {
            if (userId == null || token == null)
            {
                return BadRequest();
            }

            try
            {
                var user = await _userManager.Users
                    .SingleAsync(i => i.Id == userId);
                IdentityResult result = await _userManager.ConfirmEmailAsync(user, token);

                if (result.Succeeded)
                {
                    string url = _db.UserApplications.Where(up => up.ApplicationUser.Id == userId).Select(up => up.Applications.Url).FirstOrDefault();
                    return Redirect(url);
                }
                return null;
            }catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Route("DisableUser")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DisableUser(bool disable)
        {
            try
            {
                var user = UserUtils.GetUser(HttpContext, _userManager);
                user.IsEnabled = !disable;
                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                    return Ok();
                return BadRequest(ErrorHelper.GetErrors(result, _localizer));                
            }catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }        
    }
}