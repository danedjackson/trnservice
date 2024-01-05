//yogihosting.com/aspnet-core-identity-roles

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using trnservice.Areas.Identity.Data;
using trnservice.Data;
using trnservice.Models;
using trnservice.Models.Roles;
using trnservice.Services;
using trnservice.Services.Authorize;

namespace trnservice.Controllers
{
    [Authorize(Roles = Role.Admin)]
    public class RoleController : Controller
    {
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AuthDbContext _authDbContext;
        private readonly Utils _utils;

        public RoleController(RoleManager<ApplicationRole> roleManager, 
            UserManager<ApplicationUser> userManager, AuthDbContext authDbContext,
            Utils utils)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _authDbContext = authDbContext;
            _utils = utils;
        }

        public ViewResult Index(string searchString, bool showInactive, string sortOrder,
            string sortDirection, int page = 1, int pageSize = 10)
        {
            // Fetch all roles
            var query = _roleManager.Roles;

            // filter results based on active flag
            if (!showInactive)
            {
                query = query.Where(role => role.IsActive);
            }

            // Apply search string
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(role =>
                    role.Name.Contains(searchString));
            }

            // Sorting
            query = (sortOrder?.ToLower()) switch
            {
                "name" => (sortDirection?.ToLower() == "desc")
                    ? query.OrderByDescending(role => role.Id)
                    : query.OrderBy(role => role.Id),
                "status" => (sortDirection?.ToLower() == "desc") 
                    ? query.OrderByDescending(role => role.IsActive) 
                    : query.OrderBy(role => role.IsActive),
                _ => (sortDirection?.ToLower() == "desc")
                    ? query.OrderByDescending(role => role.Id)
                    : query.OrderBy(role => role.Id),
            };

            // Pagination
            PagedList<ApplicationRole> pagedResult = _utils.PaginateList(query, page, pageSize);

            // ViewBag object used to pass information to the View for persistence
            ViewBag.SortOrder = sortOrder;
            ViewBag.SortDirection = sortDirection;
            ViewBag.ShowInactive = showInactive;

            // Check for success message in TempData
            if (TempData.ContainsKey("SuccessMessage"))
            {
                ViewBag.SuccessMessage = TempData["SuccessMessage"];
            }

