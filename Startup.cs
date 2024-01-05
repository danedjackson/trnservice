﻿using EmailClient;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using trnservice.Areas.Identity.Data;
using trnservice.Services;
using trnservice.Services.Utils;
using trnservice.Services.Authorize;

namespace trnservice
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
            
            // Add Dependency Injection for Service layer 
            services.AddScoped<ITRNService, TRNService>();
            // DI for custom HasPermission Annotation
            services.AddScoped<IPermissionService, PermissionService>();
            services.AddScoped<EmailService>();
            services.AddScoped<Utils>();
            services.AddHttpContextAccessor();
            services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
            services.AddSingleton<IAuthorizationPolicyProvider, 
                PermissionAuthorizationPolicyProvider>();

            services.AddDbContext<AlternativeDbContext>(options => {
                options.UseSqlServer(Configuration.GetConnectionString("AuthDbContextConnection"));
                options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            });

            // Identity
            services.AddRazorPages();

            // Initialize AppSettings with the configuration
            AppSettings.Initialize(Configuration);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseStaticFiles();

            app.UseRouting();

            // Allows cross domain requests
            app.UseCors(cors => cors 
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());

            // Identity
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                // Identity
                endpoints.MapRazorPages();
            });
        }
    }
}
