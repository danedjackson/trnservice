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
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByNameAsync(model.UserName);
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
                model.Message = "If username exists, an email confirmation was sent to the corresponding email address, please check your email!";
                return View(model);
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult ResetPassword(string userId, string code)
        {
            var model = new ResetPasswordViewModel { UserId = userId, Code = code };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(model.UserId);
                if (user != null)
                {
                    var result = await _userManager.ResetPasswordAsync(user, model.Code, model.NewPassword);

                    if (result.Succeeded)
                    {
                        // Automatically sign in the user after successful password reset
                        //await _signInManager.SignInAsync(user, isPersistent: false);

                        model = new ResetPasswordViewModel
                        {
                            Message = "Successfully changed your password. Please use your new password to login!"
                        };
                        return View(model);
                    }

                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
                ModelState.AddModelError(string.Empty, "Could not change your password. Please try again.");
            }

            return View(model);
        }
    }
}
