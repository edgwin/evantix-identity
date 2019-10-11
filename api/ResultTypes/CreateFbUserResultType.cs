using IdentityService.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityService.ResultTypes
{
    public class CreateFbUserResultType : ResultType
    {
        public LoginResponseData Value { get; set; }
    }
}
