using IdentityService.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using System.Security.Claims;

namespace IdentityService.Utils
{
    public class UserUtils
    {
        private static UserManager<ApplicationUser> _userManager;

        public UserUtils(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }
        public static string GetUserId(HttpContext context)
        {
            return context.User.Claims.First(i => i.Type == ClaimTypes.NameIdentifier).Value;
        }

        public static ApplicationUser GetUser(HttpContext context, UserManager<ApplicationUser> userManager)
        {
            var userId = GetUserId(context);
            return userManager.Users
                .Single(i => i.Id == userId);
        }       
    }
}
