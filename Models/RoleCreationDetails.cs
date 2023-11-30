using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace trnservice.Models
{
    public class RoleCreationDetails
    {
        public RoleCreationDetails()
        {
            ExistingPermissions = new List<Permissions>(Enum.GetValues(typeof(Permissions)) as Permissions[]);
            
        }

        public string Name { get; set; } = "";
        public List<Permissions> ExistingPermissions { get; set; }
        public List<Permissions> SelectedPermissions { get; set; }
    }
}
