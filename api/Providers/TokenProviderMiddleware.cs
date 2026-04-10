using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using IdentityService.Models;
using IdentityService.Utils;
using System.Linq;

namespace IdentityService.Providers
{
    public class TokenProviderMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly JsonSerializerSettings _serializerSettings;

        public TokenProviderMiddleware(
            RequestDelegate next)
        {
            _next = next;
            _serializerSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
            };
        }

        public Task Invoke(HttpContext context)
        {
            // If the request path doesn't match, skip
            if (!context.Request.Path.Equals("/api/token", StringComparison.Ordinal))
            {
                return _next(context);
            }

            // Request must be POST with Content-Type: application/x-www-form-urlencoded
            if (!context.Request.Method.Equals("POST")
               || !context.Request.HasFormContentType)
            {
                context.Response.StatusCode = 400;
                return context.Response.WriteAsync("Bad request.");
            }


            return GenerateToken(context);
        }

        private async Task GenerateToken(HttpContext context)
        {
            try
            {
                var username = context.Request.Form["username"].ToString();
                var password = context.Request.Form["password"];
                var appId    = Convert.ToInt32(context.Request.Form["appId"].ToString());

                var signInManager = context.RequestServices.GetService<SignInManager<ApplicationUser>>();
                var userManager = context.RequestServices.GetService<UserManager<ApplicationUser>>();

                var result = await signInManager.PasswordSignInAsync(username, password, false, lockoutOnFailure: true);
                if (result.IsLockedOut)
                {
                    context.Response.StatusCode = 429;
                    await context.Response.WriteAsync("Account locked. Too many failed attempts. Try again in 15 minutes.");
                    return;
                }
                if (!result.Succeeded)
                {
                    context.Response.StatusCode = 400;
                    await context.Response.WriteAsync("Invalid username or password.");
                    return;
                }
                var user = await userManager.Users
                    .SingleAsync(i => i.UserName == username);
                if (!user.IsEnabled)
                {
                    context.Response.StatusCode = 400;
                    await context.Response.WriteAsync("Invalid username or password.");
                    return;
                }
                var db = context.RequestServices.GetService<ApiDbContext>();
                var appToken = db.UserApplications.Where(up => up.ApplicationUser.Id == user.Id)
                           .Include(a => a.Applications).ToList().Where(c => c.Applications.AppId == appId).FirstOrDefault().Applications.AppToken;
                var response = GetLoginToken.Execute(user, db, appId, appToken);

                // Serialize and return the response
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(JsonConvert.SerializeObject(response, _serializerSettings));
            }
            catch (Exception ex)
            {
                //TODO log error
                //Logging.GetLogger("Login").Error("Erorr logging in", ex);
            }
        }

    }

}
