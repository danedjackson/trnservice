//yogihosting.com/aspnet-core-identity-roles

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using trnservice.Areas.Identity.Data;
using trnservice.Models;

namespace trnservice.Controllers
{
    [Authorize(Roles = Role.Admin)]
    public class RoleController : Controller
    {
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly UserManager<ApplicationUser> userManager;
        public RoleController(RoleManager<IdentityRole> roleManager, 
            UserManager<ApplicationUser> userManager)
        {
            this.roleManager = roleManager;
            this.userManager = userManager;
        }

        public ViewResult Index()
        {
            return View(roleManager.Roles);
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
                IdentityResult result = await roleManager.CreateAsync(new IdentityRole(name));
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
            IdentityRole role = await roleManager.FindByIdAsync(id);
            List<ApplicationUser> members = new List<ApplicationUser>();
            List<ApplicationUser> nonMembers = new List<ApplicationUser>();

            // Check if the user is already in the role specified
            foreach (ApplicationUser user in userManager.Users)
            {
                // Append to Member or NonMember List depending on user being found in role or not
                var memberOrNonMemberList = await userManager.IsInRoleAsync(user, role.Name) ? members : nonMembers;
                memberOrNonMemberList.Add(user);
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
                    ApplicationUser user = await userManager.FindByIdAsync(userId);
                    if (null != user)
                    {
                        result = await userManager.AddToRoleAsync(user, roleModification.RoleName);
                        if (!result.Succeeded)
                            Errors(result);
                    }
                }
                // If DeleteIds is not empty, remove role from users
                foreach (string userId in roleModification.DeleteIds ?? new string[] { })
                {
                    ApplicationUser user = await userManager.FindByIdAsync(userId);
                    if (null != user  )
                    {
                        result = await userManager.RemoveFromRoleAsync(user, roleModification.RoleName);
                        if (!result.Succeeded)
                            Errors(result);
                    }
                }
            }

            if (ModelState.IsValid)
            {
                return RedirectToAction(nameof(Index));
            }
            else
            {
                return await Update(roleModification.RoleId);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            IdentityRole role = await roleManager.FindByIdAsync(id);
            if (null != role)
            {
                IdentityResult result = await roleManager.DeleteAsync(role);
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
            return View("Index", roleManager.Roles);
        }

        private void Errors(IdentityResult result)
        {
            foreach (IdentityError error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
        }
    }
}
