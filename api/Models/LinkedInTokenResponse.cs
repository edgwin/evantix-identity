using Newtonsoft.Json;

namespace IdentityService.Models
{
    public class LinkedInTokenResponse
    {
        [JsonProperty(PropertyName = "access_token")]
        public string Access_token { get; set; }

        [JsonProperty(PropertyName = "expires_in")]
        public int Expires_in { get; set; }
    }
}
