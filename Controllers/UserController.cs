using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using trnservice.Areas.Identity.Data;
using trnservice.Models;

namespace trnservice.Controllers
{
    [Authorize(Roles = Role.Admin)]
    public class UserController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;

        public UserController(UserManager<ApplicationUser> userManager)
        {
            this.userManager = userManager;
        }
        public IActionResult Index()
        {
            return View(FindNonDeletedUsers());
        }
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create([Required] UserRegisterViewModel userModel)
        {
            if (ModelState.IsValid)
            {
                // Checking if user exists
                // If user exists, assume deleted flag is true.
                // change the deleted flag to false, if not, continue creation
                ApplicationUser userResult = await userManager.FindByEmailAsync(userModel.Email);
                if(null != userResult && userResult.isDeleted)
                {
                    userResult.isDeleted = false;
                    userResult.FirstName = userModel.FirstName;
                    userResult.LastName = userModel.LastName;
                    userResult.Email = userModel.Email;
                    userResult.UserName = userModel.Email;

                    // Setting new password from model
                    var token = await userManager.GeneratePasswordResetTokenAsync(userResult);
                    _ = await userManager.ResetPasswordAsync(userResult, token, userModel.Password);
                    // Updating the user with the newly entered data
                    IdentityResult identityResult = await userManager.UpdateAsync(userResult);
                    if(identityResult.Succeeded)
                    {
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        Errors(identityResult);
                        return View();
                    }
                }
                // Continue Creation if no deleted user is found
                ApplicationUser user = new ApplicationUser
                {
                    FirstName = userModel.FirstName,
                    LastName = userModel.LastName,
                    Email = userModel.Email,
                    UserName = userModel.Email
                };

                IdentityResult result = await userManager.CreateAsync(user, userModel.Password);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    Errors(result);
                }
            }
            return View(userModel);
        }

        public async Task<IActionResult> Update(string id)
        {
            ApplicationUser result = await userManager.FindByIdAsync(id);

            return View(result);
        }

        [HttpPost]
        public async Task<IActionResult> Update([Required] ApplicationUser user)
        {
            if (!ModelState.IsValid)
            {
                return View(user);
            }
            ApplicationUser userDbRecord = await userManager.FindByIdAsync(user.Id);
            userDbRecord.FirstName = user.FirstName;
            userDbRecord.LastName = user.LastName;
            userDbRecord.Email = user.Email;
            userDbRecord.UserName = user.Email;

            IdentityResult result = await userManager.UpdateAsync(userDbRecord);
            if (!result.Succeeded)
            {
                Errors(result);
            }
            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            ApplicationUser user = await userManager.FindByIdAsync(id);

            if(null == user)
            {
                return View("Index");
            }
            user.isDeleted = true;
            IdentityResult result =await userManager.UpdateAsync(user);

            if(!result.Succeeded)
            {
                Errors(result);
            }

            return View("Index", FindNonDeletedUsers());
        }

        private void Errors(IdentityResult result)
        {
            foreach (IdentityError error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
        }
        
        private IQueryable<ApplicationUser> FindNonDeletedUsers()
        {
            return userManager.Users.Where(user => user.isDeleted == false);
        }
    }
}
