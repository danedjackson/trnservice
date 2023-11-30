﻿//yogihosting.com/aspnet-core-identity-roles

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using trnservice.Areas.Identity.Data;
using trnservice.Data;
using trnservice.Models;
using trnservice.Services.Authorize;

namespace trnservice.Controllers
{
    [Authorize(Roles = Role.Admin)]
    public class RoleController : Controller
    {
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AuthDbContext _authDbContext;
        public RoleController(RoleManager<ApplicationRole> roleManager, 
            UserManager<ApplicationUser> userManager, AuthDbContext authDbContext)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _authDbContext = authDbContext;
        }

        public ViewResult Index()
        {
            return View(_roleManager.Roles.Where(role => role.IsActive));
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
                IdentityResult result = await _roleManager.CreateAsync(new ApplicationRole { 
                    Name = roleCreationDetails.Name,
                    CreatedAt = System.DateTime.Now,
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
                    List<ApplicationRolePermissions> rolePermissions = new List<ApplicationRolePermissions>();

                    // Building the Role - Permission relationship context for persistence
                    roleCreationDetails.SelectedPermissions.ForEach(selectedPermission =>
                    {
                        foreach (var perm in applicationPermissions)
                        {
                            if (selectedPermission.ToString() == perm.Name)
                            {
                                rolePermissions.Add(new ApplicationRolePermissions
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

                    return RedirectToAction("Index");
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
        public async Task<IActionResult> Update(string id)
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
            return View(new RoleUserDetails
            {
                Role = role,
                Members = members,
                NonMembers = nonMembers
            });
        }

        // Add or remove roles from a user
        [HttpPost]
        public async Task<IActionResult> Update(RoleModification roleModification)
        {
            IdentityResult result;
            if (ModelState.IsValid)
            {
                // If AddIds is not empty, add role to users
                foreach (string userId in roleModification.AddIds ?? new string[] { })
                {
                    ApplicationUser user = await FindNonDeletedUser(userId);
                    if (null != user)
                    {
                        result = await _userManager.AddToRoleAsync(user, roleModification.RoleName);
                        if (!result.Succeeded)
                        {
                            Errors(result);
                        }
                    }
                }
                // If DeleteIds is not empty, remove role from users
                foreach (string userId in roleModification.DeleteIds ?? new string[] { })
                {
                    ApplicationUser user = await FindNonDeletedUser(userId);
                    if (null != user  )
                    {
                        result = await _userManager.RemoveFromRoleAsync(user, roleModification.RoleName);
                        if (!result.Succeeded)
                        {
                            Errors(result);
                        }
                    }
                }
            }

            if (ModelState.IsValid)
            {
                return RedirectToAction("Index");
            }
            else
            {
                return await Update(roleModification.RoleId);
            }
        }

        [HttpPost]
        [HasPermission(Permissions.CanDoRoleManagement)]
        public async Task<IActionResult> Delete(string id)
        {
            ApplicationRole role = await _roleManager.FindByIdAsync(id);
            if (null != role)
            {
                role.IsActive = false;
                IdentityResult result = await _roleManager.UpdateAsync(role);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index");
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
            return View("Index", _roleManager.Roles);
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
    }
}
