using IdentityService.Dtos;
using IdentityService.Enums;
using IdentityService.Models;
using IdentityService.ResultTypes;
using IdentityService.Services;
using IdentityService.Utils;
using IdentityService.Utils.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using System;
using System.Linq;
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
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;

        public AuthenticationController(SignInManager<ApplicationUser> signInManager,
                UserManager<ApplicationUser> userManager,
                ApiDbContext db,
                IStringLocalizer<AuthenticationController> localizer,
                IPasswordHistory passwordHistory,
                RoleManager<IdentityRole> roleManager,
                IConfiguration configuration)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _db = db;
            _localizer = localizer;
            _passwordHistory = passwordHistory;
            _roleManager = roleManager;
            _configuration = configuration;
        }

        [HttpPost]
        [Route("Authentication")]
        [AllowAnonymous]
        public async Task<IActionResult> Authenticate([FromBody] AuthenticateDto request)
        {
            if (string.IsNullOrWhiteSpace(request.UserName) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest(_localizer["El usuario y password no deben estar vacios"].Value);

            var result = await _signInManager.PasswordSignInAsync(request.UserName, request.Password, false, lockoutOnFailure: false);
            if (!result.Succeeded)
            {
                return BadRequest(_localizer["Usuario o Password Invalidos"].Value);
            }
            var user = _userManager.Users.FirstOrDefault(i => i.UserName == request.UserName);
            if (!user.IsEnabled)
            {
                return BadRequest(_localizer["El usuario esta deshabilitado"].Value);
            }
            if (!user.IsEmailConfirmed(_db, request.AppId))
            {
                return BadRequest(_localizer["El usuario no esta confirmado, vea su cuenta de correo para encontrar el email para verificar su cuenta"].Value);
            }

            var appToken = _db.UserApplications.Where(up => up.ApplicationUser.Id == user.Id)
                           .Include(a => a.Applications).ToList().Where(c => c.Applications.AppId == request.AppId).FirstOrDefault().Applications.AppToken;
            var response = GetLoginToken.Execute(user, _db, request.AppId, appToken);
            var retVal = new UserResultType()
            {
                access_token = response.access_token,
                appId = response.appId,
                appHomePage = response.appHomePage,
                appName = response.appName,
                expires_in = response.expires_in,
                refresh_token = response.refresh_token,
                User = new UserResult()
                {
                    userId = user.Id,
                    firstName = response.firstName,
                    lastName = response.lastName,
                    role = response.role,
                    userName = response.userName,
                    email = response.email,
                    picture = response.picture
                }
            };
            return Ok(retVal);
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("LinkedInLogin")]
        public IActionResult LinkedInLoginAsync()
        {
            var authUrl = _configuration["LinkedInAuth:AuthorizationUrl"];
            var clientId = _configuration["LinkedInAuth:ClientId"];
            var redirectUri = Uri.EscapeDataString(_configuration["LinkedInAuth:RedirectUri"]);
            return Redirect($"{authUrl}?response_type=code&client_id={clientId}&redirect_uri={redirectUri}&state=1&scope=r_emailaddress%20r_liteprofile");
        }     
    }
}