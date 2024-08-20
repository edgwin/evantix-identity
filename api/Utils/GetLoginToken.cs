
using IdentityService.Models;
using IdentityService.Providers;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace IdentityService.Utils
{
    public static class GetLoginToken
    {
        public static TokenProviderOptions GetOptions()
        {
            var signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Configuration.Config.GetSection("TokenAuthentication:SecretKey").Value));

            return new TokenProviderOptions
            {
                Path = Configuration.Config.GetSection("TokenAuthentication:TokenPath").Value,
                Audience = Configuration.Config.GetSection("TokenAuthentication:Audience").Value,
                Issuer = Configuration.Config.GetSection("TokenAuthentication:Issuer").Value,
                Expiration = TimeSpan.FromMinutes(Convert.ToInt32(Configuration.Config.GetSection("TokenAuthentication:ExpirationMinutes").Value)),
                SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256)
            };
        }

        public static LoginResponseData Execute(ApplicationUser user, ApiDbContext _db, int appId, string appToken)
        {
            var options = GetOptions();
            var now = DateTime.UtcNow;

            var claims = new List<Claim>()
            {
                new Claim(JwtRegisteredClaimNames.NameId, user.Id),
                new Claim(JwtRegisteredClaimNames.Iat, new DateTimeOffset(now).ToUniversalTime().ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
                new Claim(JwtRegisteredClaimNames.Email, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, appToken),
            };

            var userClaims = _db.UserClaims.Where(i => i.UserId == user.Id).ToList();
            foreach (var userClaim in userClaims)
            {
                claims.Add(new Claim(userClaim.ClaimType, userClaim.ClaimValue));
            }
            var userRoles = _db.UserRoles.Where(i => i.UserId == user.Id).ToList();
            foreach(var userRole in userRoles)
            {
                var role = _db.Roles.Where(i => i.Id == userRole.RoleId).FirstOrDefault();
                claims.Add(new Claim(Extensions.RoleClaimType, role.Name));              
            }
            
            var jwt = new JwtSecurityToken(
                issuer: options.Issuer,
                audience: options.Audience,
                claims: claims.ToArray(),
                notBefore: now,
                expires: now.Add(options.Expiration),
                signingCredentials: options.SigningCredentials);
            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);
            
            var applicationInfo = _db.UserApplications.Where(up => up.ApplicationUser.Id == user.Id)
                .Include(a => a.Applications).ToList().Where(c => c.Applications.AppId == appId).FirstOrDefault();            
            var response = new LoginResponseData
            {
                access_token = encodedJwt,
                expires_in = (int)options.Expiration.TotalSeconds,
                userName = user.UserName,
                firstName = user.FirstName,
                lastName = user.LastName,
                appId = applicationInfo.Applications.AppId,
                appHomePage = applicationInfo.Applications.HomePage,
                appName = applicationInfo.Applications.Nombre,
                role = claims.Where(x => x.Type == Extensions.RoleClaimType).Select(i => i.Value).FirstOrDefault()
            };
            return response;
        }
    }

}
