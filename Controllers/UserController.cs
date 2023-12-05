using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using trnservice.Areas.Identity.Data;
using trnservice.Models;
using trnservice.Services.Authorize;
using EmailClient;

namespace trnservice.Controllers
{
    [Authorize(Roles = Role.Admin)]
    public class UserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        public UserController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public IActionResult Index(string searchString, int page = 1, int pageSize = 10)
        {
            // Fetching all users
            var query = _userManager.Users;

            // Apply search filter from UI
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(user =>
                    user.Email.Contains(searchString) ||
                    user.FirstName.Contains(searchString) ||
                    user.LastName.Contains(searchString) ||
                    user.UserName.Contains(searchString));
            }

            // Apply pagination
            PagedList<ApplicationUser> pagedResult = PaginateList(query, page, pageSize);

            return View(pagedResult);
        }

        [HasPermission(Permissions.CanDoUserManagement)]
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
                ApplicationUser userResult = await _userManager.FindByEmailAsync(userModel.Email);
                if(null != userResult && userModel.Email.ToLower().Equals(userResult.Email.ToLower())) {
                    ModelState.AddModelError("", "Email address already exists");
                    return View();
                }
                if(null != userResult && !userResult.IsActive)
                {
                    userResult.IsActive = true;
                    userResult.FirstName = userModel.FirstName;
                    userResult.LastName = userModel.LastName;
                    userResult.UserName = userModel.UserName;
                    userResult.Email = userModel.Email;
                    userResult.Password = userModel.Password;
                    // Setting new password from model
                    await UpdatePassword(userResult);

                    // Updating the user with the newly entered data
                    IdentityResult identityResult = await _userManager.UpdateAsync(userResult);
                    if(identityResult.Succeeded)
                    {
                        return RedirectToAction("Index", FindAllUsers());
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
                    UserName = userModel.UserName,
                    Email = userModel.Email
                };

                IdentityResult result = await _userManager.CreateAsync(user, userModel.Password);
                if (result.Succeeded)
                {
                    userModel.Message = "Successfully created user details";
                    return View(userModel);
                }
                else
                {
                    Errors(result);
                }
            }
            return View(userModel);
        }

        [HasPermission(Permissions.CanDoUserManagement)]
        public async Task<IActionResult> Update(string id)
        {
            ApplicationUser result = await _userManager.FindByIdAsync(id);

            return View(result);
        }

        [HttpPost]
        public async Task<IActionResult> Update([Required] ApplicationUser user)
        {
            if (!ModelState.IsValid)
            {
                return View(user);
            }
            ApplicationUser userDbRecord = await _userManager.FindByIdAsync(user.Id);
            userDbRecord.FirstName = user.FirstName;
            userDbRecord.LastName = user.LastName;
            //userDbRecord.Email = user.Email;
            //userDbRecord.UserName = user.UserName;
            if(null != user.Password)
            {
                userDbRecord.Password = user.Password;
                await UpdatePassword(userDbRecord);
            }

            IdentityResult result = await _userManager.UpdateAsync(userDbRecord);
            if (!result.Succeeded)
            {
                Errors(result);
            }
            return View("Index", FindAllUsers());
        }

        [HttpPost]
        [HasPermission(Permissions.CanDoUserManagement)]
        public async Task<IActionResult> Delete(string id)
        {
            ApplicationUser user = await _userManager.FindByIdAsync(id);
            if(null == user)
            {
                return View("Index", FindAllUsers());
            }
            user.IsActive = false;
            IdentityResult result =await _userManager.UpdateAsync(user);

            if(!result.Succeeded)
            {
                Errors(result);
            }

            return View("Index", FindAllUsers());
        }

        public async Task<IActionResult> Reactivate(string id)
        {
            ApplicationUser fetchedUser = await _userManager.FindByIdAsync(id);
            if(null == fetchedUser)
            {
                return View("Index", FindAllUsers());
            }

            fetchedUser.IsActive = true;

            IdentityResult result = await _userManager.UpdateAsync(fetchedUser);

            if (!result.Succeeded)
            {
                Errors(result);
            }

            return View("Index", FindAllUsers());
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
            return _userManager.Users.Where(user => user.IsActive == true);
        }

        private PagedList<ApplicationUser> FindAllUsers()
        {
            return PaginateList(_userManager.Users, 1, 10);
        }
        private async Task UpdatePassword(ApplicationUser applicationUser)
        {
            // Setting new password from model
            var token = await _userManager.GeneratePasswordResetTokenAsync(applicationUser);
            _ = await _userManager.ResetPasswordAsync(applicationUser, token, applicationUser.Password);
        }

        private PagedList<ApplicationUser> PaginateList(IQueryable<ApplicationUser> query, int page, int pageSize)
        {
            var totalCount = query.Count();
            var pagedUsers = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            return new PagedList<ApplicationUser>(pagedUsers, totalCount, page, pageSize);
        }
    }
}
