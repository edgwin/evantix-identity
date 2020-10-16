using System;
using System.ComponentModel.DataAnnotations;

namespace IdentityService.ExternalProvider
{
    public class ExternalProviderLoginResource
    {
        [Required]
        [StringLength(255)]
        public string Token { get; set; }
        public int AppId { get; set; }
    }
}
