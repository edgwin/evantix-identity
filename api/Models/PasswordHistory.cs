using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace IdentityService.Models
{
    public class PasswordHistory
    {
        public PasswordHistory()
        {
            CreateDate = DateTime.Now;
        }
        public PasswordHistory(string userId, string passwordHash)
        {
            UserId = userId;
            PasswordHash = passwordHash;
            CreateDate = DateTime.Now;
        }

        [Column(Order = 0)]
        public string UserId { get; set; }
        [Column(Order = 1)]
        public string PasswordHash { get; set; }
        public DateTime CreateDate { get; set; }
        public int AppId { get; set; }
    }
}
