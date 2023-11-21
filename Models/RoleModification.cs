using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace trnservice.Models
{
    public class RoleModification
    {
        [Required]
        public string RoleName { get; set; }

        public string RoleId { get; set; }
#nullable enable
        public string[]? AddIds { get; set; }

        public string[]? DeleteIds { get; set; }
#nullable disable
    }
}
