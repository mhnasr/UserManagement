using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using UserManagement.Data;
using UserManagement.Models;
using UserManagement.Services;

namespace UserManagement.Areas.Account.Controllers
{
    [Area("Account")]
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly SMSService _smsService;
        public AccountController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, SMSService smsService)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _smsService = smsService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            var setting = _context.Settings.FirstOrDefault();
            if (setting == null || setting.LoginMethod == "UserAndPassword")
            {
                // نمایش فرم ورود با یوزر و پسورد
                return View("LoginWithPassword");
            }
            else
            {
                // نمایش فرم ورود با شماره موبایل
                return View("LoginWithMobile");
            }
        }

        [HttpPost]
        public async Task<IActionResult> LoginWithPassword(string phoneNumber, string password)
        {
            var user = await _userManager.FindByNameAsync(phoneNumber);
            if (user != null)
            {
                var result = await _signInManager.PasswordSignInAsync(user, password, isPersistent: false, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }
            }

            ModelState.AddModelError(string.Empty, "شماره موبایل یا کلمه عبور اشتباه است.");
            return View("LoginWithPassword");
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
                var smsResult = await _smsService.SendSmsAsync($"کد تأیید شما: {verificationCode} ", phoneNumber, "100040001");

                if (smsResult == "Success")
                {
                    TempData["VerificationCode"] = verificationCode;
                    TempData["PhoneNumber"] = phoneNumber;
                    return Redirect("/account/account/verifycode");
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
            // بررسی و بازگرداندن TempData
            var phoneNumber = TempData["PhoneNumber"] as string;
            var verificationCode = TempData["VerificationCode"] as string;

            TempData["PhoneNumber"] = phoneNumber;
            TempData["VerificationCode"] = verificationCode;

            if (string.IsNullOrEmpty(phoneNumber) || string.IsNullOrEmpty(verificationCode))
            {
                return RedirectToAction("LoginWithMobile", "Account", new { area = "Account" });
            }

            return View();
        }



        [HttpPost]
        public async Task<IActionResult> VerifyCode(string code)
        {
            var verificationCode = TempData["VerificationCode"] as string;
            var phoneNumber = TempData["PhoneNumber"] as string;

            TempData["PhoneNumber"] = phoneNumber;
            TempData["VerificationCode"] = verificationCode;

            if (code == verificationCode && phoneNumber != null)
            {
                var user = await _userManager.FindByNameAsync(phoneNumber);
                if (user != null)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Home");
                }
            }

            ModelState.AddModelError(string.Empty, "کد تأیید اشتباه است.");
            return View();
        }


    }
}
