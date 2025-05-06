using IdentityService.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace IdentityService.ExternalProvider
{
    public class ExternalProviderLoginResource
    {
        [Required]
        [StringLength(255)]
        public string Token { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public RolesEnum Role { get; set; }
        public int AppId { get; set; }
    }
}
