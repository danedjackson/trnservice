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

namespace trnservice.Controllers
{
    [Authorize(Roles = Role.Admin)]
    public class UserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly EmailService _emailService;

        // Constant variables for pagination process
        private readonly int PAGE = 1;
        private readonly int PAGE_SIZE = 10;
        public UserController(UserManager<ApplicationUser> userManager,
            EmailService emailService)
        {
            _userManager = userManager;
            _emailService = emailService;
        }

        public IActionResult Index(string searchString, bool showInactive, int page = 1, int pageSize = 10)
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

                IdentityResult result = await _userManager.CreateAsync(user, userModel.Password);

                if (result.Succeeded)
                {
                    await SendEmailConfirmationCode(user);

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
            return View("Index", FindNonDeletedUsers());
        }
        public IActionResult ResentConfirmationEmail()
        {
            return View();
        }

        public async Task<IActionResult> ResendConfirmationEmail(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            await SendEmailConfirmationCode(user);

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
            user.IsActive = false;
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

            IdentityResult result = await _userManager.UpdateAsync(fetchedUser);

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
            return PaginateList(_userManager.Users.Where(user => user.IsActive == true), PAGE, PAGE_SIZE);
        }

        private PagedList<ApplicationUser> FindAllUsers()
        {
            return PaginateList(_userManager.Users, PAGE, PAGE_SIZE);
        }
        private async Task UpdatePassword(ApplicationUser applicationUser)
        {
            // Setting new password from model
            var token = await _userManager.GeneratePasswordResetTokenAsync(applicationUser);
            _ = await _userManager.ResetPasswordAsync(applicationUser, token, applicationUser.Password);
        }

        private async Task SendEmailConfirmationCode(ApplicationUser user)
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
                $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");
        }

        private PagedList<ApplicationUser> PaginateList(IQueryable<ApplicationUser> query, int page, int pageSize)
        {
            var totalCount = query.Count();
            var pagedUsers = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            return new PagedList<ApplicationUser>(pagedUsers, totalCount, page, pageSize);
        }
    }
}
