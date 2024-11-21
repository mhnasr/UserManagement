using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using UserManagement.Data;
using UserManagement.Models;
using UserManagement.Services;

namespace UserManagement.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SMSService _smsService;
        private readonly JwtService _jwtService;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, SMSService smsService, JwtService jwtService, SignInManager<ApplicationUser> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _smsService = smsService;
            _jwtService = jwtService;
            _signInManager = signInManager;
        }

        [HttpGet]
        public IActionResult Login()
        {

            if (TempData.ContainsKey("SuccessMessage"))
            {
                ViewBag.SuccessMessage = TempData["SuccessMessage"];
            }

            if (User.Identity is { IsAuthenticated: true })
            {
                return RedirectToAction("Index", "Home");
            }
            var setting = _context.Settings.FirstOrDefault();
            if (setting == null || setting.LoginMethod == "UserAndPassword")
            {
                return View("LoginWithPassword");
            }
            else
            {
                return View("LoginWithMobile");
            }
        }




        [HttpPost]
        public async Task<IActionResult> LoginWithPassword(string phoneNumber, string password)
        {
            var user = await _userManager.FindByNameAsync(phoneNumber);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "کاربری با این شماره موبایل یافت نشد.");
                return View("LoginWithPassword");
            }

            if (!await _userManager.CheckPasswordAsync(user, password))
            {
                ModelState.AddModelError(string.Empty, "رمز عبور اشتباه است.");
                return View("LoginWithPassword");
            }

            // تولید JWT یا ورود موفق
            var roles = await _userManager.GetRolesAsync(user);
            var token = _jwtService.GenerateJwtToken(user.Id, user.Email, roles);
            await _signInManager.SignInAsync(user, isPersistent: true);

            TempData["SuccessMessage"] = "ورود شما با موفقیت انجام شد.";
            return RedirectToAction("Index", "Home");

        }

        [HttpGet]
        public async Task<IActionResult> LoginWithMobile()
        {
            return RedirectToAction("Index", "Home");
        }
        [HttpGet]
        public async Task<IActionResult> LoginWithPassword()
        {
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> LoginWithMobile(string phoneNumber)
        {
            var user = await _userManager.FindByNameAsync(phoneNumber);
            if (user != null)
            {
                // ایجاد کد تأیید
                var verificationCode = new Random().Next(1000, 9999).ToString();

                // ارسال پیامک
                var smsResult = await _smsService.SendSmsAsync($"کد تأیید شما: {verificationCode}", phoneNumber);

                if (smsResult == "Success")
                {
                    // ذخیره در Session
                    HttpContext.Session.SetString("VerificationCode", verificationCode);
                    HttpContext.Session.SetString("PhoneNumber", phoneNumber);

                    return RedirectToAction("VerifyCode");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, smsResult);
                    return View("LoginWithMobile");
                }
            }

            ModelState.AddModelError(string.Empty, "شماره موبایل وارد شده یافت نشد.");
            return View("LoginWithMobile");
        }




        [HttpGet]
        public IActionResult VerifyCode()
        {
            var phoneNumber = HttpContext.Session.GetString("PhoneNumber");
            var verificationCode = HttpContext.Session.GetString("VerificationCode");

            if (string.IsNullOrEmpty(phoneNumber) || string.IsNullOrEmpty(verificationCode))
            {
                ModelState.AddModelError(string.Empty, "اطلاعات نامعتبر است.");
                return RedirectToAction("LoginWithMobile");
            }

            ViewBag.PhoneNumber = phoneNumber;
            ViewBag.VerificationCode = verificationCode; // ارسال کد تأیید به ویو

            return View();
        }



        [HttpPost]
        public async Task<IActionResult> VerifyCode(string code)
        {
            // خواندن مقادیر از Session
            var verificationCode = HttpContext.Session.GetString("VerificationCode");
            var phoneNumber = HttpContext.Session.GetString("PhoneNumber");

            if (string.IsNullOrEmpty(verificationCode) || string.IsNullOrEmpty(phoneNumber))
            {
                ModelState.AddModelError(string.Empty, "اطلاعات نامعتبر است. لطفاً دوباره تلاش کنید.");
                return View("VerifyCode");
            }

            if (code != verificationCode)
            {
                ModelState.AddModelError(string.Empty, "کد تأیید اشتباه است.");
                return View("VerifyCode");
            }

            // یافتن کاربر
            var user = await _userManager.FindByNameAsync(phoneNumber);
            if (user != null)
            {
                // ورود کاربر به سیستم
                await _signInManager.SignInAsync(user, isPersistent: true);

                // پاک کردن Session (اختیاری)
                HttpContext.Session.Remove("VerificationCode");
                HttpContext.Session.Remove("PhoneNumber");

                // هدایت به صفحه Home
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, "کاربری با این شماره موبایل یافت نشد.");
            return View("VerifyCode");
        }





        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(string firstName, string lastName, string phoneNumber, string password)
        {
            if (ModelState.IsValid)
            {
                // ایجاد کاربر جدید
                var user = new ApplicationUser
                {
                    FirstName = firstName,
                    LastName = lastName,
                    UserName = phoneNumber,
                    PhoneNumber = phoneNumber
                };

                // ثبت‌نام کاربر
                var result = await _userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    return RedirectToAction("Login", "Account");
                }

                // نمایش خطاها
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View();
        }




        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string phoneNumber)
        {
            if (string.IsNullOrEmpty(phoneNumber))
            {
                ModelState.AddModelError(string.Empty, "شماره موبایل نمی‌تواند خالی باشد.");
                return View();
            }

            var user = await _userManager.FindByNameAsync(phoneNumber);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "کاربری با این شماره موبایل یافت نشد.");
                return View();
            }

            // ایجاد کد تأیید برای بازیابی رمز عبور
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetCode = new Random().Next(1000, 9999).ToString();

            // ارسال کد بازیابی به موبایل
            var smsResult = await _smsService.SendSmsAsync($"کد بازیابی رمز عبور شما: {resetCode}", phoneNumber);
            if (smsResult != "Success")
            {
                ModelState.AddModelError(string.Empty, "ارسال پیامک با مشکل مواجه شد.");
                return View();
            }

            HttpContext.Session.SetString("PhoneNumber", phoneNumber);
            HttpContext.Session.SetString("ResetCode", resetCode);
            HttpContext.Session.SetString("ResetToken", token);

            // ذخیره پیام موفقیت
            TempData["Message"] = "کد تأییدیه برای شما ارسال گردید. لطفاً کد را وارد نمایید.";

            return RedirectToAction("ResetPassword");
        }



        [HttpGet]
        public IActionResult ResetPassword()
        {
            var phoneNumber = HttpContext.Session.GetString("PhoneNumber");
            var resetCode = HttpContext.Session.GetString("ResetCode");

            if (string.IsNullOrEmpty(phoneNumber) || string.IsNullOrEmpty(resetCode))
            {
                ModelState.AddModelError(string.Empty, "اطلاعات نامعتبر است.");
                return RedirectToAction("ForgotPassword");
            }

            ViewBag.PhoneNumber = phoneNumber;

            // ارسال پیام از TempData به ویو
            if (TempData.ContainsKey("Message"))
            {
                ViewBag.Message = TempData["Message"];
            }

            return View();
        }



        [HttpPost]
        public async Task<IActionResult> ResetPassword(string phoneNumber, string resetCode, string newPassword)
        {
            var expectedCode = HttpContext.Session.GetString("ResetCode");
            var token = HttpContext.Session.GetString("ResetToken");

            if (string.IsNullOrEmpty(expectedCode) || string.IsNullOrEmpty(token) || resetCode != expectedCode)
            {
                ModelState.AddModelError(string.Empty, "کد بازیابی اشتباه است.");
                ViewBag.PhoneNumber = phoneNumber; // نمایش شماره موبایل برای ورود دوباره
                return View();
            }

            var user = await _userManager.FindByNameAsync(phoneNumber);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "کاربری با این شماره موبایل یافت نشد.");
                return View();
            }

            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
            if (result.Succeeded)
            {
                // پاک کردن مقادیر Session
                HttpContext.Session.Remove("ResetCode");
                HttpContext.Session.Remove("PhoneNumber");
                HttpContext.Session.Remove("ResetToken");

                // ذخیره پیام موفقیت در TempData
                TempData["SuccessMessage"] = "کلمه عبور شما با موفقیت تغییر کرد. اکنون با رمز جدید وارد شوید.";

                return RedirectToAction("Login", "Account");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View();
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }

    }
}
