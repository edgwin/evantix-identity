using Azure.Core;
using Microsoft.Extensions.Logging;
using IdentityService.Dtos;
using IdentityService.Enums;
using IdentityService.ExtensionMethods;
using IdentityService.ExternalProvider;
using IdentityService.Models;
using IdentityService.ResultTypes;
using IdentityService.Services;
using IdentityService.Utils;
using IdentityService.Utils.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Web;

namespace IdentityService.Controllers
{
    [Produces("application/json")]
    [Route("api/User")]
    public class UserController : BaseController
    {
        private UserManager<ApplicationUser> _userManager;
        private IPasswordHistory _passwordHistory;
        private IStringLocalizer<UserController> _localizer;
        private readonly ApiDbContext _db;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IStringLocalizer<AuthenticationController> _authLocalizer;
        private readonly AccountService _accountService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<UserController> _logger;

        public UserController(UserManager<ApplicationUser> userManager, 
            IPasswordHistory passwordHistory,
            IStringLocalizer<UserController> localizer,
            ApiDbContext db,
            RoleManager<IdentityRole> roleManager,
            IStringLocalizer<AuthenticationController> authLocalizer,
            AccountService accountService, IHttpClientFactory httpClientFactory,
            ILogger<UserController> logger)
        {
            _userManager = userManager;
            _passwordHistory = passwordHistory;
            _localizer = localizer;
            _db = db;
            _roleManager = roleManager;
            _authLocalizer = authLocalizer;
            _accountService = accountService;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        [HttpPost]
        [Route("Create")]
        [AllowAnonymous]
        public async Task<IActionResult> CreateUser([FromBody] UserModel model)
        {
            if (model == null)
                return BadRequest(_localizer["Se envio informacion invalida"].Value);

            if (model.Password == null)
                return BadRequest(_localizer["Password requerido"].Value);

            if (!_db.Applications.Where(c => c.AppId == model.AppId).Any())
                return BadRequest(_localizer["La applicacion no existe"]);
            if (await _roleManager.FindByNameAsync(model.Role.ToString()) == null)
            {
                return BadRequest(_localizer["El role de usuario no existe"]);
            }
            //validar el modelo
            var user = new ApplicationUser()
            {
                UserName = model.Email.Trim(),
                Email = model.Email.Trim(),
                FirstName = model.FirstName.Trim(),
                LastName = model.LastName.Trim(),
                IsEnabled = model.IsEnabled,
                IsSocialUser = model.IsSocialUser,
                Picture = "<svg height=\"32px\" width=\"32px\" version=\"1.1\" id=\"Capa_1\" xmlns=\"http://www.w3.org/2000/svg\" xmlns:xlink=\"http://www.w3.org/1999/xlink\" \r\n\t viewBox=\"0 0 53 53\" xml:space=\"preserve\">\r\n<path style=\"fill:#E7ECED;\" d=\"M18.613,41.552l-7.907,4.313c-0.464,0.253-0.881,0.564-1.269,0.903C14.047,50.655,19.998,53,26.5,53\r\n\tc6.454,0,12.367-2.31,16.964-6.144c-0.424-0.358-0.884-0.68-1.394-0.934l-8.467-4.233c-1.094-0.547-1.785-1.665-1.785-2.888v-3.322\r\n\tc0.238-0.271,0.51-0.619,0.801-1.03c1.154-1.63,2.027-3.423,2.632-5.304c1.086-0.335,1.886-1.338,1.886-2.53v-3.546\r\n\tc0-0.78-0.347-1.477-0.886-1.965v-5.126c0,0,1.053-7.977-9.75-7.977s-9.75,7.977-9.75,7.977v5.126\r\n\tc-0.54,0.488-0.886,1.185-0.886,1.965v3.546c0,0.934,0.491,1.756,1.226,2.231c0.886,3.857,3.206,6.633,3.206,6.633v3.24\r\n\tC20.296,39.899,19.65,40.986,18.613,41.552z\"/>\r\n<g>\r\n\t<path style=\"fill:#556080;\" d=\"M26.953,0.004C12.32-0.246,0.254,11.414,0.004,26.047C-0.138,34.344,3.56,41.801,9.448,46.76\r\n\t\tc0.385-0.336,0.798-0.644,1.257-0.894l7.907-4.313c1.037-0.566,1.683-1.653,1.683-2.835v-3.24c0,0-2.321-2.776-3.206-6.633\r\n\t\tc-0.734-0.475-1.226-1.296-1.226-2.231v-3.546c0-0.78,0.347-1.477,0.886-1.965v-5.126c0,0-1.053-7.977,9.75-7.977\r\n\t\ts9.75,7.977,9.75,7.977v5.126c0.54,0.488,0.886,1.185,0.886,1.965v3.546c0,1.192-0.8,2.195-1.886,2.53\r\n\t\tc-0.605,1.881-1.478,3.674-2.632,5.304c-0.291,0.411-0.563,0.759-0.801,1.03V38.8c0,1.223,0.691,2.342,1.785,2.888l8.467,4.233\r\n\t\tc0.508,0.254,0.967,0.575,1.39,0.932c5.71-4.762,9.399-11.882,9.536-19.9C53.246,12.32,41.587,0.254,26.953,0.004z\"/>\r\n</g>\r\n</svg>"
            };

            var addUserResult = await _userManager.CreateUserAsync(_db, user, model.Password, model.Role.ToString(), model.AppId, _localizer);

            if (addUserResult.IsDuplicated)
            {
                user = await _userManager.FindByNameAsync(model.Email);
            }
            var socialUser = new SocialUserModel()
            {
                Email = model.Email.Trim(),
                FirstName = model.FirstName.Trim(),
                LastName = model.LastName.Trim(),
                Picture = model.SocialPicture.Trim(),
            };

            if (addUserResult.Success)
            {
                try
                {
                    //verify if AppId exists
                    if (!_db.Applications.Any(a => a.AppId == model.AppId))
                    {
                        throw new Exception(_localizer["AppId invalido"].Value);
                    }
                    //Add password to passwordHistory
                    var passwordModel = new Models.PasswordHistory()
                    {
                        CreateDate = DateTime.Now,
                        PasswordHash = user.PasswordHash,
                        UserId = user.Id,
                        AppId = model.AppId
                    };
                    if (!await _passwordHistory.SavePassword(passwordModel))
                        throw new Exception(_localizer["Fallo al guardar el historial de la contraseña"].Value);
                    //crea el usuario en la otra API
                    var appToken = _db.UserApplications.Where(up => up.ApplicationUser.Id == user.Id)
                           .Include(a => a.Applications).ToList().Where(c => c.Applications.AppId == model.AppId).FirstOrDefault().Applications.AppToken;
                    var accessToken = GetLoginToken.Execute(user, _db, model.AppId, appToken).access_token;
                    var response = await CreateUserInEvantix(user, accessToken, model.Role.ToString());                    

                    if (!model.IsSocialUser)
                    {
                        //Send email to confirm account
                        var emailToken = _userManager.GenerateUserTokenAsync(user, "EmailConfirm", "EmailConfirm").Result;
                        emailToken = HttpUtility.UrlEncode(emailToken);
                        var system = _db.UserApplications.Where(up => up.ApplicationUser.Id == user.Id)
                                        .Include(a => a.Applications).ToList().Where(c => c.Applications.AppId == model.AppId)
                                            .FirstOrDefault().Applications.Nombre;

                        var request = HttpContext.Request;
                        var systemUrl = $"{request.Scheme}://{request.Host}/";

                        //var systemUrl = _db.UserApplications.Where(up => up.ApplicationUser.Id == user.Id)
                        //    .Include(a => a.Applications).ToList().Where(c => c.Applications.AppId == model.AppId).FirstOrDefault().Applications.HomePage;

                        var confirmLink = model.ConfirmUrl ?? Url.Action(nameof(ConfirmEmail), "User");
                        confirmLink = confirmLink.Substring(1, confirmLink.Length - 1);
                        var confirmationLink = $"{systemUrl}{confirmLink}?userid={user.Id}&token={emailToken}&appId={model.AppId}";

                        var emailDto = new EmailDto()
                        {
                            Body = string.Format(_localizer["Hola {0} {1} <br />Gracias por tu interes en {2}.<br />Tu cuenta esta casi lista para ser usada. Solo un paso mas, por favor da click en la liga de abajo para activar tu cuenta.<br /><br /> <a href=\"{3}\">{3}</a> <br /><br />Si la liga no funciona, copia y pegala en tu navegador de internet.<br/>El equipo de {2}."].Value, model.FirstName, model.LastName, system, confirmationLink),
                            FromAddress = Configuration.Config.GetSection("EmailSettings:FromEmail").Value,
                            FromName = Configuration.Config.GetSection("EmailSettings:FromName").Value,
                            ToAddress = model.Email,
                            ToName = $"{model.FirstName} {model.LastName}",
                            Subject = string.Format(_localizer["Por favor, confirme su cuenta de {0}"].Value, system)
                        };
                        await EmailSender.SendAsync(emailDto);
                        return Ok();
                    }
                    return Ok(new { socialUser });
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }else if (model.IsSocialUser)
            {
                return Ok(new { socialUser });
            }

            return BadRequest(ErrorHelper.GetErrors(addUserResult.IdentityResult, _localizer));
        }

        [HttpGet]
        [Route("ConfirmEmail")]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string userId, [FromQuery] string appId, [FromQuery] string token)
        {
            if (userId == null || token == null)
            {
                return BadRequest();
            }

            try
            {
                var user = _userManager.Users.FirstOrDefault(i => i.Id == userId);
                var result = await _userManager.EmailConfirmAsync(_db, Convert.ToInt32(appId), user, token, _authLocalizer);

                if (result.IsValid)
                {                    
                    string url = _db.UserApplications.Where(up => up.ApplicationUser.Id == userId && up.Applications.AppId == Convert.ToInt32(appId)).Select(up => up.Applications.Url).FirstOrDefault();
                    url += "&confirmed=true";
                    return Redirect(url);
                }
                return BadRequest(result.ErrorMessage);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private async Task<IActionResult> CreateUserInEvantix(ApplicationUser user, string accessToken, string role)
        {
            var client = _httpClientFactory.CreateClient();
            var evantixUser = new EvantixUserModel()
            {
                Id = user.Id,
                Apellidos = user.LastName,
                Nombres = user.FirstName,
                Email = user.Email,
                Role = role
            };

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var url = Configuration.Config.GetSection("ApplicationNameSettings:Evantix:API").Value;
            var response = await client.PostAsJsonAsync($"{url}/api/User/Create", evantixUser);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode, $"Error desde API externa: {error}");
            }
            return Ok();            
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
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("forgotpassword")]
        public async Task<IActionResult> ForgotPassword([FromBody] EmailRequestDto request)
        {

            try
            {
                var email = request.Email;
                if (email == null)
                {
                    return BadRequest(_localizer["Direccion de correo faltante"].Value);
                }

                var user = await _userManager.FindByEmailAsync(email);

                if (user == null)
                {
                    return BadRequest(_localizer["No se encontro el usuario"].Value);
                }

                string code = await _userManager.GeneratePasswordResetTokenAsync(user);
                //string resetUrl = GetProtocolUrl() + HttpContext.Current.Request.Url.Authority + "/home/resetpassword?code=" + code;

                //string resetUrl = string.Format("{0}/api/User/resetpassword?code={1}", UrlHelper.GetUrlAuthority(HttpContext), code);
                var systemUrl = _db.UserApplications.Where(up => up.ApplicationUser.Id == user.Id)
                                    .Include(a => a.Applications).ToList().Where(c => c.Applications.AppId == request.AppId).FirstOrDefault().Applications.HomePage;
                string resetUrl = string.Format("{0}forgotPassword?code={1}&email={2}", systemUrl, Uri.EscapeDataString(code), Uri.EscapeDataString(email));
                var system = "Evantix";
                var emailDto = new EmailDto()
                {
                    Body = string.Format(_localizer["Hola {0} {1}<br /><br />Da click en la liga siguiente para reestablecer tu contraseña de {2}.<br /><br /><a href=\"{3}\">{3}</a><br /><br />Esta liga sera valida por las siguientes 24 horas despues de recibir este email. Si tu no hiciste esta solicitud, por favor ignora este email, tu contraseña no ha sido cambiada."].Value, user.FirstName, user.LastName, system, resetUrl),
                    FromAddress = Configuration.Config.GetSection("EmailSettings:FromEmail").Value,
                    FromName = Configuration.Config.GetSection("EmailSettings:FromName").Value,
                    Subject = string.Format(_localizer["Restablecer contraseña {0}"].Value, system),
                    ToAddress = email,
                    ToName = user.FirstName + " " + user.LastName
                };
                await EmailSender.SendAsync(emailDto);

                return Ok();
            }catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("resetpassword", Name = "ResetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto model)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(model.Email);

                if (user == null)
                {
                    _logger.LogWarning($"Reset password: usuario no encontrado con email {model.Email}");
                    return BadRequest(_localizer["Usuario no encontrado"].Value);
                }

                if (_passwordHistory.PasswordAlreadyExists(user.Id, model.Password))
                {
                    return BadRequest(_localizer["Al parecer ya haz usado esta contraseña con anterioridad. Por favor, intenta con una diferente"].Value);
                }
                IdentityResult result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);

                if (result.Succeeded)
                {
                    var pswModel = new Models.PasswordHistory()
                    {
                        CreateDate = DateTime.Now,
                        PasswordHash = model.Password,
                        UserId = user.Id
                    };
                    if (await _passwordHistory.SavePassword(pswModel))
                    {                    
                        result = await _userManager.UpdateAsync(user);
                        _logger.LogInformation($"Contraseña restablecida exitosamente para {model.Email}");
                        return Ok();
                    }
                    else
                    {
                        return BadRequest(GetErrors(result));
                    }
                }
                else
                {
                    _logger.LogWarning($"Fallo al restablecer contraseña para {model.Email}: {GetErrors(result)}");
                    return BadRequest(GetErrors(result));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error en ResetPassword para {model.Email}: {ex.Message}");
                return StatusCode(500, "Error interno al restablecer la contraseña");
            }
        }

        [HttpPost]
        [Route("ChangePassword")]
        [Authorize]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto model)
        {
            try
            {
                if (model == null || !ModelState.IsValid)
                {
                    return BadRequest(GetModelErrors());
                }
           
                var userId = _userManager.GetUserId(HttpContext.User);
                var user = await _userManager.FindByIdAsync(userId);

                if (user == null)
                {
                    _logger.LogError($"ChangePassword: usuario no encontrado con Id {userId}");
                    return BadRequest(_localizer["Usuario no encontrado"].Value);
                }

                if (_passwordHistory.PasswordAlreadyExists(userId, model.NewPassword))
                {
                    return BadRequest(_localizer["No puedes reusar contraseñas anteriores"].Value);
                }
                IdentityResult result = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);

                if (result.Succeeded)
                {
                    if (!_passwordHistory.PasswordAlreadyExists(userId, user.PasswordHash))
                    {

                        result = await _userManager.UpdateAsync(user);
                        if (result.Succeeded)
                        {
                            var pswHistory = new Models.PasswordHistory()
                            {
                                CreateDate = DateTime.Now,
                                PasswordHash = user.PasswordHash,
                                UserId = userId
                            };
                            await _passwordHistory.SavePassword(pswHistory);
                            _logger.LogInformation($"Contraseña cambiada exitosamente para userId {userId}");
                            return Ok();
                        }
                        return BadRequest(_localizer["No se pudo cambiar la contraseña por un error Interno"].Value);
                    }
                    else
                    {
                        return BadRequest(_localizer["No se pudo cambiar la contraseña por un error Interno"].Value);
                    }
                }
                else
                {
                    return BadRequest(GetErrors(result));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error en ChangePassword: {ex.Message}");
                return StatusCode(500, "Error interno al cambiar la contraseña");
            }
        }

