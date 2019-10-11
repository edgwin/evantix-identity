using System.ComponentModel.DataAnnotations;

namespace IdentityService.Resources
{
    public class FacebookLoginResource
    {
        [Required]
        [StringLength(255)]
        public string facebookToken { get; set; }
    }
}
