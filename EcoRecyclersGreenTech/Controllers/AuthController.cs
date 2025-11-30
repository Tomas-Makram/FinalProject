using Microsoft.AspNetCore.Mvc;
using EcoRecyclersGreenTech.Models;
using EcoRecyclersGreenTech.Services;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;

namespace EcoRecyclersGreenTech.Controllers
{
    public class AuthController : Controller
    {
        private readonly DBContext _db;
        private readonly PasswordHasher _passwordHasher;
        private readonly IDataCiphers _dataProtection;
        private readonly ILogger<AuthController> _logger;

        public AuthController(DBContext db, PasswordHasher passwordHasher, IDataCiphers dataProtection, ILogger<AuthController> logger)
        {
            _db = db;
            _passwordHasher = passwordHasher;
            _dataProtection = dataProtection;
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

            try
            {
                // Search all users with decryption for comparison
                var users = _db.Users
                    .Include(u => u.UserType)
                    .ToList();

                var user = users.FirstOrDefault(u =>
                {
                    try
                    {
                        string decryptedEmail = _dataProtection.Decrypt(u.Email!);
                        string? decryptedPhone = u.phoneNumber != null ?
                            _dataProtection.Decrypt(u.phoneNumber) : null;

                        return decryptedEmail == login.username ||
                               decryptedPhone == login.username;
                    }
                    catch
                    {
                        return false;
                    }
                });

                if (user == null)
                {
                    var rand = new Random();
                    await Task.Delay(rand.Next(800, 1500));
                    ViewBag.Error = "Invalid login credentials";
                    _logger.LogWarning($"Failed login attempt for: {login.username}");
                    return View();
                }

                // Checking the block
                if (user.Blocked)
                {
                    ViewBag.Error = "Your account is blocked. Contact support.";
                    _logger.LogWarning($"Blocked account login attempt: {login.username}");
                    return View();
                }

                // Password verification
                if (_passwordHasher.VerifyPassword(login.password!, user.HashPassword!))
                {
                    // Recreate Session ID for protection
                    await HttpContext.Session.CommitAsync();

                    // Decrypt data to saving the session
                    string decryptedEmail = _dataProtection.Decrypt(user.Email!);
                    string? decryptedPhone = user.phoneNumber != null ?
                        _dataProtection.Decrypt(user.phoneNumber) : null;

                    // Save data in session
                    HttpContext.Session.SetString("UserEmail", decryptedEmail);
                    HttpContext.Session.SetInt32("UserID", user.UserID);
                    HttpContext.Session.SetString("UserName", user.FullName ?? "");
                    HttpContext.Session.SetInt32("UserTypeID", user.UserTypeID);
                    HttpContext.Session.SetString("PhoneNumber", decryptedPhone ?? "");

                    // Setting up Claims for authentication
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
                    // Increase in failed attempts
                    user.FailedLoginAttempts++;
                    _logger.LogWarning($"Failed login attempt {user.FailedLoginAttempts} for user: {user.Email}");

                    if (user.FailedLoginAttempts >= 20)
                    {
                        user.Blocked = true;
                        _logger.LogWarning($"User {user.Email} is blocked due to too many failed login attempts.");
                    }

                    await _db.SaveChangesAsync();

                    // Random delay to prevent Brute Force attacks
                    var rand = new Random();
                    await Task.Delay(rand.Next(800, 1500));

                    ViewBag.Error = "Invalid login credentials";
                    return View();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login process");
                ViewBag.Error = "An error occurred during login. Please try again.";
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
                // Email verification is using by email
                var allUsers = _db.Users.ToList();
                bool emailExists = allUsers.Any(u =>
                {
                    try
                    {
                        string decryptedEmail = _dataProtection.Decrypt(u.Email!);
                        return decryptedEmail == signup.user.Email;
                    }
                    catch
                    {
                        return false;
                    }
                });

                if (emailExists)
                {
                    ViewBag.Error = "This email is already registered";
                    return View();
                }

                // Email encryption and storage
                signup.user.Email = _dataProtection.Encrypt(signup.user.Email!);
                if (!string.IsNullOrEmpty(signup.user.phoneNumber))
                    signup.user.phoneNumber = _dataProtection.Encrypt(signup.user.phoneNumber!);

                // Password hash using built-in password hasher
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
                string decryptedEmail = _dataProtection.Decrypt(signup.user.Email!);
                string? decryptedPhone = signup.user.phoneNumber != null ? _dataProtection.Decrypt(signup.user.phoneNumber) : null;

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

                _logger.LogInformation($"New user registered: {decryptedEmail}");
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