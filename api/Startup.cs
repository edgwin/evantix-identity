using IdentityService.Models;
using IdentityService.Providers;
using IdentityService.Services;
using IdentityService.Utils.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IdentityService
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        public static string CurrentEnvironment { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // ── Validación de configuración requerida al inicio ────────────────────────
            var requiredSettings = new[]
            {
                "TokenAuthentication:SecretKey",
                "DefaultConnection",
            };
            var missingSettings = requiredSettings
                .Where(key => string.IsNullOrWhiteSpace(Configuration[key]))
                .ToList();

            if (missingSettings.Any())
            {
                var env = CurrentEnvironment ?? "Unknown";
                var missing = string.Join("\n  - ", missingSettings);
                throw new InvalidOperationException(
                    $"\n\n⚠️  IDENTITY — CONFIGURACIÓN INCOMPLETA (entorno: {env})\n" +
                    $"Los siguientes valores son requeridos y están vacíos:\n  - {missing}\n\n" +
                    $"En desarrollo: configúralos en appsettings.Development.json\n" +
                    $"En producción: configúralos en el archivo de variables de entorno del servicio systemd\n");
            }
            // ────────────────────────────────────────────────────────────────────────────

            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", builder =>
                {
                    builder.AllowAnyHeader()
                    .AllowAnyMethod()
                    .SetIsOriginAllowed(origin => true)
                    .AllowCredentials();
                });
            });

            //Resources Localization
            services.AddLocalization(o =>
            {
                // We will put our translations in a folder called Resources
                o.ResourcesPath = "Resources";
            });

            services.AddMvc(option => option.EnableEndpointRouting = false)
                .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
                .AddDataAnnotationsLocalization();

            var currentCulture = Thread.CurrentThread.CurrentCulture;

            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 8;
            })
            .AddEntityFrameworkStores<ApiDbContext>()
            .AddTokenProvider<DataProtectorTokenProvider<ApplicationUser>>("EmailConfirm")
            .AddDefaultTokenProviders()
            .AddErrorDescriber<SpanishIdentityErrorDescriber>();

            var efConnection = Configuration["DefaultConnection"];
            services.AddDbContext<ApiDbContext>(options => options.UseNpgsql(efConnection)
                .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning)));
            services.AddHttpClient();

            // return 401 instead of redirect to login
            services.ConfigureApplicationCookie(options => {
                options.Events.OnRedirectToLogin = context => {
                    context.Response.Headers["Location"] = context.RedirectUri;
                    context.Response.StatusCode = 401;
                    return Task.CompletedTask;
                };
            });

            // ── Social login: registrar solo si están configurados ─────────────────────
            // Evita errores de startup en entornos donde no se usa social login
            var facebookAppId = Configuration["FacebookAuth:AppId"];
            var linkedInClientId = Configuration["LinkedInAuth:ClientId"];

            var authBuilder = services.AddAuthentication(sharedOptions =>
            {
                sharedOptions.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                sharedOptions.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                .AddJwtBearer(cfg =>
                {
                    cfg.RequireHttpsMetadata = false;

                    cfg.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = Configuration["TokenAuthentication:Issuer"],
                        ValidAudience = Configuration["TokenAuthentication:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Configuration["TokenAuthentication:SecretKey"]))
                    };

                    cfg.Events = new JwtBearerEvents
                    {
                        OnAuthenticationFailed = context =>
                        {
                            Console.WriteLine("OnAuthenticationFailed: " + context.Exception.Message);
                            return Task.CompletedTask;
                        },
                        OnTokenValidated = context =>
                        {
                            Console.WriteLine("OnTokenValidated: " + context.SecurityToken);
                            return Task.CompletedTask;
                        }
                    };
                });

            if (!string.IsNullOrEmpty(facebookAppId))
            {
                authBuilder.AddFacebook(options =>
                {
                    options.AppId = Configuration["FacebookAuth:AppId"];
                    options.AppSecret = Configuration["FacebookAuth:AppSecret"];
                });
            }

            if (!string.IsNullOrEmpty(linkedInClientId))
            {
                authBuilder.AddLinkedIn(options =>
                {
                    options.ClientId = Configuration["LinkedInAuth:ClientId"];
                    options.ClientSecret = Configuration["LinkedInAuth:ClientSecret"];
                });
            }
            // ────────────────────────────────────────────────────────────────────────────

            
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "Identity API",
                    Version = "v1"
                });
            });

            services.AddScoped<IPasswordHistory, Utils.PasswordHistory>();
            services.AddScoped<AccountService, AccountService>();
            services.AddScoped<FacebookService, FacebookService>();
            services.AddScoped<SharedResource, SharedResource>();
            services.AddScoped<LinkedInService, LinkedInService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        // NOTE: DI is done here
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            app.UseCors("CorsPolicy");

            //loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            //loggerFactory.AddDebug();

            CurrentEnvironment = env.EnvironmentName;
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //Resources Localization
            var supportedCultures = new[]
            {
                new CultureInfo("es-MX"),
                new CultureInfo("en-US"),
            };

            app.UseRequestLocalization(new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture("es-MX"),                
                SupportedCultures = supportedCultures,                
                SupportedUICultures = supportedCultures
            });

            app.UseStaticFiles();            
            app.UseMiddleware<TokenProviderMiddleware>();
            app.UseAuthentication();
            app.UseSwagger(c =>
            {
                c.OpenApiVersion = Microsoft.OpenApi.OpenApiSpecVersion.OpenApi3_0;
            });
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Identity API");
                c.RoutePrefix = "help"; // Set Swagger UI at the app's root (localhost:5000/)
            });
            app.UseMvc(routes =>
            {
                routes.MapRoute("default", "{controller=Home}/{action=Index}/{id?}");
            });
            app.UseStaticFiles();
        }

    }
}
