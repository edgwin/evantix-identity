using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Models
{
    public class ApiDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApiDbContext(DbContextOptions options) : base(options)
        {
        }

        protected ApiDbContext()
        {
        }

        public DbSet<PasswordHistory> PasswordHistory { get; set; }
        public DbSet<Applications> Applications { get; set; }
        public DbSet<UsersApplications> UserApplications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {            
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<PasswordHistory>()
                .HasKey(c => new { c.UserId, c.PasswordHash });
            modelBuilder.Entity<Applications>()
                .HasKey(c => new { c.AppId });
            modelBuilder.Entity<UsersApplications>()
                .HasKey(c => new { c.Id });
        }        
    }

}
