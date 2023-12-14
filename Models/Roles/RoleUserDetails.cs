using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using trnservice.Areas.Identity.Data;

namespace trnservice.Models
{
    public class RoleUserDetails
    {
        public IdentityRole Role { get; set; }
        public PagedList<ApplicationUser> Members { get; set; }
        public PagedList<ApplicationUser> NonMembers { get; set; }
    }
}
