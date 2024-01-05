
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using trnservice.Areas.Identity.Data;

namespace trnservice.Data
{
    public class AuthDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<ApplicationPermission> Permissions { get; set; }
        public DbSet<ApplicationRolePermission> RolePermissions { get; set; }

        // Manage Application Authentication
        public DbSet<ApplicationPlatform> Platforms { get; set; }
        public DbSet<ApplicationPlatformUser> PlatformUsers { get; set; }

        public AuthDbContext(DbContextOptions<AuthDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);

            // Configure many-to-many relationships
            // Specify that the primary key of the AppplicationRolePermission table consists 
            // of a composite key made up of both the PermissionId and RoleId
            builder.Entity<ApplicationRolePermission>()
                .HasKey(rp => new { rp.PermissionId, rp.RoleId });

            // Foreign key configuration. HasOne and WithMany are used to define the foreign key
            // for the Permission side of the relationship
            builder.Entity<ApplicationRolePermission>()
                .HasOne(rp => rp.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(rp => rp.PermissionId);

            // Foreign key configuration. HasOne and WithMany are used to define the foreign key
            // for the Role side of the relationship
            builder.Entity<ApplicationRolePermission>()
                .HasOne(rp => rp.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(rp => rp.RoleId);


            //Setting up many-to-many relationship with Platform and User
            builder.Entity<ApplicationPlatformUser>()
                .HasKey(pu => new { pu.PlatformId, pu.UserId });

            builder.Entity<ApplicationPlatformUser>()
                .HasOne(pu => pu.Platform)
                .WithMany(p => p.PlatformUsers)
                .HasForeignKey(pu => pu.PlatformId);

            builder.Entity<ApplicationPlatformUser>()
                .HasOne(pu => pu.User)
                .WithMany(u => u.PlatformUsers)
                .HasForeignKey(pu => pu.UserId);
        }
    }
}
