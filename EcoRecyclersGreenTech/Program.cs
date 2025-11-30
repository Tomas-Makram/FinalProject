using EcoRecyclersGreenTech.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using EcoRecyclersGreenTech.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using EcoRecyclersGreenTech.Data.Users;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// Password Hasher
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

// Add Session Security
builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.IdleTimeout = TimeSpan.FromMinutes(30);
});

// Data Protection Configuration
var keysFolder = Path.Combine(builder.Environment.ContentRootPath, "data-protection-keys");
Directory.CreateDirectory(keysFolder);

builder.Services.AddDataProtection()
    .SetApplicationName("EcoRecyclersGreenTech")
    .PersistKeysToFileSystem(new DirectoryInfo(keysFolder));

// Authentication Cookies
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.AccessDeniedPath = "/Home/Error";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    });

// Register Custom Services
builder.Services.AddScoped<IDataCiphers, DataCiphers>();
builder.Services.AddScoped<PasswordHasher, PasswordHasherService>();


// Database Connection
builder.Services.AddDbContext<DBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Connection"))
);

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();

// Routing Secure
app.Use(async (context, next) =>
{
    bool sessionEmpty = !context.Session.GetInt32("UserID").HasValue;

    if (sessionEmpty)
    {
        context.Session.Clear();
        await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    }

    await next();
});

app.UseAuthentication();
app.UseAuthorization();

// Middleware Security (Routing)
app.UseMiddleware<SecurityHeadersMiddleware>();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();