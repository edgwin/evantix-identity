using IdentityService.Utils;

namespace IdentityService.ResultTypes
{
    public class CreateSocialUserResultType : ResultType
    {  
        public LoginResponseData Value { get; set; }
    }
}
