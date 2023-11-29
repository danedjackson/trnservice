using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace trnservice.Areas.Identity.Data
{
    public class ApplicationPermission
    {
        public int Id { get; set; }
        [Required]
        [Column(TypeName="varchar(50)")]
        public string Name { get; set; }

        [Column(TypeName = "varchar(30)")]
        public string CreatedBy { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime DateCreated { get; set; }

        [JsonIgnore]
        public ICollection<ApplicationRolePermissions> RolePermissions { get; set; }
    }
}
