using IdentityService.Models;
using IdentityService.ResultTypes;
using IdentityService.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityService.ExtensionMethods
{
    public static class UserManagerExtension
    {
        public static async Task<CreateUserResultType> CreateUserAsync<T>(this UserManager<ApplicationUser> userManager, ApiDbContext db, ApplicationUser user, string password, bool isAdmin, int appId, IStringLocalizer<T> localizer)
        {
            IdentityResult addUserResult;

            addUserResult = await userManager.CreateAsync(user, password);
            var result = new CreateUserResultType();
            result.IdentityResult = addUserResult;
            if (addUserResult.Succeeded)
            {                
                try
                {
                    //Add User to role
                    var role = (isAdmin) ? "admin" : "user";
                    await userManager.AddToRoleAsync(user, role);
                    //Add relation between Applications and Users
                    var UsersApplication = new UsersApplications()
                    {
                        Applications = db.Applications.Where(c => c.AppId == appId).FirstOrDefault(),
                        ApplicationUser = user
                    };
                    db.UserApplications.Add(UsersApplication);
                    await db.SaveChangesAsync();
                    result.Success = true;                    
                    return result;
                }catch(Exception ex)
                {
                    result.Success = false;
                    result.Message = ex.Message;
                }
            }else if (addUserResult.Errors.Where(e => e.Code == "DuplicateUserName").Any())
            {

            }
            result.Success = false;
            result.Message = ErrorHelper.GetErrors(addUserResult, localizer);
            return result;
        }
    }
}
