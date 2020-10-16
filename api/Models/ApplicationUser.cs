using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Linq;

namespace IdentityService.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string CellPhone { get; set; }        
        public bool IsEnabled { get; set; }

        public bool IsEmailConfirmed(ApiDbContext db, int appId) 
        {
            return db.UserApplications.Where(app => app.Applications.AppId == appId && app.ApplicationUser.Id == Id).Select(app => app.EmailConfirmed).FirstOrDefault();
            
        }
    }
}
