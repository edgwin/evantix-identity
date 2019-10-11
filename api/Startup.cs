using IdentityService.Models;
using IdentityService.Providers;
using IdentityService.Utils.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IdentityService.Services;
using Microsoft.AspNetCore.Authentication.OAuth;

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
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", builder =>
                {
                    builder.AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowAnyOrigin()
                    .AllowCredentials();
                });
            });

            //Resources Localization
            services.AddLocalization(o =>
            {
                // We will put our translations in a folder called Resources
                o.ResourcesPath = "Resources";
            });

            services.AddMvc()
                .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
                .AddDataAnnotationsLocalization();

            var currentCulture = Thread.CurrentThread.CurrentCulture;

            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequireNonAlphanumeric = false;
            })
            .AddEntityFrameworkStores<ApiDbContext>()
            .AddDefaultTokenProviders()
            .AddErrorDescriber<SpanishIdentityErrorDescriber>();

            var efConnection = Configuration["DefaultConnection"];
            services.AddDbContext<ApiDbContext>(options => options.UseSqlServer(efConnection));

            // return 401 instead of redirect to login
            services.ConfigureApplicationCookie(options => {
                options.Events.OnRedirectToLogin = context => {
                    context.Response.Headers["Location"] = context.RedirectUri;
                    context.Response.StatusCode = 401;
                    return Task.CompletedTask;
                };
            });

            services.AddAuthentication(sharedOptions =>
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
                            Console.WriteLine("OnTokenValidated: " +
                                              context.SecurityToken);
                            return Task.CompletedTask;
                        }
                    };

                })
                .AddFacebook(options =>
                {
                    options.AppId = "353994368862998";
                    options.AppSecret = "897b7284048a0bbe15c7e6553d0acc82";
                })
                .AddLinkedIn(options => {
                    options.ClientId = "78p1nrxr7qwobe";
                    options.ClientSecret = "y461OtLEqUHnrvbT";                                  
                });
            //.AddLinkedIn(options => {
            //    options.ClientId = "78p1nrxr7qwobe";
            //    options.ClientSecret = "y461OtLEqUHnrvbT";
            //});

            services.AddScoped<IPasswordHistory, Utils.PasswordHistory>();
            services.AddScoped<AccountService, AccountService>();
            services.AddScoped<FacebookService, FacebookService>();
            services.AddScoped<SharedResource, SharedResource>();
            services.AddScoped<LinkedInService, LinkedInService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        // NOTE: DI is done here
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            app.UseCors("CorsPolicy");

            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

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
            app.UseMvc(routes =>
            {
                routes.MapRoute("default", "{controller=Home}/{action=Index}/{id?}");
            });
        }

    }
}
