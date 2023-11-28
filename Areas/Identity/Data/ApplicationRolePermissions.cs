using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace trnservice.Areas.Identity.Data
{
    public class ApplicationRolePermissions
    {
        public int PermissionId { get; set; }
        public ApplicationPermission Permission { get; set; }

        public string RoleId { get; set; }
        public ApplicationRole Role { get; set; }
    }
}
