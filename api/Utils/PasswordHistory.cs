using IdentityService.Models;
using IdentityService.Utils.Interfaces;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityService.Utils
{
    public class PasswordHistory : IPasswordHistory
    {
        private readonly ApiDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        int PASSWORD_HISTORY_LIMIT;

        public PasswordHistory(ApiDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
            PASSWORD_HISTORY_LIMIT = int.TryParse(Configuration.Config.GetSection("PasswordValues:PasswordLimitReuse").Value, out PASSWORD_HISTORY_LIMIT) ? PASSWORD_HISTORY_LIMIT : 4;
        }

        public virtual async Task<bool> SavePassword(Models.PasswordHistory PasswordHistory)
        {
            try
            {
                _db.PasswordHistory.Add(PasswordHistory);
                await _db.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool PasswordAlreadyExists(string userId, string newPassword)
        {

            var records = _db.PasswordHistory.Where(c => c.UserId == userId).
                            OrderByDescending(c => c.CreateDate).
                            Take(PASSWORD_HISTORY_LIMIT);
            var user = _userManager.Users.SingleOrDefault(uid => uid.Id == userId);
            foreach (var r in records)
            {
                if (_userManager.PasswordHasher.VerifyHashedPassword(user, user.PasswordHash, newPassword) != PasswordVerificationResult.Failed)
                {
                    return true;
                }
            }
            return false;
            
        }
    }
}
