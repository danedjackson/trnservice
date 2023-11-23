//yogihosting.com/aspnet-core-identity-roles

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using trnservice.Areas.Identity.Data;
using trnservice.Models;

namespace trnservice.Controllers
{
    [Authorize(Roles = Role.Admin)]
    public class RoleController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        public RoleController(RoleManager<IdentityRole> roleManager, 
            UserManager<ApplicationUser> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public ViewResult Index()
        {
            return View(_roleManager.Roles);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create([Required] string name)
        {
            if (ModelState.IsValid)
            {
                IdentityResult result = await _roleManager.CreateAsync(new IdentityRole(name));
                if (result.Succeeded)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    Errors(result);
                }
            }
            return View(name);
        }

        // Fetch members ad non-members of a selected Role
        public async Task<IActionResult> Update(string id)
        {
            IdentityRole role = await _roleManager.FindByIdAsync(id);
            List<ApplicationUser> members = new List<ApplicationUser>();
            List<ApplicationUser> nonMembers = new List<ApplicationUser>();

            // Check if the user is already in the role specified
            foreach (ApplicationUser user in _userManager.Users.Where(user => user.isDeleted == false))
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
        public async Task<IActionResult> Delete(string id)
        {
            IdentityRole role = await _roleManager.FindByIdAsync(id);
            if (null != role)
            {
                IdentityResult result = await _roleManager.DeleteAsync(role);
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
            if(user.isDeleted)
            {
                return null;
            }
            return user;
        }
    }
}
