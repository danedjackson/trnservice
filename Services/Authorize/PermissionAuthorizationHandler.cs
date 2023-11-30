using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using trnservice.Areas.Identity.Data;
using trnservice.Models;

namespace trnservice.Services.Authorize
{
    public class PermissionAuthorizationHandler :
        AuthorizationHandler<PermissionRequirement>
    {
        // DI fir Scope Factory since this class will be singleton
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public PermissionAuthorizationHandler(
            IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }
        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, 
            PermissionRequirement requirement)
        {
            // Fetch current user role
            List<string> userRole = context.User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

            // Because we are going to create a singleton, we create ServiceScope
            using IServiceScope scope = _serviceScopeFactory.CreateScope();

            IPermissionService permissionService = scope.ServiceProvider
                .GetRequiredService<IPermissionService>();

            //Process for the annotation
            bool hasPerms = await permissionService
                .GetPermissionsAsync(userRole, requirement.Permission);

            if (hasPerms)
            {
                context.Succeed(requirement);
            }
        }
    }
}
