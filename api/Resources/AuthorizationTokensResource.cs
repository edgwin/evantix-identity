namespace IdentityService.Resources
{
    public class AuthorizationTokensResource
    {
        public TokenResource AccessToken { get; set; }
        public TokenResource RefreshToken { get; set; }
    }
}
