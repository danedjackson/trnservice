using EmailClient;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Collections;
using System.Threading.Tasks;
using trnservice.Areas.Identity.Data;
using trnservice.Models;
using trnservice.Models.User;

namespace trnservice.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly EmailService _emailService;

        public AccountController(UserManager<ApplicationUser> userManager,
            EmailService emailService)
        {
            _userManager = userManager;
            _emailService = emailService;
        }
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel forgotPasswordViewModel)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByNameAsync(forgotPasswordViewModel.UserName);
                if (user != null)
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = token }, protocol: HttpContext.Request.Scheme);

                    // Send an email with the reset link 
                    _emailService.SendEmail(
                        user.Email,
                        "Password Reset Confirmation",
                        $"Click this link to reset your password for the TRN Validation Service:<br>{callbackUrl}"
                        );
                }

                // If the user is not found, still show a success message to prevent enumeration attacks
                forgotPasswordViewModel.Message = "If username exists, an email confirmation was sent to the corresponding email address, please check your email!";
                return View(forgotPasswordViewModel);
            }

            return View(forgotPasswordViewModel);
        }

        [HttpGet]
        public IActionResult ResetPassword(string userId, string code)
        {
            var model = new ResetPasswordViewModel { UserId = userId, Code = code };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel resetPasswordViewModel)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(resetPasswordViewModel.UserId);
                if (user != null)
                {
                    var result = await _userManager.ResetPasswordAsync(user, resetPasswordViewModel.Code, resetPasswordViewModel.NewPassword);

                    if (result.Succeeded)
                    {
                        resetPasswordViewModel = new ResetPasswordViewModel
                        {
                            Message = "Successfully changed your password. Please use your new password to login!"
                        };
                        return View(resetPasswordViewModel);
                    }

                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
                ModelState.AddModelError(string.Empty, "Could not change your password. Please try again.");
            }

            return View(resetPasswordViewModel);
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel changePasswordViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            if(user is null)
            {
                ModelState.AddModelError(string.Empty, "Could not change your password");
                return View();
            }

            IdentityResult result = await _userManager.ChangePasswordAsync(user, changePasswordViewModel.CurrentPassword,
                changePasswordViewModel.NewPassword);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View();
            }

            changePasswordViewModel.Message = "You have successfully changed your password! Use your updated password on your next sign in.";
            return View(changePasswordViewModel);
        }

        [HttpGet]
        public IActionResult ForceChangePassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForceChangePassword(ForceChangePasswordViewModel forceChangePasswordViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            if (user is null)
            {
                ModelState.AddModelError(string.Empty, "Could not change your password");
                return View();
            }
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            IdentityResult result = await _userManager.ResetPasswordAsync(user, token,
                forceChangePasswordViewModel.NewPassword);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View();
            }
            // Updating Last log in field
            user.LastLoggedIn = System.DateTime.Now;
            var update = await _userManager.UpdateAsync(user);

            if (!update.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View();
            }
            forceChangePasswordViewModel.Message = "You have successfully changed your password! Use your updated password on your next sign in.";
            return View(forceChangePasswordViewModel);
        }
    }
}
