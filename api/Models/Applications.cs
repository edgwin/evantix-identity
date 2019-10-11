using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityService.Models
{
    public class Applications
    {
        public int AppId { get; set; }
        public string Nombre { get; set; }
        public string Url { get; set; }
        public string HomePage { get; set; }
        public bool Disable { get; set; }        

        public Applications()
        {           
            Disable = false;
        }
    }
}
