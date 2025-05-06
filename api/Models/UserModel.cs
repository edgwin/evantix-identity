using IdentityService.Enums;
using System;
using System.Text.Json.Serialization;

namespace IdentityService.Models
{
    public class UserModel
    {        
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int AppId { get; set; }        
        public bool IsEnabled { get; set; }
        public string Password { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public RolesEnum Role { get; set; }
        public string ConfirmUrl { get; set; }
        public bool IsSocialUser { get; set; } = false;
        public string SocialPicture { get; set; } = string.Empty;
        public string Picture { get; set; }

        public UserModel()
        {
            Picture = "images/User.png";
        }
    }
}
