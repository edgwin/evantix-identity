using Google.Apis.Auth;
using IdentityService.Enums;
using IdentityService.ExtensionMethods;
using IdentityService.ExternalProvider;
using IdentityService.Models;
using IdentityService.ResultTypes;
using IdentityService.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

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

        public async Task<CreateSocialUserResultType> ExternalLoginAsync<T>(SocialMediaEnum socialMedia, string code, ApiDbContext _db, IStringLocalizer<T> localizer, int AppId)
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
        public async Task<CreateSocialUserResultType> ExternalLoginAsync<T>(SocialMediaEnum socialMedia, ExternalProviderLoginResource LoginResource, ApiDbContext _db, IStringLocalizer<T> localizer)
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
                case (SocialMediaEnum.Google):
                    {
                        if (string.IsNullOrEmpty(LoginResource.Token))
                            throw new Exception("Token is null or empty");

                        if (LoginResource.AppId == 0)
                            throw new Exception("AppId was not provided");

                        var payload = await GoogleJsonWebSignature.ValidateAsync(LoginResource.Token);
                        User = new ExternalProviderUserResource()
                        {
                            Email = payload.Email,
                            FirstName = payload.GivenName,
                            LastName = payload.FamilyName,
                            Picture = payload.Picture                            
                        };
                        break;
                    }
            }
            return await SaveUser(User, _db, LoginResource.AppId, localizer);            
        }

        private async Task<CreateSocialUserResultType> SaveUser<T>(ExternalProviderUserResource User, ApiDbContext _db, int AppId, IStringLocalizer<T> localizer)
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
                    IsEnabled = true,
                    Picture = User.Picture
                };
                //Ver la mejor manera de agregar el usuario de facebook a la base de datos
                var password = HashPassword(GenerateRandomString(32));
                // TODO cuando se pruebe el external login verificar el role del usuario
                result = await _userManager.CreateUserAsync(_db, user, password, "User", AppId, localizer);
                if (!result.Success)
                    return new CreateSocialUserResultType()
                    {
                        Ok = false,
                        Message = result.Message,
                        Value = null
                    };
            }
            domainUser = await _userManager.FindByEmailAsync(User.Email);
            var appToken = _db.UserApplications.Where(up => up.ApplicationUser.Id == domainUser.Id)
                           .Include(a => a.Applications).ToList().Where(c => c.Applications.AppId == AppId).FirstOrDefault().Applications.AppToken;

            return new CreateSocialUserResultType()
            {
                Ok = true,
                Message = result.Message,
                Value = GetLoginToken.Execute(domainUser, _db, AppId, appToken)
            };
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
