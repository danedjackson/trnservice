
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using trnservice.Areas.Identity.Data;

namespace trnservice.Data
{
    public class AuthDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<ApplicationPermission> Permissions { get; set; }
        public DbSet<ApplicationRolePermissions> RolePermissions { get; set; }

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

            // Configure many-to-many relationship
            builder.Entity<ApplicationRolePermissions>()
                .HasKey(rp => new { rp.PermissionId, rp.RoleId });

            builder.Entity<ApplicationRolePermissions>()
                .HasOne(rp => rp.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(rp => rp.PermissionId);

            builder.Entity<ApplicationRolePermissions>()
                .HasOne(rp => rp.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(rp => rp.RoleId);
        }
    }
}
