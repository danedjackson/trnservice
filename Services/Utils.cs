using System;
using System.Collections.Generic;
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

        public PagedList<T> PaginateList<T>(List<T> query, int page, int pageSize)
        {
            var totalCount = query.Count();
            var pagedUsers = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            return new PagedList<T>(pagedUsers, totalCount, page, pageSize);
        }

        public string GenerateRandomPassword(int passwordLength)
        {
            Random res = new Random();

            // String of alphabets and numbers
            string characterPool = "abcdefghijklmnopqrstuvwxyz0123456789";

            // Initializing the empty password string 
            string randomPassword = "";

            for (int i = 0; i < passwordLength; i++)
            {
                // Selecting a index randomly 
                int randomChar = res.Next(36);

                // Appending the character at the index to the random password. 
                randomPassword += characterPool[randomChar];
            }
            return randomPassword;
        }
    }
}
