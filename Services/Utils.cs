using System;
using System.Linq;
using trnservice.Areas.Identity.Data;
using trnservice.Models;

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

        public ApplicationRole UpdateDeletedFields(ApplicationRole role,
            string modifiedBy)
        {
            role.DeletedAt = DateTime.Now;
            role.ModifiedBy = modifiedBy;
            role.DeletedBy = modifiedBy;
            role.IsActive = false;

            return role;
        }

        public PagedList<T> PaginateList<T>(IQueryable<T> query, int page, int pageSize)
        {
            var totalCount = query.Count();
            var pagedUsers = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            return new PagedList<T>(pagedUsers, totalCount, page, pageSize);
        }
    }
}
