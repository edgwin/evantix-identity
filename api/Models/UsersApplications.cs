using System.ComponentModel.DataAnnotations.Schema;

namespace IdentityService.Models
{
    public class UsersApplications
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public ApplicationUser ApplicationUser { get; set; }
        public Applications Applications { get; set; }
    }
}