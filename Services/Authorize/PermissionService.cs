using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using trnservice.Areas.Identity.Data;
using trnservice.Data;

namespace trnservice.Services.Authorize
{
    public class PermissionService : IPermissionService
    {
        private readonly RoleManager<ApplicationRole> _roleManager;

        public PermissionService(RoleManager<ApplicationRole> roleManager)
        {
            _roleManager = roleManager;
        }
        // This does the processing for our HasPermission annotation
        public async Task<bool> GetPermissionsAsync(string userRole, string queryPermission)
        {
            // Fetch current user role
            ApplicationRole roleDetails = await _roleManager.Roles
                            // Eager loading grabs relationship object
                            .Include(role => role.RolePermissions)
                                // Repeating to grab permission data to retrieve Name
                                .ThenInclude(rolePermission => rolePermission.Permission)
                            .Where(role => role.Name == userRole)
                            .FirstOrDefaultAsync();

            // Check if user has permissions according to their current role
            return roleDetails.RolePermissions.Any(role => role.Permission.Name == queryPermission);

        }
    }
}
