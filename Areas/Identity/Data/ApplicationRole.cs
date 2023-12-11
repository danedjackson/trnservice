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

        [Column(TypeName = "varchar(30)")]
        public string? ModifiedBy { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? LastModified { get; set; }

        [Column(TypeName = "varchar(30)")]
        public string? DeletedBy { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? DeletedAt { get; set; }


        public ICollection<ApplicationRolePermission> RolePermissions { get; set; }

    }
}
