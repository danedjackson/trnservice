﻿using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace trnservice.Areas.Identity.Data
{
    public class AlternativeDbContext : IdentityDbContext<ApplicationUser>
    {
        public AlternativeDbContext(DbContextOptions<AlternativeDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);

            //Setting up many-to-many relationship with Platform and User
            builder.Entity<ApplicationPlatformUser>()
                .HasKey(pu => new { pu.PlatformId, pu.UserId });
        }
    }
}
