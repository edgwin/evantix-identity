namespace IdentityService.Models
{
    public class UserModel
    {        
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string CellPhone { get; set; }
        public int AppId { get; set; }        
        public bool IsEnabled { get; set; }
        public string Password { get; set; }
        public bool IsAdmin { get; set; }
    }
}
