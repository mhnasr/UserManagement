using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using UserManagement.Data;
using UserManagement.Models;

public class DataSeeder
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ApplicationDbContext _context;

    public DataSeeder(RoleManager<IdentityRole> roleManager, ApplicationDbContext context)
    {
        _roleManager = roleManager;
        _context = context;
    }

    public async Task SeedRolesAsync()
    {
        if (!await _roleManager.RoleExistsAsync("Admin"))
            await _roleManager.CreateAsync(new IdentityRole("Admin"));
        if (!await _roleManager.RoleExistsAsync("User"))
            await _roleManager.CreateAsync(new IdentityRole("User"));
    }

    public async Task SeedSettingsAsync()
    {
        if (!_context.Settings.Any())
        {
            _context.Settings.Add(new Setting
            {
                LoginMethod = "UserAndPassword" // حالت پیش‌فرض
            });
            await _context.SaveChangesAsync();
        }

    }


    public async Task SeedAllAsync()
    {
        await SeedRolesAsync();
        await SeedSettingsAsync();
    }
}