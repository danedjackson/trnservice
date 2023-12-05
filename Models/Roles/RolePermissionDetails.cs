using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using trnservice.Areas.Identity.Data;

namespace trnservice.Models.Roles
{
    public class RolePermissionDetails
    {
        public IdentityRole Role { get; set; }
        public List<ApplicationPermission> AssignedPermissions { get; set; }
        public List<ApplicationPermission> UnassignedPermissions { get; set; }
    }
}
