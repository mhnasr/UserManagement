using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using UserManagement.Models;

namespace UserManagement.Controllers
{
    [Area("Account")]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpGet]
        public IActionResult Test()
        {
            return Content("Test route is working!");
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.PhoneNumber,
                PhoneNumber = model.PhoneNumber,
                FirstName = model.FirstName,
                LastName = model.LastName
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
            {
                //if (error.Code == "PasswordRequiresUpper")
                //{
                //    ModelState.AddModelError(string.Empty, "رمز عبور باید حداقل یک حرف بزرگ داشته باشد.");
                //}
                //else if (error.Code == "PasswordRequiresLower")
                //{
                //    ModelState.AddModelError(string.Empty, "رمز عبور باید حداقل یک حرف کوچک داشته باشد.");
                //}
                //else if (error.Code == "PasswordRequiresNonAlphanumeric")
                //{
                //    ModelState.AddModelError(string.Empty, "رمز عبور باید حداقل شامل یک کاراکتر غیرالفبایی (مانند !، @، #) باشد.");
                //}

                //else
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string phoneNumber, string password)
        {
            var result = await _signInManager.PasswordSignInAsync(phoneNumber, password, isPersistent: false, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, "نام کاربری یا کلمه عبور اشتباه است.");
            return View();
        }
    }
}
