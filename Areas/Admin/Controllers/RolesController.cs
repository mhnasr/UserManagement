using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using UserManagement.Areas.Admin.Models;

namespace UserManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    //[Authorize(Roles = "Admin")] // دسترسی فقط برای مدیران
    public class RolesController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;

        public RolesController(RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
        }

        // لیست نقش‌ها
        public IActionResult Index()
        {
            var roles = _roleManager.Roles;
            return View(roles);
        }

        // ایجاد نقش جدید (GET)
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // ایجاد نقش جدید (POST)
        [HttpPost]
        public async Task<IActionResult> Create(string roleName)
        {
            if (string.IsNullOrEmpty(roleName))
            {
                ModelState.AddModelError(string.Empty, "نام نقش نمی‌تواند خالی باشد.");
                return View();
            }

            var result = await _roleManager.CreateAsync(new IdentityRole(roleName));
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View();
        }


        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound("شناسه نقش یافت نشد.");
            }

            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return NotFound("نقش مورد نظر یافت نشد.");
            }

            // ارسال اطلاعات نقش به ویو
            var model = new RoleEditViewModel
            {
                Id = role.Id,
                Name = role.Name
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(RoleEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var role = await _roleManager.FindByIdAsync(model.Id);
            if (role == null)
            {
                return NotFound("نقش مورد نظر یافت نشد.");
            }

            role.Name = model.Name;
            var result = await _roleManager.UpdateAsync(role);

            if (result.Succeeded)
            {
                return RedirectToAction("Index");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }



        // حذف نقش
        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return NotFound();
            }

            var result = await _roleManager.DeleteAsync(role);
            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "حذف نقش با مشکل مواجه شد.");
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
