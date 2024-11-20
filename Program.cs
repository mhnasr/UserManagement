using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using UserManagement.Data;
using UserManagement.Helpers;
using UserManagement.Models;
using UserManagement.Services;

var builder = WebApplication.CreateBuilder(args);

// تنظیم Serilog برای لاگ‌برداری
builder.Host.UseSerilog((context, configuration) =>
{
    configuration.WriteTo.Console()
                 .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day);
});

// تنظیمات دیتابیس
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

//اضافه کردن سرویس پیامکی
builder.Services.AddHttpClient<SMSService>();


builder.Services.Configure<IdentityOptions>(options =>
{
    // تنظیمات رمز عبور
    options.Password.RequireDigit = false; // عدم نیاز به عدد
    options.Password.RequiredLength = 6; // حداقل طول رمز عبور
    options.Password.RequireNonAlphanumeric = false; // عدم نیاز به کاراکتر خاص
    options.Password.RequireUppercase = false; // عدم نیاز به حروف بزرگ
    options.Password.RequireLowercase = false; // عدم نیاز به حروف کوچک
});

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders()
    .AddErrorDescriber<CustomIdentityErrorDescriber>();


// تنظیم Authentication با JWT (اختیاری)
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = "JwtBearer";
    options.DefaultChallengeScheme = "JwtBearer";
})
.AddJwtBearer("JwtBearer", options =>
{
    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            System.Text.Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])
        )
    };
});

// پیکربندی Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // زمان انقضا
    options.Cookie.HttpOnly = true; // محافظت از کوکی
    options.Cookie.IsEssential = true; // ضروری برای کار با Session
});

// تنظیم مسیرهای کوکی‌ها
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

// تنظیم AutoMapper
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// افزودن FluentValidation (اختیاری)
builder.Services.AddControllersWithViews()
    .AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<Program>());

var app = builder.Build();

// استفاده از Middleware‌ها
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseSession();
app.UseRouting();

app.UseAuthentication(); // برای Identity و JWT
app.UseAuthorization();  // برای کنترل دسترسی


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

// تنظیم مسیرها (شامل Areaها)
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
);
app.MapControllerRoute(
    name: "Account",
    pattern: "Account/{controller=Home}/{action=Index}/{id?}"
);





app.Run();
