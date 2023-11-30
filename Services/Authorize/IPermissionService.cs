using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace trnservice.Services.Authorize
{
    public interface IPermissionService
    {
        Task<bool> GetPermissionsAsync(List<string> roleId, string permissionToQuery);
    }
}
