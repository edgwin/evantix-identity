using System.Threading.Tasks;

namespace IdentityService.Utils.Interfaces
{
    public interface IPasswordHistory
    {
        Task<bool> SavePassword(Models.PasswordHistory PasswordHistory);
        bool PasswordAlreadyExists(string userId, string newPassword);
    }
}
