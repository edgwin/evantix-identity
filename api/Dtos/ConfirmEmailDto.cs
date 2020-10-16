namespace IdentityService.Dtos
{
    public class ConfirmEmailDto
    {
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; }
    }
}
