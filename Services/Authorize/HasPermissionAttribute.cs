using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using trnservice.Models;

namespace trnservice.Services.Authorize
{
    public class HasPermissionAttribute : AuthorizeAttribute
    {
        public HasPermissionAttribute(Permissions.Enum permission)
            :base(policy: permission.ToString())
        {

        }
    }
}
