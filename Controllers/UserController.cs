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
using System.Text;
using Microsoft.AspNetCore.WebUtilities;
using System.Text.Encodings.Web;
using trnservice.Services;

namespace trnservice.Controllers
{
    [Authorize(Roles = Role.Admin)]
    public class UserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly EmailService _emailService;
        private readonly Utils _utils;
        public UserController(UserManager<ApplicationUser> userManager,
            EmailService emailService, Utils utils)
        {
            _userManager = userManager;
            _emailService = emailService;
            _utils = utils;
        }

        public IActionResult Index(string searchString, bool showInactive, string sortOrder, 
            string sortDirection, int page = 1, int pageSize = 10)
        {
            // Fetching all users
            var query = _userManager.Users;

            if (!showInactive)
            {
                query = query.Where(user => user.IsActive);
            }
            // Apply search filter from UI
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(user =>
                    user.Email.Contains(searchString) ||
                    user.FirstName.Contains(searchString) ||
                    user.LastName.Contains(searchString) ||
                    user.UserName.Contains(searchString));
            }

            // Apply sorting
            query = (sortOrder?.ToLower()) switch
            {
                "firstname" => (sortDirection?.ToLower() == "desc") ? query.OrderByDescending(user => user.FirstName) : query.OrderBy(user => user.FirstName),
                "lastname" => (sortDirection?.ToLower() == "desc") ? query.OrderByDescending(user => user.LastName) : query.OrderBy(user => user.LastName),
                "username" => (sortDirection?.ToLower() == "desc") ? query.OrderByDescending(user => user.UserName) : query.OrderBy(user => user.UserName),
                "status" => (sortDirection?.ToLower() == "desc") ? query.OrderByDescending(user => user.IsActive) : query.OrderBy(user => user.IsActive),
                _ => (sortDirection?.ToLower() == "desc") ? query.OrderByDescending(user => user.Email) : query.OrderBy(user => user.Email),
            };

            // Apply pagination
            PagedList<ApplicationUser> pagedResult = _utils.PaginateList(query, page, pageSize);

            // Pass sorting information to the view
            ViewBag.SortOrder = sortOrder;
            ViewBag.SortDirection = sortDirection;
            ViewBag.ShowInactive = showInactive;

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
                ApplicationUser userResult = await _userManager.FindByEmailAsync(userModel.Email);
                if(null != userResult && userModel.Email.ToLower().Equals(userResult.Email.ToLower())) {
                    ModelState.AddModelError("", "Email address already exists");
                    return View();
                }

                // Continue Creation if no deleted user is found
                ApplicationUser user = new ApplicationUser
                {
                    FirstName = userModel.FirstName,
                    LastName = userModel.LastName,
                    UserName = userModel.UserName,
                    Email = userModel.Email
                };
                string randomPassword = _utils.GenerateRandomPassword(6);

                IdentityResult result = await _userManager.CreateAsync(user, randomPassword);

                if (result.Succeeded)
                {
                    await SendEmailConfirmationCode(user, randomPassword);

                    userModel.Message = "Successfully created user details. Inform user to check their email for login information.";

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

            // Setting ModifiedBy and LastModified fields
            userDbRecord = _utils.UpdateModifiedFields(userDbRecord, User.Identity.Name);

            IdentityResult result = await _userManager.UpdateAsync(userDbRecord);
            if (!result.Succeeded)
            {
                Errors(result);
            }
            return View("Index", FindNonDeletedUsers());
        }
        public IActionResult ResentConfirmationEmail()
        {
            return View();
        }

        public async Task<IActionResult> ResendConfirmationEmail(string id)
        {
            ApplicationUser user = await _userManager.FindByIdAsync(id);
            string randomPassword = _utils.GenerateRandomPassword(6);

            await UpdatePassword(user, randomPassword);

            await SendEmailConfirmationCode(user, randomPassword);

            return View();
        }

        [HttpPost]
        [HasPermission(Permissions.CanDoUserManagement)]
        public async Task<IActionResult> Delete(string id)
        {
            ApplicationUser user = await _userManager.FindByIdAsync(id);
            if(null == user)
            {
                return View("Index", FindNonDeletedUsers());
            }
            user = _utils.UpdateDeletedFields(user, User.Identity.Name);
            IdentityResult result =await _userManager.UpdateAsync(user);

            if(!result.Succeeded)
            {
                Errors(result);
            }

            return View("Index", FindNonDeletedUsers());
        }

        public async Task<IActionResult> Reactivate(string id)
        {
            ApplicationUser fetchedUser = await _userManager.FindByIdAsync(id);
            if(null == fetchedUser)
            {
                return View("Index", FindNonDeletedUsers());
            }

            fetchedUser.IsActive = true;
            fetchedUser = _utils.UpdateModifiedFields(fetchedUser, User.Identity.Name);

            IdentityResult result = await _userManager.UpdateAsync(fetchedUser);

            if (!result.Succeeded)
            {
                Errors(result);
            }

            return View("Index", FindNonDeletedUsers());
        }

        public async Task<IActionResult> Unlock(string id)
        {
            ApplicationUser user = await _userManager.FindByIdAsync(id);
            if (null == user)
            {
                return View("Index", FindNonDeletedUsers());
            }

            user.LockoutEnd = null;
            user = _utils.UpdateModifiedFields(user, User.Identity.Name);

            IdentityResult result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
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

        private PagedList<ApplicationUser> FindNonDeletedUsers()
        {
            return _utils.PaginateList(_userManager.Users.Where(user => user.IsActive == true), 1, 10);
        }

        private PagedList<ApplicationUser> FindAllUsers()
        {
            return _utils.PaginateList(_userManager.Users, 1, 10);
        }

        private async Task UpdatePassword(ApplicationUser applicationUser, string newPassword)
        {
            // Setting new password from model
            var token = await _userManager.GeneratePasswordResetTokenAsync(applicationUser);
            await _userManager.ResetPasswordAsync(applicationUser, token, newPassword);
        }

        private async Task SendEmailConfirmationCode(ApplicationUser user, string randomPassword)
        {
            // Generate code to confirm email
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = Url.Page(
                "/Account/ConfirmEmail",
                pageHandler: null,
                values: new { area = "Identity", userId = user.Id, code = code, returnUrl = Url.Content("~/") },
                protocol: Request.Scheme);

            _emailService.SendEmail(user.Email, "Confirm your email",
                $"Hello {user.FirstName}, your account was created for the TRN Validation Service. </br>" +
                $"You will need to activate your account by clicking the link below. </br>" +
                $"Please note that you will need to use the One Time Password to create a new password on first log in.</br></br>" +
                $"See below for your Username, OTP and Confirmation link:</br></br>" +
                $"Your Username is: <b>{user.UserName}</b></br>" +
                $"Your One Time Password (OTP) is: <b>{randomPassword}</b></br></br>" +
                $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");
        }
    }
}
