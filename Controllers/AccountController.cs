using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using trnservice.Models;

namespace trnservice.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;

        public AccountController(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }
        public IActionResult Login()
        {
            return View("Login");
        }



        //[HttpPost]
        //public async Task<IActionResult> Login(UserLoginViewModel user)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return View("Login", user);
        //    }
        //    //var result = await _signInManager.PasswordSignInAsync(user.Username, user.Password, false, lockoutOnFailure: false);

        //    var check = await _userManager.FindByNameAsync(user.Username);

        //    if(null == check)
        //    {
        //        return View("Login", user);
        //    }

        //    var result = await _signInManager.PasswordSignInAsync(check, user.Password, false, false);

        //    if (result.Succeeded)
        //    {
        //        // Redirect to a secure page
        //        return RedirectToAction("Index", "Home");
        //    }
        //    else
        //    {
        //        ModelState.AddModelError(string.Empty, "Invalid login attempt");
        //        return View("Login");
        //    }
        //}

       
    }
}
