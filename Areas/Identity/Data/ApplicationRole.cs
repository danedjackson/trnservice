using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace trnservice.Areas.Identity.Data
{
    public class ApplicationRole : IdentityRole
    {
        [Column(TypeName = "bit")]
        public bool IsActive { get; set; } = true;

        [Column(TypeName = "varchar(30)")]
        public string CreatedBy { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime CreatedAt { get; set; }

        public ICollection<ApplicationRolePermissions> RolePermissions { get; set; }

    }
}
