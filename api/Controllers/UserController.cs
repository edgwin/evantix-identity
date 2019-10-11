using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityService.Dtos;
using IdentityService.Models;
using IdentityService.Utils;
using IdentityService.Utils.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace IdentityService.Controllers
{
    [Produces("application/json")]
    [Route("api/User")]
    public class UserController : BaseController
    {
        private UserManager<ApplicationUser> _userManager;
        private IPasswordHistory _passwordHistory;
        private IStringLocalizer<UserController> _localizer;

        public UserController(UserManager<ApplicationUser> userManager, 
            IPasswordHistory passwordHistory,
            IStringLocalizer<UserController> localizer)
        {
            _userManager = userManager;
            _passwordHistory = passwordHistory;
            _localizer = localizer;
        }
        [HttpPost]
        [AllowAnonymous]
        [Route("forgotpassword")]
        public async Task<IActionResult> ForgotPassword([FromBody] EmailRequestDto Email)
        {

            try
            {
                var email = Email.email;
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

                string resetUrl = string.Format("{0}/api/User/resetpassword?code={1}", UrlHelper.GetUrlAuthority(HttpContext), code);
                var system = "iTarea.com";
                var emailDto = new EmailDto()
                {
                    Body = string.Format(_localizer["Hola {0} {1}<br /><br />Da click en la liga siguiente para reestablecer tu contraseña de {2}.<br /><br />{3}<br /><br />Esta liga sera valida por las siguientes 24 horas despues de recibir este email. Si tu no hiciste esta solicitud, por favor ignora este email, tu contraseña no ha sido cambiada."].Value, user.FirstName, user.LastName, system, resetUrl),
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
        public async Task<IActionResult> ResetPassword(ResetPasswordDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
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
                    return Ok();
                }
                else
                {
                    return BadRequest(GetErrors(result));
                }
            }
            else
            {
                return BadRequest(GetErrors(result));
            }
        }

        [HttpPost]
        [Route("ChangePassword")]
        [Authorize]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto model)
        {
            if (model == null || !ModelState.IsValid)
            {
                return BadRequest(GetModelErrors());
            }
           
            var userId = _userManager.GetUserId(HttpContext.User);
            var user = await _userManager.FindByIdAsync(userId);

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

        private string GetErrors(IdentityResult errors)
        {
            string errorString = _localizer["Ocurrieron los siguientes errores:\n"].Value;
            foreach (var error in errors.Errors)
            {
                errorString += error.Description + "\n";
            }

            return errorString;
        }
    }
}