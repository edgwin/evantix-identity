using System;

namespace IdentityService.Utils
{
    public class LoginResponseData
    {
        public string access_token { get; set; }
        public string refresh_token { get; set; }
        public int expires_in { get; set; }
        public string userName { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public int appId { get; set; }
        public string appHomePage { get; set; }
        public string appName { get; set; }
        public string role { get; set; }
    }
}
