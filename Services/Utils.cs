using System;
using trnservice.Areas.Identity.Data;

namespace trnservice.Services
{
    public class Utils
    {
        public ApplicationUser UpdateModifiedFields(ApplicationUser user, 
            string modifiedBy)
        {
            user.LastModified = DateTime.Now;
            user.ModifiedBy = modifiedBy;

            return user;
        }
        public ApplicationRole UpdateModifiedFields(ApplicationRole role,
            string modifiedBy)
        {
            role.LastModified = DateTime.Now;
            role.ModifiedBy = modifiedBy;

            return role;
        }
        public ApplicationUser UpdateDeletedFields(ApplicationUser user,
            string modifiedBy)
        {
            user.DeletedAt = DateTime.Now;
            user.ModifiedBy = modifiedBy;
            user.DeletedBy = modifiedBy;
            user.IsActive = false;

            return user;
        }

    }
}
