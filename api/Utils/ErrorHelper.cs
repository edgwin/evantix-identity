using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;

namespace IdentityService.Utils
{
    public static class ErrorHelper
    {
        public static string GetErrors<T>(IdentityResult errors, IStringLocalizer<T> localizer)
        {
            string errorString = localizer["Ocurrieron los siguientes errores:"].Value;
            errorString += "\n";
            if (errors != null)
            {
                foreach (var error in errors.Errors)
                {
                    errorString += error.Description + "\n";
                }
            }
            return errorString;
        }
    }
}
