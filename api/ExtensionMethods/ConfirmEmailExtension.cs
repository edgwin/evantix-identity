using IdentityService.Controllers;
using IdentityService.Dtos;
using IdentityService.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityService.ExtensionMethods
{
    public static class ConfirmEmailExtension
    {
        public static async Task<ConfirmEmailDto> EmailConfirmAsync(this UserManager<ApplicationUser> userManager, ApiDbContext db, int appId, ApplicationUser user, string token, IStringLocalizer<AuthenticationController> localizer)
        {
            var tokenValidation = await userManager.VerifyUserTokenAsync(user, "EmailConfirm", "EmailConfirm", token);
            if (!tokenValidation)
            {
                return new ConfirmEmailDto()
                {
                    ErrorMessage = localizer["Token de confirmacion invalido"].Value,
                    IsValid = false
                };
            }
            
            var userApplications = db.UserApplications.Where(ua => ua.ApplicationUser.Id == user.Id && ua.Applications.AppId == appId).FirstOrDefault();
            if (userApplications == null)
            {
                return new ConfirmEmailDto()
                {
                    ErrorMessage = localizer["AppId invalido"].Value,
                    IsValid = false
                };
            }
            //Verifica si ya se confirmo
            if (db.UserApplications.Where(ua => ua.EmailConfirmed == false).Any())
            {                
                userApplications.EmailConfirmed = true;
                db.UserApplications.Update(userApplications);
                await db.SaveChangesAsync();
                return new ConfirmEmailDto()
                {
                    ErrorMessage = "",
                    IsValid = true
                };
            }
            else
            {
                return new ConfirmEmailDto()
                {
                    ErrorMessage = localizer["Ya se encuentra verificada la cuenta"].Value,
                    IsValid = false
                };
            }
        }
    }
}