        private string GetErrors(IdentityResult errors)
        {
            string errorString = _localizer["Ocurrieron los siguientes errores:\n"].Value;
            foreach (var error in errors.Errors)
            {
                errorString += error.Description + "\n";
            }

            return errorString;
        }

        [HttpPost("google")]
        public async Task<IActionResult> GoogleLogin([FromBody] ExternalProviderLoginResource resource)
        {            
            if (string.IsNullOrEmpty(resource.Token))
                return BadRequest(_localizer["El token debe ser enviado"].Value);

            if (resource.AppId == 0)
                return BadRequest(_localizer["AppId no fue enviado"].Value);

            var authorizationTokens = await _accountService.ExternalLoginAsync(SocialMediaEnum.Google, resource, _db, _localizer);
            if (authorizationTokens.Ok)
            {
                // Buscar usuario para obtener el Id
                var domainUser = await _userManager.FindByEmailAsync(authorizationTokens.Value.email);

                var retVal = new UserResultType()
                {
                    access_token = authorizationTokens.Value.access_token,
                    appId = authorizationTokens.Value.appId,
                    appHomePage = authorizationTokens.Value.appHomePage,
                    appName = authorizationTokens.Value.appName,
                    expires_in = authorizationTokens.Value.expires_in,
                    refresh_token = authorizationTokens.Value.refresh_token,
                    User = new UserResult()
                    {
                        userId = domainUser?.Id,
                        firstName = authorizationTokens.Value.firstName,
                        lastName = authorizationTokens.Value.lastName,
                        role = authorizationTokens.Value.role,
                        userName = authorizationTokens.Value.userName,
                        email = authorizationTokens.Value.email,
                        picture = authorizationTokens.Value.picture,
                        isSocial = true
                    }
                };

                // Crear usuario en Evantix API
                try
                {
                    if (domainUser != null)
                    {
                        await CreateUserInEvantix(domainUser, authorizationTokens.Value.access_token, authorizationTokens.Value.role ?? "User");
                    }
                }
                catch (Exception ex) { _logger.LogWarning($"Fallo al registrar usuario Google en Evantix: {ex.Message}"); }

                return Ok(retVal);
            }
            return BadRequest(authorizationTokens.Message);
        }

