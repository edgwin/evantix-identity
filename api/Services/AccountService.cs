using IdentityService.ExternalProvider;
using IdentityService.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Threading.Tasks;
using IdentityService.Utils;
using IdentityService.ExtensionMethods;
using Microsoft.Extensions.Localization;
using IdentityService.ResultTypes;
using System.Security.Cryptography;
using System.Text;
using System.Linq;
using IdentityService.Enums;

namespace IdentityService.Services
{
    public class AccountService
    {
        private readonly FacebookService _facebookService;
        private readonly LinkedInService _linkedInService;
        private readonly UserManager<ApplicationUser> _userManager;

        public AccountService(FacebookService facebookService, LinkedInService linkedInService, UserManager<ApplicationUser> userManager)
        {
            _facebookService = facebookService;
            _linkedInService = linkedInService;
            _userManager = userManager;
        }

        public async Task<CreateFbUserResultType> ExternalLoginAsync<T>(SocialMediaEnum socialMedia, string code, ApiDbContext _db, IStringLocalizer<T> localizer, int AppId)
        {
            ExternalProviderUserResource User = null;
            switch (socialMedia)
            {
                case (SocialMediaEnum.LinkedIn):
                {
                    //get token
                    var token = _linkedInService.GetTokenAsync(code);
                    User = await _linkedInService.GetUserFromLinkedInAsync(token.Result);
                    break;
                }
            }

            return await SaveUser(User, _db, AppId, localizer);
        }
        public async Task<CreateFbUserResultType> ExternalLoginAsync<T>(SocialMediaEnum socialMedia, ExternalProviderLoginResource LoginResource, ApiDbContext _db, IStringLocalizer<T> localizer)
        {            
            ExternalProviderUserResource User = null;
            switch (socialMedia)
            {
                case (SocialMediaEnum.Facebook): {
                        if (string.IsNullOrEmpty(LoginResource.Token))
                            throw new Exception("Token is null or empty");

                        if (LoginResource.AppId == 0)
                            throw new Exception("AppId was not provided");
                        User = await _facebookService.GetUserFromFacebookAsync(LoginResource.Token);
                        break;
                    }
            }
            return await SaveUser(User, _db, LoginResource.AppId, localizer);            
        }

        private async Task<CreateFbUserResultType> SaveUser<T>(ExternalProviderUserResource User, ApiDbContext _db, int AppId, IStringLocalizer<T> localizer)
        {
            var domainUser = await _userManager.FindByEmailAsync(User.Email.Trim());
            var result = new CreateUserResultType();
            if (domainUser == null)
            {
                var user = new ApplicationUser()
                {
                    UserName = User.Email,
                    Email = User.Email.Trim(),
                    FirstName = User.FirstName.Trim(),
                    LastName = User.LastName.Trim(),
                    PhoneNumber = string.Empty,
                    CellPhone = string.Empty,
                    IsEnabled = true
                };

                //Ver la mejor manera de agregar el usuario de facebook a la base de datos
                var password = RandomString(6);

                result = await _userManager.CreateUserAsync(_db, user, password, false, AppId, localizer);
                if (!result.Success)
                    return new CreateFbUserResultType()
                    {
                        Ok = false,
                        Message = result.Message,
                        Value = null
                    };
            }
            domainUser = await _userManager.FindByEmailAsync(User.Email);
            return new CreateFbUserResultType()
            {
                Ok = true,
                Message = result.Message,
                Value = GetLoginToken.Execute(domainUser, _db, AppId)
            };
        }

        public static string RandomString(int length)
        {
            const string charsL = "abcdefghijklmnopqrstuvwxyz%&@!_$";
            const string charsU = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            var random = new Random();
            var retVal = new string(Enumerable.Repeat(charsU, 1).Select(s => s[random.Next(s.Length)]).ToArray());
            retVal += new string(Enumerable.Repeat(charsL, length).Select(s => s[random.Next(s.Length)]).ToArray());
            retVal += random.Next(1000,9999).ToString();
            return retVal;
        }
    }
}