            return View(pagedResult);
        }

        [HasPermission(Permissions.CanDoRoleManagement)]
        public IActionResult Create()
        {
            return View(new RoleCreationDetails());
        }

        [HttpPost]
        public async Task<IActionResult> Create([Required] RoleCreationDetails roleCreationDetails)
        {
            if (ModelState.IsValid)
            {
                var role = await _roleManager.FindByNameAsync(roleCreationDetails.Name);
                if (null != role)
                {
                    role.IsActive = true;
                    IdentityResult updateResult = await _roleManager.UpdateAsync(role);
                    if (updateResult.Succeeded)
                    {
                        ModelState.AddModelError("", $"Role already exists. Reactivating {role.Name}");
                        return View("Create");
                    }
                }

                IdentityResult result = await _roleManager.CreateAsync(new ApplicationRole { 
                    Name = roleCreationDetails.Name,
                    CreatedAt = DateTime.Now,
                    CreatedBy = User.Identity.Name,
                    IsActive = true
                });
                if (result.Succeeded)
                {
                    // Fetch permissions from the database
                    List<ApplicationPermission> applicationPermissions =  _authDbContext.Permissions.ToList();

                    // Fetch Role information from the database
                    ApplicationRole roleResult = await _roleManager.FindByNameAsync(roleCreationDetails.Name);
                    
                    // Initialize our role - permission relationship variable
                    List<ApplicationRolePermission> rolePermissions = new List<ApplicationRolePermission>();

                    // Building the Role - Permission relationship context for persistence
                    roleCreationDetails.SelectedPermissions.ForEach(selectedPermission =>
                    {
                        foreach (var perm in applicationPermissions)
                        {
                            if (selectedPermission.ToString() == perm.Name)
                            {
                                rolePermissions.Add(new ApplicationRolePermission
                                {
                                    PermissionId = perm.Id,
                                    RoleId = roleResult.Id
                                });
                                break;
                            }
                        }
                    });

                    // Adding role - permission details
                    _ = _authDbContext.RolePermissions.AddRangeAsync(rolePermissions);
                    // Saving changes to RolePermissions Database
                    var saved = _authDbContext.SaveChanges();

                    if(saved == 0)
                    {
                        ModelState.AddModelError("", "Failed to create new Role");
                        return View("Create");
                    }

                    return RedirectToAction("Index", FindNonDeletedRoles());
                }
                else
                {
                    Errors(result);
                }
            }
            return View("Create");
        }

        // Fetch members and non-members of a selected Role
        [HasPermission(Permissions.CanDoRoleManagement)]
        public async Task<IActionResult> Update(string id, string searchString, string sortOrder,
            string sortDirection, int page = 1, int pageSize = 10)
        {
            IdentityRole role = await _roleManager.FindByIdAsync(id);
            List<ApplicationUser> members = new List<ApplicationUser>();
            List<ApplicationUser> nonMembers = new List<ApplicationUser>();

            // Check if the user is already in the role specified
            foreach (ApplicationUser user in _userManager.Users.Where(user => user.IsActive == true))
            {
                if (await _userManager.IsInRoleAsync(user, role.Name))
                {
                    members.Add(user);
                }
                else
                {
                    nonMembers.Add(user);
                }
            }

            // Apply search filter from UI
            if (!string.IsNullOrEmpty(searchString))
            {
                // Filter members
                var filteredMembers = members.Where(user =>
                    user.Email.Contains(searchString) ||
                    user.FirstName.Contains(searchString) ||
                    user.LastName.Contains(searchString) ||
                    user.UserName.Contains(searchString))
                    .ToList();

                // Filter nonMembers
                var filteredNonMembers = nonMembers.Where(user =>
                    user.Email.Contains(searchString) ||
                    user.FirstName.Contains(searchString) ||
                    user.LastName.Contains(searchString) ||
                    user.UserName.Contains(searchString))
                    .ToList();

                // Use the filtered results
                members = filteredMembers;
                nonMembers = filteredNonMembers;
            }

            // Apply sorting
            // Create copies of the original lists for sorting as sorting original lists may lead to issues
            List<ApplicationUser> membersCopy = new List<ApplicationUser>(members);
            List<ApplicationUser> nonMembersCopy = new List<ApplicationUser>(nonMembers);

            if (!string.IsNullOrEmpty(sortOrder))
            {
                switch (sortOrder.ToLower())
                {
                    case "nonmemberfirstname":
                        nonMembersCopy = SortList(nonMembers, "FirstName", sortDirection);
                        break;
                    case "nonmemberlastname":
                        nonMembersCopy = SortList(nonMembers, "LastName", sortDirection);
                        break;
                    case "nonmemberusername":
                        nonMembersCopy = SortList(nonMembers, "UserName", sortDirection);
                        break;
                    case "nonmemberemail":
                        nonMembersCopy = SortList(nonMembers, "Email", sortDirection);
                        break;
                    case "membersfirstname":
                        membersCopy = SortList(members, "FirstName", sortDirection);
                        break;
                    case "memberslastname":
                        membersCopy = SortList(members, "LastName", sortDirection);
                        break;
                    case "membersusername":
                        membersCopy = SortList(members, "UserName", sortDirection);
                        break;
                    case "membersemail":
                        membersCopy = SortList(members, "Email", sortDirection);
                        break;
                }
            }
            // Apply pagination 
            PagedList<ApplicationUser> pagedMembers = _utils.PaginateList(membersCopy, page, pageSize);
            PagedList<ApplicationUser> pagedNonMembers = _utils.PaginateList(nonMembersCopy, page, pageSize);

            // Pass sorting information to the view
            ViewBag.SortOrder = sortOrder;
            ViewBag.SortDirection = sortDirection;

            return View(new RoleUserDetails
            {
                Role = role,
                Members = pagedMembers,
                NonMembers = pagedNonMembers
            });
        }

        // Add or remove roles from a user
        [HttpPost]
        public async Task<IActionResult> Update(UserRoleModification userRoleModification)
        {
            IdentityResult result;
            if (ModelState.IsValid)
            {
                // If AddIds is not empty, add role to users
                foreach (string userId in userRoleModification.AddIds ?? new string[] { })
                {
                    ApplicationUser user = await FindNonDeletedUser(userId);
                    if (null != user)
                    {
                        result = await _userManager.AddToRoleAsync(user, userRoleModification.RoleName);
                        if (!result.Succeeded)
                        {
                            Errors(result);
                        }
                        else
                        {
                            TempData["SuccessMessage"] = $"{user.UserName} successfully added to {userRoleModification.RoleName} role.";
                        }
                    }
                }
                // If DeleteIds is not empty, remove role from users
                foreach (string userId in userRoleModification.DeleteIds ?? new string[] { })
                {
                    ApplicationUser user = await FindNonDeletedUser(userId);
                    if (null != user  )
                    {
                        result = await _userManager.RemoveFromRoleAsync(user, userRoleModification.RoleName);
                        if (!result.Succeeded)
                        {
                            Errors(result);
                        }
                        else
                        {
                            TempData["SuccessMessage"] = $"{user.UserName} successfully removed from {userRoleModification.RoleName} role.";
                        }
                    }
                }
            }

            if (ModelState.IsValid)
            {
                return RedirectToAction("Index", FindNonDeletedRoles());
            }
            else
            {
                return RedirectToAction("Index", FindNonDeletedRoles());
            }
        }

        [HasPermission(Permissions.CanDoRoleManagement)]
        public async Task<IActionResult> UpdatePermissions(string id)
        {
            // Fetching selected role information
            IdentityRole role = await _roleManager.FindByIdAsync(id);

            // Grabbing all application role-permission relationships from the database
            List<ApplicationRolePermission> rolePermissions = _authDbContext.RolePermissions
                .Where(rp => rp.RoleId == role.Id)
                .ToList();

            // Grabbing all permissions from the database for assignment
            List<ApplicationPermission> allPermissions = _authDbContext.Permissions.ToList();

            // Use LINQ to filter assigned and unassigned permissions
            var assignedPermissionIds = rolePermissions
                                        .Select(rp => rp.PermissionId)
                                        .ToList();
            var assignedPermissions = allPermissions.Where(p => assignedPermissionIds
                                        .Contains(p.Id))
                                        .ToList();
            var unassignedPermissions = allPermissions
                                        .Except(assignedPermissions)
                                        .ToList();

            return View(new RolePermissionDetails
            {
                Role = role,
                AssignedPermissions = assignedPermissions,
                UnassignedPermissions = unassignedPermissions,
            });
        }

        [HttpPost]
        public IActionResult UpdatePermissions(RolePermissionModification rolePermissionModification)
        {
            if (!ModelState.IsValid)
            {
                return View("UpdatePermissions", rolePermissionModification.RoleId);
            }

            foreach (int permissionId in rolePermissionModification.PermissionIdsToAdd
                ?? new int[] { })
            {
                ApplicationPermission permission = _authDbContext.Permissions.FirstOrDefault(p => p.Id == permissionId);
                if (null != permission)
                {

                    // Adding role - permission details
                    _ = _authDbContext.RolePermissions.Add(new ApplicationRolePermission
                    {
                        PermissionId = permission.Id,
                        RoleId = rolePermissionModification.RoleId,
                    });
                }
            }

            foreach (int permissionId in rolePermissionModification.PermissionIdsToDelete
                ?? new int[] { })
            {
                ApplicationPermission permission = _authDbContext.Permissions.FirstOrDefault(p => p.Id == permissionId);
                {

                    // Removing role - permission details
                    _ = _authDbContext.RolePermissions.Remove(new ApplicationRolePermission
                    {
                        PermissionId = permission.Id,
                        RoleId = rolePermissionModification.RoleId,
                    });
                }
            }

            // Saving changes to RolePermissions Database
            var saved = _authDbContext.SaveChanges();

            if (saved == 0)
            {
                ModelState.AddModelError("", "Failed to adjust permission changes");
                return RedirectToAction("Index", FindNonDeletedRoles());
            }

            return RedirectToAction("Index", FindNonDeletedRoles());
        }

        [HasPermission(Permissions.CanDoRoleManagement)]
        public async Task<IActionResult> Reactivate(string id)
        {
            ApplicationRole role = await _roleManager.FindByIdAsync(id);
            if(null == role)
            {
                return View("Index", FindNonDeletedRoles());
            }

            role.IsActive = true;
            role = _utils.UpdateModifiedFields(role, User.Identity.Name);

            IdentityResult result = await _roleManager.UpdateAsync(role);

            if(!result.Succeeded)
            {
                Errors(result);
            }

            return View("Index", FindNonDeletedRoles());
        }

        [HttpPost]
        [HasPermission(Permissions.CanDoRoleManagement)]
        public async Task<IActionResult> Delete(string id)
        {
            ApplicationRole role = await _roleManager.FindByIdAsync(id);
            if (null != role)
            {
                //role.IsActive = false;
                role = _utils.UpdateDeletedFields(role, User.Identity.Name);
                IdentityResult result = await _roleManager.UpdateAsync(role);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index", FindNonDeletedRoles());
                }
                else
                {
                    Errors(result);
                }
            }
            else
            {
                ModelState.AddModelError("", "No role found");
            }
            return View("Index", FindNonDeletedRoles());
        }

        private void Errors(IdentityResult result)
        {
            foreach (IdentityError error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
        }
        private async Task<ApplicationUser> FindNonDeletedUser(string id)
        {
            ApplicationUser user = await _userManager.FindByIdAsync(id);
            if(!user.IsActive)
            {
                return null;
            }
            return user;
        }
        private PagedList<ApplicationRole> FindNonDeletedRoles()
        {
            return _utils.PaginateList(_roleManager.Roles.Where(role => role.IsActive == true), 1, 10);
        }

        // Helper method for sorting
        private List<ApplicationUser> SortList(List<ApplicationUser> list, string criteria, string sortDirection)
        {
            switch (criteria.ToLower())
            {
                case "firstname":
                    return (sortDirection?.ToLower() == "desc")
                        ? list.OrderByDescending(user => user.FirstName).ToList()
                        : list.OrderBy(user => user.FirstName).ToList();
                case "lastname":
                    return (sortDirection?.ToLower() == "desc")
                        ? list.OrderByDescending(user => user.LastName).ToList()
                        : list.OrderBy(user => user.LastName).ToList();
                case "username":
                    return (sortDirection?.ToLower() == "desc")
                        ? list.OrderByDescending(user => user.UserName).ToList()
                        : list.OrderBy(user => user.UserName).ToList();
                case "email":
                    return (sortDirection?.ToLower() == "desc")
                        ? list.OrderByDescending(user => user.Email).ToList()
                        : list.OrderBy(user => user.Email).ToList();
                default:
                    return list; // No sorting for unknown properties
            }
        }
    }
}