        [HttpPost]
        [Route("facebook")]
        [AllowAnonymous]
        public async Task<IActionResult> FacebookLoginAuthenticationAsync([FromBody] ExternalProviderLoginResource resource)
        {
            if (string.IsNullOrEmpty(resource.Token))
                return BadRequest(_localizer["El token debe ser enviado"].Value);

            if (resource.AppId == 0)
                return BadRequest(_localizer["AppId no fue enviado"].Value);

            var authorizationTokens = await _accountService.ExternalLoginAsync(SocialMediaEnum.Facebook, resource, _db, _localizer);            
            if (authorizationTokens.Ok)
            {
                // Buscar usuario para obtener el Id
                var domainUser = await _userManager.FindByEmailAsync(authorizationTokens.Value.email);

                var retVal = new UserResultType()
                {
                    access_token = authorizationTokens.Value.access_token,
                    appId = authorizationTokens.Value.appId,
                    appHomePage = authorizationTokens.Value.appHomePage,
                    appName = authorizationTokens.Value.appName,
                    expires_in = authorizationTokens.Value.expires_in,
                    refresh_token = authorizationTokens.Value.refresh_token,
                    User = new UserResult()
                    {
                        userId = domainUser?.Id,
                        firstName = authorizationTokens.Value.firstName,
                        lastName = authorizationTokens.Value.lastName,
                        role = authorizationTokens.Value.role,
                        userName = authorizationTokens.Value.userName,
                        email = authorizationTokens.Value.email,
                        picture = authorizationTokens.Value.picture,
                        isSocial = true
                    }
                };

                // Crear usuario en Evantix API
                try
                {
                    if (domainUser != null)
                    {
                        await CreateUserInEvantix(domainUser, authorizationTokens.Value.access_token, authorizationTokens.Value.role ?? "User");
                    }
                }
                catch (Exception ex) { _logger.LogWarning($"Fallo al registrar usuario Facebook en Evantix: {ex.Message}"); }

                return Ok(retVal);
            }
            return BadRequest(authorizationTokens.Message);
        }

        private static string HashPassword(string password)
        {
            // Salt = aleatorio para que el mismo password no genere el mismo hash
            byte[] salt = RandomNumberGenerator.GetBytes(16); // 16 bytes = 128 bits
            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256);

            byte[] hash = pbkdf2.GetBytes(32); // 32 bytes = 256 bits
            byte[] hashBytes = new byte[48]; // 16 + 32

            // Juntamos el salt + hash en un solo array para guardarlo
            Array.Copy(salt, 0, hashBytes, 0, 16);
            Array.Copy(hash, 0, hashBytes, 16, 32);

            // Lo convertimos en string (Base64) para guardarlo en base de datos
            return Convert.ToBase64String(hashBytes);
        }

        private static string GenerateRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            Random random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}