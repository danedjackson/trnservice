using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using trnservice.Areas.Identity.Data;
using trnservice.Data;
using Microsoft.EntityFrameworkCore;
using trnservice.Services.Utils;

namespace trnservice.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly AuthDbContext _authDbContext;
        private readonly AlternativeDbContext _alternativeDbContext;

        private readonly ILogger<LoginModel> _logger;

        public LoginModel(SignInManager<ApplicationUser> signInManager, ILogger<LoginModel> logger,
            UserManager<ApplicationUser> userManager, AuthDbContext authDbContext,
            AlternativeDbContext alternativeDbContext)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _authDbContext = authDbContext;
            _alternativeDbContext = alternativeDbContext;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            public string UserName { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Display(Name = "Remember This Device")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (User.Identity.IsAuthenticated)
            {
                Response.Redirect("/");
            }

            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl = returnUrl ?? Url.Content("~/");

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");

            if (ModelState.IsValid)
            {
                ApplicationUser user = await _userManager.FindByNameAsync(Input.UserName);

                if (user != null && !user.IsActive)
                {
                    ModelState.AddModelError(string.Empty, "Account Inactive.");
                    //await _signInManager.SignOutAsync();
                    return Page();
                }
                // Check to see if the userId exists for this platform
                bool userExistsInPlatformUsers = _authDbContext.PlatformUsers.Any(platformUser => 
                    platformUser.UserId == user.Id 
                        && platformUser.PlatformId == Convert.ToInt32(AppSettings.GetAppSetting("PlatformId")));
               
                if (!userExistsInPlatformUsers)
                {
                    ModelState.AddModelError(string.Empty, "Account not registered for this platform");
                    return Page();
                }

                var signInResult = await _signInManager.PasswordSignInAsync(Input.UserName, Input.Password, Input.RememberMe, lockoutOnFailure: true);
                
                if (!signInResult.Succeeded)
                {
                    // Check for various login result scenarios
                    if (signInResult.RequiresTwoFactor)
                    {
                        return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
                    }
                    if (signInResult.IsLockedOut)
                    {
                        _logger.LogWarning("User account locked out.");
                        return RedirectToPage("./Lockout");
                    }

                    if(user != null) { 
                        // Invalid login attempt, add error message
                        ModelState.AddModelError(string.Empty, $"Invalid login attempt. " +
                            $"Attempts left: {(_signInManager.Options.Lockout.MaxFailedAccessAttempts-1) - user.AccessFailedCount}");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, $"{Input.UserName} does not exist");
                    }
                    return Page();
                }

                _logger.LogInformation("User logged in.");

                if (user.LastLoggedIn is null)
                {
                    return RedirectToAction("ForceChangePassword", "Account");
                }
                else
                {
                    /*
                     * Developer Note:
                     * Initially had a straightforward process to read user data using _userManager, 
                     * then update LastLoggedIn if the user has logged in before. However, we were getting
                     * an issue with the db Context tracking the ID and not allowing us to update using the
                     * same context.
                     * 
                     * Detaching the ApplicationUser from the context before the update was not working.
                     * 
                     * Work around was to create an alternate DB Context, load the user initially with main context
                     * (_userManager uses main context), then update using the alternate DB Context.
                     * 
                     * We are also Detaching the ApplicationUser from the initial context to be safe.
                     */
                    _authDbContext.Entry(user).State = EntityState.Detached;

                    ApplicationUser updateUser = _alternativeDbContext.Users.FirstOrDefault(u =>
                        u.UserName == Input.UserName);

                    updateUser.LastLoggedIn = DateTime.Now;

                    _alternativeDbContext.Update(updateUser);
                    var updated = _alternativeDbContext.SaveChanges();

                    if (updated == 0 )
                    {
                        _logger.LogWarning($"Could not set LastLoggedIn value for user '{user.UserName}'");
                    }

                    //var updateResult = await _userManager.UpdateAsync(user);

                    //if (!updateResult.Succeeded)
                    //{
                    //    _logger.LogWarning($"Could not set LastLoggedIn value for user '{user.UserName}'");
                    //}
                }

                return LocalRedirect(returnUrl);
            }

            return Page();
        }
    }
}
