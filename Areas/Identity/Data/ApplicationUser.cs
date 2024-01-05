using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace trnservice.Areas.Identity.Data
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser
    {
        [PersonalData]
        [Column(TypeName = "varchar(30)")]
        public string FirstName { get; set; }

        [PersonalData]
        [Column(TypeName = "varchar(30)")]
        public string LastName { get; set; }

        [Column(TypeName = "bit")]
        public Boolean IsActive { get; set; } = true;

        [Column(TypeName = "datetime")]
        public DateTime? LastLoggedIn { get; set; }

        [Column(TypeName = "varchar(30)")]
        public string? DeletedBy { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? DeletedAt { get; set; }

        [Column(TypeName = "varchar(30)")]
        public string? ModifiedBy { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? LastModified { get; set; }

        [JsonIgnore]
        public ICollection<ApplicationPlatformUser> PlatformUsers { get; set; }
    }
}
