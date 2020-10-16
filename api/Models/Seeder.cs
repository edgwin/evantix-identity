using IdentityService.Utils;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityService.Models
{
    public class Seeder
    {
        private ApiDbContext _context;

        public Seeder(ApiDbContext context)
        {
            _context = context;
        }

        public void SeedData()
        {
            var HomePage = string.Empty;
            var Url = string.Empty;
            var appToken = "C8C1A19531B77DBD6CC255F9ADB52";
            switch (Startup.CurrentEnvironment)
            {
                case "Development":
                default:
                    {
                        HomePage = Configuration.Config.GetSection("ApplicationNameSettings:HomeUrl").Value;
                        Url = Configuration.Config.GetSection("ApplicationNameSettings:Url").Value;
                        break;
                    }
            }
            var rnd = new Random();
            AddNewType(new Applications { AppId = rnd.Next(100,1000), Nombre = "IBusness", HomePage = HomePage, Url = Url, AppToken = appToken });
            AddNewRole(new IdentityRole { Id = Guid.NewGuid().ToString(), Name = "comerciante", NormalizedName = "COMERCIANTE", ConcurrencyStamp = Guid.NewGuid().ToString() });
            _context.SaveChanges();
        }

        private void AddNewType(Applications App)
        {
            //var existingType = _context.Applications.FirstOrDefault(p => p.Nombre == App.Nombre);
            if (!_context.Applications.Where(c => c.Nombre == App.Nombre).Any())
            {
                _context.Applications.Add(App);
            }
        }

        private void AddNewRole(IdentityRole App)
        {
            //var existingType = _context.Applications.FirstOrDefault(p => p.Nombre == App.Nombre);
            if (!_context.Roles.Where(c => c.NormalizedName == App.NormalizedName).Any())
            {
                _context.Roles.Add(App);
            }
        }
    }
}
