using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Memory.API.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SpaServices.Webpack;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.AspNetCore.Server.Kestrel;
using Memory.API.Services;

namespace memory_dotnet
{
    public class Startup
    {
        private IHostingEnvironment _env;

        public Startup(IHostingEnvironment env)
        {
            _env = env;
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            if (env.IsDevelopment())
            {
                builder.AddUserSecrets<Startup>();
            }
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var connectionString = Configuration["connectionStrings:memoryDBConnectionString"];
            if (_env.IsDevelopment())
            { // Only add this local cert if in development
                services.Configure<KestrelServerOptions>(opt =>
                {
                    opt.UseHttps("certificate.pfx", Configuration["Certificates:HTTPS:Password"]);
                });
            }
            services.AddDbContext<MemoryContext>(o => o.UseSqlServer(connectionString));
            services.AddScoped<IMemoryRepository, MemoryRepository>();
            services.AddIdentity<GameUser, IdentityRole>()
                .AddEntityFrameworkStores<MemoryContext>();
            
            services.Configure<MvcOptions>(options =>
            {
                options.Filters.Add(new RequireHttpsAttribute());
            });
            
            services.Configure<IdentityOptions>(config =>
            { // Override Identity redirect to login for API
                config.Cookies.ApplicationCookie.Events = 
                    new CookieAuthenticationEvents()
                    {
                        OnRedirectToLogin = (ctx) =>
                        {
                            if(ctx.Request.Path.StartsWithSegments("/api") && ctx.Response.StatusCode == 200)
                            {
                                ctx.Response.StatusCode = 401;
                            }
                            return Task.CompletedTask;
                        },
                        OnRedirectToAccessDenied = (ctx) =>
                        {
                            if(ctx.Request.Path.StartsWithSegments("/api") && ctx.Response.StatusCode == 200)
                            {
                                ctx.Response.StatusCode = 403;
                            }
                            return Task.CompletedTask;
                        }
                    };

            });
            
            // Add framework services.
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory,
                                MemoryContext memoryContext, UserManager<GameUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();
            var options = new RewriteOptions()
                                .AddRedirectToHttps();

            app.UseRewriter(options);

            if (env.IsDevelopment())
            {
                // app.UseDeveloperExceptionPage();
                app.UseWebpackDevMiddleware(new WebpackDevMiddlewareOptions {
                    HotModuleReplacement = true
                });
            }
            app.UseExceptionHandler(appBuilder => {
                appBuilder.Run(async context=>
                {
                    context.Response.StatusCode = 500;
                    await context.Response.WriteAsync("An unexpected error happened: try again later.");
                });
            });
            app.UseStaticFiles();
            app.UseIdentity();
            AutoMapper.Mapper.Initialize(cfg =>{
                cfg.CreateMap<Memory.API.Entities.GameUser, Memory.API.Models.UserModel>();
                cfg.CreateMap<Memory.API.Models.UserCreateModel,Memory.API.Entities.GameUser>()
                    .ForMember(dest => dest.UserName, opt => opt.MapFrom(src =>
                        $"{src.Email}"));
            });
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");

                routes.MapSpaFallbackRoute(
                    name: "spa-fallback",
                    defaults: new { controller = "Home", action = "Index" });
            });
            // Seed data for testing.
            // if (env.IsDevelopment())
            // {
            //     memoryContext.EnsureSeedDataForContext(userManager, roleManager).Wait();
            // }
        }
    }
}
