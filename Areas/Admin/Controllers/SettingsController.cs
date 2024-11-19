using Microsoft.AspNetCore.Mvc;
using System.Linq;
using UserManagement.Data;
using UserManagement.Models;

namespace UserManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class SettingsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SettingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var setting = _context.Settings.FirstOrDefault() ?? new Setting { LoginMethod = "UserAndPassword" };
            return View(setting);
        }

        [HttpPost]
        public IActionResult Index(Setting model)
        {
            var setting = _context.Settings.FirstOrDefault();
            if (setting != null)
            {
                setting.LoginMethod = model.LoginMethod;
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}