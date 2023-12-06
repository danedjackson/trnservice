using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace trnservice.Models.Roles
{
    public class RolePermissionModification
    {
        [Required]
        public string RoleName { get; set; }
        [Required]
        public string RoleId { get; set; }
#nullable enable
        public int[]? PermissionIdsToAdd { get; set; }

        public int[]? PermissionIdsToDelete { get; set; }
#nullable disable
    }
}
