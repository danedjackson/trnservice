using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace trnservice.Services.Authorize
{
    public interface IPermissionService
    {
        Task<bool> GetPermissionsAsync(string roleId, string permissionToQuery);
    }
}
