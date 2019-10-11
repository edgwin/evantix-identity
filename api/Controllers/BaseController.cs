using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.Controllers
{
    public class BaseController : Controller
    {
        protected string GetModelErrors()
        {
            string[] arrayErrorString = ModelState.Values.SelectMany(x => x.Errors).Select(e => e.ErrorMessage).ToArray();
            return string.Join(", ", arrayErrorString);
        }
    }
}