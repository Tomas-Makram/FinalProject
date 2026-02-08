using EcoRecyclersGreenTech.Models;
using EcoRecyclersGreenTech.Services;
using EcoRecyclersGreenTech.Data.Users;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();

// Emails
builder.Services.AddTransient<IEmailSender, MailService>();
builder.Services.AddScoped<IEmailTemplateService, MailService>();

// App Services
builder.Services.AddSingleton<IThemeService, ThemeService>();
builder.Services.AddScoped<IImageStorageService, ImageStorageService>();
builder.Services.AddScoped<IOtpService, OtpService>();
builder.Services.AddScoped<StripePaymentService>();

builder.Services.AddScoped<IFactoryStoreService, FactoryStoreService>();
builder.Services.AddScoped<IFilterAIService, FilterAIService>();

// Password Hasher
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

//// Pricing Options
//builder.Services.Configure<PricingOptions>(opt =>
//{
//    opt.PlatformFeePercent = 0.02m; // 2%
//    opt.DepositPercent = 0.10m;     // 10%
//});

// Location Service (HttpClient)
builder.Services.AddHttpClient<ILocationService, LocationService>();

// Database
builder.Services.AddDbContext<DBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Connection"))
);

// Session
builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.IsEssential = true;
    options.IdleTimeout = TimeSpan.FromMinutes(30);
});

// Data Protection (Persist Keys)
var keysFolder = Path.Combine(builder.Environment.ContentRootPath, "data-protection-keys");
Directory.CreateDirectory(keysFolder);

builder.Services.AddDataProtection()
    .SetApplicationName("EcoRecyclersGreenTech")
    .PersistKeysToFileSystem(new DirectoryInfo(keysFolder));

// Authentication (Cookies)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.AccessDeniedPath = "/Home/Error";

        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Lax;   // ✅ يسمح بالرجوع من Stripe
        options.Cookie.IsEssential = true;

        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = true;
    });

// Custom Services
builder.Services.AddScoped<IDataCiphers, DataCiphers>();
builder.Services.AddScoped<DataHasher, PasswordHasherService>();

var app = builder.Build();

// Pipeline
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

// Authentication/Authorization
app.UseAuthentication();
app.UseAuthorization();

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

// Middleware Security Headers (CSP + Routing)
app.UseMiddleware<SecurityHeadersMiddleware>();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();