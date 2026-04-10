using System;
using System.ComponentModel.DataAnnotations;

namespace IdentityService.Models
{
    public class RefreshToken
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string UserId { get; set; }

        [Required]
        [MaxLength(256)]
        public string Token { get; set; }

        public DateTime ExpiresUtc { get; set; }

        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Null until revoked or rotated. Revoked tokens cannot be reused.
        /// </summary>
        public DateTime? RevokedUtc { get; set; }

        public bool IsExpired => DateTime.UtcNow >= ExpiresUtc;
        public bool IsRevoked => RevokedUtc != null;
        public bool IsActive => !IsRevoked && !IsExpired;

        // Navigation
        public ApplicationUser User { get; set; }
    }
}
