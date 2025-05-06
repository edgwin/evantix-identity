using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityService.Dtos
{
    public class EmailRequestDto
    {   
        public string Email { get; set; }
        public int AppId { get; set; }
    }
}
