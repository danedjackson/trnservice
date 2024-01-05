using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace trnservice.Areas.Identity.Data
{
    public class ApplicationPlatform
    {
        public int Id { get; set; }

        [Required]
        [Column(TypeName = "varchar(30)")]
        public string PlatformName { get; set; }

        [Column(TypeName = "varchar(120)")]
        public string Description { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime RegisteredAt { get; set; }

        [Column(TypeName = "varchar(30)")]
        public string RegisteredBy { get; set; }

        [JsonIgnore]
        public ICollection<ApplicationPlatformUser> PlatformUsers { get; set; }
    }
}
