using EcoRecyclersGreenTech.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using EcoRecyclersGreenTech.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
//Adding Hashing and Ciphers
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<IEncryptionKeyService, EncryptionKeyService>();
builder.Services.AddSingleton<EncryptionService>(sp =>
{
    var keyService = sp.GetRequiredService<IEncryptionKeyService>();
    return new EncryptionService(keyService.GetEncryptionKey());
});

builder.Services.AddScoped<PasswordHasher>();
builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.IdleTimeout = TimeSpan.FromMinutes(30);
});
builder.Services.AddDataProtection();

// Adding Authentication
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

// Adding Connection Database
builder.Services.AddDbContext<DBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Connection"))
);

var app = builder.Build();

app.UseMiddleware<EcoRecyclersGreenTech.Services.SecurityHeadersMiddleware>();

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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();