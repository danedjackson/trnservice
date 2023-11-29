using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace trnservice.Models
{
    public class Permissions
    {
        public enum Enum
        {
            CanDoIndividualQuery,
            CanDoBulkQuery,
            CanDoRoleManagement,
            CanDoUserManagement,
        }
    }
}
