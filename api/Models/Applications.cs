using System;

namespace IdentityService.Models
{
    public class Applications
    {
        public int Id { get; set; }
        public int AppId { get; set; }
        public string Nombre { get; set; }
        public string Url { get; set; }
        public string HomePage { get; set; }
        public bool Disable { get; set; }      
        public string AppToken { get; set; }

        public Applications()
        {
            var rnd = new Random();
            AppId = rnd.Next(200, 1000);
            Disable = false;
        }
    }
}
