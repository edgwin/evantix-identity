namespace IdentityService.ResultTypes
{
    public class UserResultType
    {
        public string access_token { get; set; }
        public string refresh_token { get; set; }
        public int expires_in { get; set; }
        public int appId { get; set; }
        public string appHomePage { get; set; }
        public string appName { get; set; }
        public UserResult User { get; set; }
        
    }

    public class UserResult
    {
        public string userId { get; set; }
        public string userName { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string role { get; set; }
        public string email { get; set; }
        public string picture { get; set; }
        public bool isSocial { get; set; } = false;
    }
}
