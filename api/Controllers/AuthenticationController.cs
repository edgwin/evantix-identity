using IdentityService.Dtos;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<AuthenticationController> _logger;

        public AuthenticationController(SignInManager<ApplicationUser> signInManager,
                UserManager<ApplicationUser> userManager,
                ApiDbContext db,
                IStringLocalizer<AuthenticationController> localizer,
                IPasswordHistory passwordHistory,
                RoleManager<IdentityRole> roleManager,
                IConfiguration configuration,
                ILogger<AuthenticationController> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _db = db;
            _localizer = localizer;
            _passwordHistory = passwordHistory;
            _roleManager = roleManager;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost]
        [Route("Authentication")]
        [AllowAnonymous]
        public async Task<IActionResult> Authenticate([FromBody] AuthenticateDto request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.UserName) || string.IsNullOrWhiteSpace(request.Password))
                    return BadRequest(_localizer["El usuario y password no deben estar vacios"].Value);

                var result = await _signInManager.PasswordSignInAsync(request.UserName, request.Password, false, lockoutOnFailure: true);
                if (result.IsLockedOut)
                {
                    _logger.LogWarning($"Cuenta bloqueada por intentos fallidos: {request.UserName}");
                    return BadRequest(_localizer["Cuenta bloqueada temporalmente por demasiados intentos fallidos. Intente de nuevo en 15 minutos."].Value);
                }
                if (!result.Succeeded)
                {
                    _logger.LogWarning($"Intento de login fallido para usuario: {request.UserName}");
                    return BadRequest(_localizer["Usuario o Password Invalidos"].Value);
                }
                var user = _userManager.Users.FirstOrDefault(i => i.UserName == request.UserName);
                if (user == null)
                {
                    _logger.LogError($"Usuario autenticado pero no encontrado en DB: {request.UserName}");
                    return BadRequest(_localizer["Usuario o Password Invalidos"].Value);
                }
                if (!user.IsEnabled)
                {
                    _logger.LogWarning($"Usuario deshabilitado intentó login: {request.UserName}");
                    return BadRequest(_localizer["El usuario esta deshabilitado"].Value);
                }
                if (!user.IsEmailConfirmed(_db, request.AppId))
                {
                    return BadRequest(_localizer["El usuario no esta confirmado, vea su cuenta de correo para encontrar el email para verificar su cuenta"].Value);
                }

                var userApp = _db.UserApplications.Where(up => up.ApplicationUser.Id == user.Id)
                               .Include(a => a.Applications).ToList().Where(c => c.Applications.AppId == request.AppId).FirstOrDefault();
                if (userApp == null)
                {
                    _logger.LogError($"No se encontró la aplicación {request.AppId} para el usuario {request.UserName}");
                    return BadRequest(_localizer["La applicacion no existe"]);
                }
                var appToken = userApp.Applications.AppToken;
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
                _logger.LogInformation($"Login exitoso: {request.UserName}");
                return Ok(retVal);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error en Authenticate para usuario {request.UserName}: {ex.Message}");
                return StatusCode(500, "Error interno durante la autenticación");
            }
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