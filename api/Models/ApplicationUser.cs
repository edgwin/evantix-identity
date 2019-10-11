using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace IdentityService.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string CellPhone { get; set; }        
        public bool IsEnabled { get; set; }
    }
}
