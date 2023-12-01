using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using trnservice.Areas.Identity.Data;
using trnservice.Data;

[assembly: HostingStartup(typeof(trnservice.Areas.Identity.IdentityHostingStartup))]
namespace trnservice.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) => {
                services.AddDbContext<AuthDbContext>(options => {
                    options.UseSqlServer(
                        context.Configuration.GetConnectionString("AuthDbContextConnection"));
                    options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
                    });


                services.AddDefaultIdentity<ApplicationUser>(options => 
                {
                    //TODO: Change this when email confirmation is added
                    options.SignIn.RequireConfirmedAccount = false;
                    options.Password.RequireDigit = false;
                    options.Password.RequireUppercase = false;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Lockout.MaxFailedAccessAttempts = 3;
                    options.Lockout.DefaultLockoutTimeSpan = new TimeSpan(12, 0, 0);
                })
                    .AddRoles<ApplicationRole>()
                    .AddEntityFrameworkStores<AuthDbContext>();
            });
        }
    }
}