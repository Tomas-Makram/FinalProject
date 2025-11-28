using Microsoft.AspNetCore.Mvc;
using EcoRecyclersGreenTech.Models;
using EcoRecyclersGreenTech.Services;
using Microsoft.EntityFrameworkCore;

namespace EcoRecyclersGreenTech.Controllers
{
    public class AuthController : Controller
    {
        private readonly DBContext _db;
        private readonly PasswordHasher _passwordHasher;
        private readonly ILogger<AuthController> _logger;
        public AuthController(DBContext db, ILogger<AuthController> logger)
        {
            _db = db;
            _passwordHasher = new PasswordHasher();
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginDataModel login)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    ViewBag.Error = "Please check your input";
                    return View();
                }

                var user = _db.Users
                    .Include(u => u.UserType)
                    .FirstOrDefault(u =>
                        u.Email == login.username ||
                        u.phoneNumber == login.username ||
                        u.FullName == login.username);

                if (user != null && PasswordHasher.VerifyPassword(login.password!, user.HashPassword!))
                {
                    // Secure session information recording
                    HttpContext.Session.SetString("UserEmail", user.Email!);
                    HttpContext.Session.SetInt32("UserID", user.UserID);
                    HttpContext.Session.SetString("UserName", user.FullName ?? "");
                    HttpContext.Session.SetInt32("UserTypeID", user.UserTypeID);

                    // Log in
                    _logger.LogInformation($"User {user.Email} logged in successfully.");

                    return RedirectToAction("Index", "Home");
                }

                // Recording failed login attempt
                _logger.LogWarning($"Failed login attempt for username: {login.username}");

                ViewBag.Error = "Invalid login credentials";
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login process");
                ViewBag.Error = "An error occurred during login";
                return View();
            }
        }

        [HttpGet]
        public IActionResult Signup()
        {
            return View();
        }

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
                // Check for duplicate emails
                if (_db.Users.Any(u => u.Email == signup.user.Email))
                {
                    ViewBag.Error = "This email is already registered";
                    return View();
                }

                // Password hash
                signup.user.HashPassword = PasswordHasher.HashPassword(signup.user.HashPassword!);
                Console.WriteLine(signup.user.HashPassword);
                Console.WriteLine(signup.user.HashPassword);
                // Set default values
                signup.user.JoinDate = DateTime.Now;
                signup.user.Verified = false;
                signup.user.Blocked = false;

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

                // Secure session recording
                HttpContext.Session.SetString("UserEmail", signup.user.Email!);
                HttpContext.Session.SetInt32("UserID", signup.user.UserID);
                HttpContext.Session.SetString("UserName", signup.user.FullName ?? "");
                HttpContext.Session.SetInt32("UserTypeID", signup.user.UserTypeID);

                // Successful registration process recorded
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
        public IActionResult Logout()
        {
            try
            {
                var userEmail = HttpContext.Session.GetString("UserEmail");

                // Clear all session data
                HttpContext.Session.Clear();

                // Recreate the session to prevent Session Fixation attacks
                HttpContext.Session.CommitAsync();

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
