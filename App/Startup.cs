using App.Data;
using App.IocConfig;
using App.ViewModels.DynamicAccess;
using App.ViewModels.Settings;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace App
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public IServiceProvider Services { get; }
        private readonly SiteSettings SiteSettings;
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            SiteSettings = configuration.GetSection(nameof(SiteSettings)).Get<SiteSettings>();
        }
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {

            services.Configure<SiteSettings>(Configuration.GetSection(nameof(SiteSettings)));
            services.AddDbContext<AppDBContext>(options => options.UseSqlServer(Configuration.GetConnectionString("SqlServer")));
            services.AddCustomServices();
            services.AddCustomIdentityServices();
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            services.ConfigureWritable<SiteSettings>(Configuration.GetSection("SiteSettings"));
         

            services.ConfigureApplicationCookie(options =>
            {
                // Cookie settings
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(3600);
                options.LoginPath = "/Admin/Manage/SignIn";
                options.LogoutPath = "/Admin/Manage/SignOut";
                options.AccessDeniedPath = "/Admin/Manage/AccessDenied";
                options.SlidingExpiration = true;
            });
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie();
            services.AddAuthorization(options =>
            { 
                options.AddPolicy(ConstantPolicies.DynamicPermission, policy => policy.Requirements.Add(new DynamicPermissionRequirement()));
            });
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            var cachePeriod = env.IsDevelopment() ? "600" : "605800";
            app.UseWhen(context => !context.Request.Path.StartsWithSegments("/api"), appBuilder =>
            {
                if (env.IsDevelopment())
                    appBuilder.UseDeveloperExceptionPage();
                else
                    appBuilder.UseExceptionHandler("/Home/Error");
            });

           
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "CacheFiles")),
                OnPrepareResponse = ctx =>
                {
                    ctx.Context.Response.Headers.Append("Cache-Control", $"public,max-age={cachePeriod}");
                },
                RequestPath = "/CacheFiles",
            });


            app.UseStaticFiles();

            app.UseCustomIdentityServices();

            app.Use(async (context, next) =>
            {
                await next();
                if (context.Response.StatusCode == 404)
                {
                    context.Request.Path = "/home/error404";
                    await next();
                }
            });
          
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
              
                endpoints.MapControllerRoute(
                      name: "areas",
                pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
                 );

                endpoints.MapControllerRoute(
               name: "default",
               pattern: "{controller=Home}/{action=Index}/{id?}"
                );

            });
        }
    }
}
