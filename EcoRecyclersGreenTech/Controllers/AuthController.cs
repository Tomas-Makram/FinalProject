using Microsoft.AspNetCore.Mvc;
using EcoRecyclersGreenTech.Models;
using EcoRecyclersGreenTech.Services;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using EcoRecyclersGreenTech.Data.Users;

namespace EcoRecyclersGreenTech.Controllers
{
    public class AuthController : Controller
    {
        private readonly DBContext _db;
        private readonly PasswordHasher _passwordHasher;
        private readonly EncryptionService _encryptionService;
        private readonly ILogger<AuthController> _logger;
        public AuthController(DBContext db, PasswordHasher hasher, EncryptionService chipher, ILogger<AuthController> logger)
        {
            _db = db;
            _passwordHasher = hasher;
            _encryptionService = chipher;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginDataModel login)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Error = "Please check your input";
                return View();
            }

            // Encrypting email or phone number for database search
            string encryptedInput = _encryptionService.Encrypt(login.username!);

            // User search
            var user = _db.Users
                .Include(u => u.UserType)
                .FirstOrDefault(u =>
                    u.Email == encryptedInput ||
                    u.phoneNumber == encryptedInput);

            // Random delay if the user is not found
            if (user == null)
            {
                var rand = new Random();
                await Task.Delay(rand.Next(800, 1500));

                ViewBag.Error = "Invalid login credentials";
                _logger.LogWarning($"Failed login attempt for non-existing username: {login.username}");
                return View();
            }

            // Checking for a ban
            if (user.Blocked)
            {
                ViewBag.Error = "Your account is blocked. Contact support.";
                _logger.LogWarning($"Blocked account login attempt: {login.username}");
                return View();
            }

            // Password verification
            if (_passwordHasher.VerifyPassword(login.password!, user.HashPassword!))
            {
                // Regenerate Session ID (Session Fixation protection)
                await HttpContext.Session.CommitAsync();

                // Decryption before recording the session
                string decryptedEmail = _encryptionService.Decrypt(user.Email!);
                string? decryptedPhone = user.phoneNumber != null ? _encryptionService.Decrypt(user.phoneNumber) : null;

                // Securely recording data during the session
                HttpContext.Session.SetString("UserEmail", decryptedEmail);
                HttpContext.Session.SetInt32("UserID", user.UserID);
                HttpContext.Session.SetString("UserName", user.FullName ?? "");
                HttpContext.Session.SetInt32("UserTypeID", user.UserTypeID);
                HttpContext.Session.SetString("PhoneNumber", decryptedPhone ?? "");

                // Setting up claims for authentication
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, decryptedEmail),
                    new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString())
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30),
                    AllowRefresh = true
                };

                // Log in using Authentication
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties
                );

                // Reset failed login attempts
                user.FailedLoginAttempts = 0;
                await _db.SaveChangesAsync();

                _logger.LogInformation($"User {decryptedEmail} logged in successfully.");
                return RedirectToAction("Index", "Home");
            }
            else
            {
                // Increase FailedLoginAttempts and block after a specified number
                user.FailedLoginAttempts++;
                _logger.LogWarning($"Failed login attempt {user.FailedLoginAttempts} for user: {user.Email}");

                if (user.FailedLoginAttempts >= 20)
                {
                    user.Blocked = true;
                    _logger.LogWarning($"User {user.Email} is blocked due to too many failed login attempts.");
                }

                await _db.SaveChangesAsync();

                // Random delay before replying to prevent Brute Force
                var rand = new Random();
                await Task.Delay(rand.Next(800, 1500));

                ViewBag.Error = "Invalid login credentials";
                return View();
            }
        }

        [HttpGet]
        public IActionResult Signup() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Signup(SignupDataModel signup)
        {
            if (signup == null || signup.user == null || signup.type == null)
            {
                ViewBag.Error = "Please check your input data";
                return View();
            }

            using var transaction = await _db.Database.BeginTransactionAsync();

            try
            {
                // Encrypt email and phone number before saving
                signup.user.Email = _encryptionService.Encrypt(signup.user.Email!);
                if (!string.IsNullOrEmpty(signup.user.phoneNumber))
                    signup.user.phoneNumber = _encryptionService.Encrypt(signup.user.phoneNumber!);

                // Check for pre-encrypted email
                if (_db.Users.Any(u => u.Email == signup.user.Email))
                {
                    ViewBag.Error = "This email is already registered";
                    return View();
                }

                // Password hash
                signup.user.HashPassword = _passwordHasher.HashPassword(signup.user.HashPassword!);

                // Set default values
                signup.user.JoinDate = DateTime.Now;
                signup.user.Verified = false;
                signup.user.Blocked = false;
                signup.user.FailedLoginAttempts = 0;

                // Save additional type data first
                int realTypeId = 0;

                switch (signup.type!.TypeName)
                {
                    case "Individual":
                        _db.Individuals.Add(signup.individual!);
                        await _db.SaveChangesAsync();
                        realTypeId = signup.individual!.IndividualID;
                        break;

                    case "Factory":
                        _db.Factories.Add(signup.factory!);
                        await _db.SaveChangesAsync();
                        realTypeId = signup.factory!.FactoryID;
                        break;

                    case "Craftsman":
                        _db.Craftsmen.Add(signup.craftsman!);
                        await _db.SaveChangesAsync();
                        realTypeId = signup.craftsman!.CraftsmanID;
                        break;
                }

                // Save UserType with the correct RealTypeID
                signup.type!.RealTypeID = realTypeId;
                _db.UserTypes.Add(signup.type);
                await _db.SaveChangesAsync();

                // Linking the user to the type
                signup.user.UserTypeID = signup.type.TypeID;
                _db.Users.Add(signup.user);
                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                // Securely recording data during the session
                string decryptedEmail = _encryptionService.Decrypt(signup.user.Email!);
                string? decryptedPhone = signup.user.phoneNumber != null ? _encryptionService.Decrypt(signup.user.phoneNumber) : null;

                HttpContext.Session.SetString("UserEmail", decryptedEmail);
                HttpContext.Session.SetInt32("UserID", signup.user.UserID);
                HttpContext.Session.SetString("UserName", signup.user.FullName ?? "");
                HttpContext.Session.SetInt32("UserTypeID", signup.user.UserTypeID);
                HttpContext.Session.SetString("PhoneNumber", decryptedPhone ?? "");

                // Preparing Claims for Authentication
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, decryptedEmail),
                    new Claim(ClaimTypes.NameIdentifier, signup.user.UserID.ToString())
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30),
                    AllowRefresh = true
                };

                // Log in using Authentication
                await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties
                );

                _logger.LogInformation($"New user registered: {signup.user.Email}");
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error during user registration");
                ViewBag.Error = "An error occurred during registration: " + ex.Message;
                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var userEmail = HttpContext.Session.GetString("UserEmail");

                // Clear all session data
                HttpContext.Session.Clear();

                // Log out of Authentication
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                // Recreate the session to prevent Session Fixation attacks
                await HttpContext.Session.CommitAsync();

                _logger.LogInformation($"User {userEmail} logged out successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
            }

            return RedirectToAction("Index", "Home");
        }
    }
}
