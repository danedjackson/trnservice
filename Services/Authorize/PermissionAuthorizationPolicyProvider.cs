using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace trnservice.Services.Authorize
{
    public class PermissionAuthorizationPolicyProvider : DefaultAuthorizationPolicyProvider
    {
        // We create a policy in order to facilitate the requirements of our Annotation
        public PermissionAuthorizationPolicyProvider(IOptions<AuthorizationOptions> options)
            :base(options)
        {

        }

        public override async Task<AuthorizationPolicy?> GetPolicyAsync(
            string policyName)
        {
            // We do a check to see if the policy exists. If it does not, it is created
            AuthorizationPolicy? policy = await base.GetPolicyAsync(policyName);

            if (null != policy)
            {
                return policy;
            }

            return new AuthorizationPolicyBuilder()
                .AddRequirements(new PermissionRequirement(policyName))
                .Build();
        }
    }
}
