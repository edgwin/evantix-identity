using IdentityService.Utils;
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
            AddNewType(new Applications { Nombre = "iTareas.com", HomePage = HomePage, Url = Url });
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
    }
}
