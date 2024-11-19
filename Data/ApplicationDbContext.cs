using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using UserManagement.Models;

namespace UserManagement.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSet برای جدول کاربران Identity
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }

        // DbSet برای جدول تنظیمات
        public DbSet<Setting> Settings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // مقداردهی اولیه به جدول Settings
            modelBuilder.Entity<Setting>().HasData(
                new Setting
                {
                    Id = 1,
                    LoginMethod = "UserAndPassword" // حالت پیش‌فرض
                }
            );
        }
    }
}