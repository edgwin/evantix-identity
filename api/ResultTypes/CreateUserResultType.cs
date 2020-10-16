using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityService.ResultTypes
{
    public class CreateUserResultType
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public IdentityResult IdentityResult { get; set; }
        public bool IsDuplicated { get; set; }

        public CreateUserResultType()
        {
            Message = string.Empty;
            IsDuplicated = false;
        }
    }
}
