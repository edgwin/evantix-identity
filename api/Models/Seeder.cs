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
            var Apps = Configuration.Config.GetSection("ApplicationNameSettings").GetChildren().ToList();
            Apps.ForEach(app => {
                var appToken = Configuration.Config.GetSection($"ApplicationNameSettings:{app.Key}:AppToken").Value;
                var HomePage = Configuration.Config.GetSection($"ApplicationNameSettings:{app.Key}:HomeUrl").Value;
                var Url = Configuration.Config.GetSection($"ApplicationNameSettings:{app.Key}:Url").Value;
                var AppId = Convert.ToInt32(Configuration.Config.GetSection($"ApplicationNameSettings:{app.Key}:AppId").Value);
                AddNewType(new Applications { AppId = AppId, Nombre = app.Key, HomePage = HomePage, Url = Url, AppToken = appToken });
                //Adding Roles
                var Roles = Configuration.Config.GetSection($"ApplicationNameSettings:{app.Key}:Roles").GetChildren().ToList();
                Roles.ForEach(role => {
                    AddNewRole(new IdentityRole { Id = Guid.NewGuid().ToString(), Name = role.Key.ToLower(), NormalizedName = role.Key.ToUpper(), ConcurrencyStamp = Guid.NewGuid().ToString() });
                });                
            });            
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
