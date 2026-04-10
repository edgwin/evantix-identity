using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityService.Utils
{
    public static class Configuration
    {
        public static IConfigurationRoot Config { get; set; }

        static Configuration()
        {
            // Lee la configuración en el mismo orden que .NET:
            // 1. appsettings.json (base, valores por defecto)
            // 2. appsettings.{Environment}.json (sobreescribe por entorno)
            // 3. Variables de entorno (sobreescribe todo — usado en VPS producción)
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables();

            Config = builder.Build();
        }

        public static string DbConnection => Config["DefaultConnection"];
    }
}
