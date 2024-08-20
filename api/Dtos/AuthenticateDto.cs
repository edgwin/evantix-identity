namespace IdentityService.Dtos
{
    public class AuthenticateDto
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public int AppId { get; set; }
    }
}
