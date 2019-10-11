using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityService.Utils
{
    public class UrlHelper
    {
        public static string GetUrlAuthority(HttpContext context)
        {
            //Cambiar por la url del webapi, ya que el identity se llamara por medio del api principal
            return context.Request.Host != null ? $"{context.Request.Scheme}://{context.Request.Host}" : "localhost";
        }
    }
}
