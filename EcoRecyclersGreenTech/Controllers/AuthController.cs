using Microsoft.AspNetCore.Mvc;
using EcoRecyclersGreenTech.Models;
using EcoRecyclersGreenTech.Models.Auth;
using EcoRecyclersGreenTech.Services;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Net;
using System.Globalization;
using EcoRecyclersGreenTech.Data.Users;
using Stripe.Terminal;

namespace EcoRecyclersGreenTech.Controllers
{
    public class AuthController : Controller
    {
        private readonly DBContext _db; // Connect with database
        private readonly DataHasher _passwordHasher; // Using Hashing Code
        private readonly IDataCiphers _dataProtection; // Using En/De Cryption Data
        private readonly IEmailTemplateService _emailTemplate; // Using Templates Emails
        private readonly IOtpService _otpService; // Using OTP Services
        private readonly ILocationService _locationService;
        private readonly ILogger<AuthController> _logger; // Using Validate AntiForgery Token Key
        private readonly int maxInputIncorrectPassword = 20; // Max Input incorrect Password after that block account 
        const decimal welcomeBonus = 50m; // Welcome bonus

        public AuthController(DBContext db, DataHasher passwordHasher, IDataCiphers dataProtection, ILogger<AuthController> logger, IEmailTemplateService emailTemplate, IOtpService otpService, ILocationService locationService)
        {
            _db = db;
            _passwordHasher = passwordHasher;
            _dataProtection = dataProtection;
            _logger = logger;
            _emailTemplate = emailTemplate;
            _otpService = otpService;
            _locationService = locationService;
        }

        [HttpGet]
        public async Task<IActionResult> ReverseGeocode(decimal lat, decimal lng, CancellationToken ct)
        {
            var address = await _locationService.ReverseGeocodeAsync(lat, lng, ct);
            return Json(new { address });
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginModel login)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Error = "Please check your input";
                return View();
            }

            try
            {
                var input = login.username?.Trim() ?? "";
                var emailHasher = _passwordHasher.HashComparison(input.ToLowerInvariant());
                var phoneHasher = _passwordHasher.HashComparison(input);

                var user = await _db.Users
                    .Include(u => u.UserType)
                    .FirstOrDefaultAsync(u => u.HashEmail == emailHasher || u.HashPhoneNumber == phoneHasher);

                if (user == null)
                {
                    await Task.Delay(Random.Shared.Next(800, 1500));
                    ViewBag.Error = "Invalid login credentials";
                    _logger.LogWarning("Failed login attempt for: {User}", input);
                    return View();
                }

                if (user.Blocked)
                {
                    ViewBag.Error = "Your account is blocked. Contact support.";
                    _logger.LogWarning("Blocked account login attempt: {UserId}", user.UserId);
                    return View();
                }

                if (!user.Verified)
                {
                    TempData["VerificationMessage"] =
                            $"Welcome {user.FullName}! Please go to settings to verify your account.";
                    TempData["ShowVerificationAlert"] = "true";
                }

                if (_passwordHasher.VerifyHashed(login.password!, user.HashPassword!))
                {
                    // regenerate session
                    HttpContext.Session.Clear();
                    await HttpContext.Session.CommitAsync();

                    var decryptedEmail = _dataProtection.Decrypt(user.Email!);
                    var decryptedPhone = user.phoneNumber != null ? _dataProtection.Decrypt(user.phoneNumber) : null;

                    HttpContext.Session.SetString("UserEmail", decryptedEmail);
                    HttpContext.Session.SetInt32("UserID", user.UserId);
                    HttpContext.Session.SetString("UserName", user.FullName ?? "");
                    HttpContext.Session.SetInt32("UserTypeID", user.UserTypeID);
                    HttpContext.Session.SetString("PhoneNumber", decryptedPhone ?? "");

                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, decryptedEmail),
                        new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                        new Claim("IsVerified", user.Verified ? "true" : "false"),
                        new Claim(ClaimTypes.Role, user.UserType.TypeName),
                        new Claim("UserType", user.UserType.TypeName)
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = true,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30),
                        AllowRefresh = true
                    };

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties
                    );

                    user.FailedLoginAttempts = 0;
                    await _db.SaveChangesAsync();

                    _logger.LogInformation("User {Email} logged in successfully.", decryptedEmail);
                    return RedirectToAction("Index", "Home");
                }

                user.FailedLoginAttempts++;
                if (user.FailedLoginAttempts >= maxInputIncorrectPassword)
                    user.Blocked = true;

                await _db.SaveChangesAsync();
                await Task.Delay(Random.Shared.Next(800, 1500));

                ViewBag.Error = "Invalid login credentials";
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login process");
                ViewBag.Error = "An error occurred during login. Please try again.";
                return View();
            }
        }

        [HttpGet]
        public IActionResult Signup() => View(new SignupModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Signup(SignupModel signup)
        {
            if (signup?.user == null || signup.type == null)
            {
                ViewBag.Error = "Please check your input data";
                return View(signup);
            }

            // =========================
            // Location extraction
            // =========================
            var loc = _locationService.ExtractAndValidateFromForm(
                signup.user.Latitude?.ToString(CultureInfo.InvariantCulture),
                signup.user.Longitude?.ToString(CultureInfo.InvariantCulture),
                signup.user.Address
            );

            if (!loc.IsValid)
            {
                ViewBag.Error = loc.Error;
                return View(signup);
            }

            var gpsOrMapProvided = loc.Latitude.HasValue && loc.Longitude.HasValue;

            if (gpsOrMapProvided)
            {
                signup.user.Latitude = loc.Latitude;
                signup.user.Longitude = loc.Longitude;

                if (string.IsNullOrWhiteSpace(loc.Address))
                {
                    var rev = await _locationService.ReverseGeocodeAsync(loc.Latitude!.Value, loc.Longitude!.Value);
                    signup.user.Address = rev;
                }
                else
                {
                    signup.user.Address = loc.Address;
                }
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(loc.Address))
                {
                    var geo = await _locationService.GetLocationFromAddressAsync(loc.Address);
                    if (geo != null)
                    {
                        signup.user.Latitude = geo.Latitude;
                        signup.user.Longitude = geo.Longitude;
                        signup.user.Address = string.IsNullOrWhiteSpace(geo.NormalizedAddress) ? loc.Address : geo.NormalizedAddress;
                    }
                    else
                    {
                        signup.user.Latitude = null;
                        signup.user.Longitude = null;
                        signup.user.Address = loc.Address;
                    }
                }
                else
                {
                    var ip = _locationService.GetClientPublicIp(HttpContext);
                    if (!string.IsNullOrWhiteSpace(ip))
                    {
                        var ipLoc = await _locationService.GetLocationFromIpAsync(ip);
                        if (ipLoc != null)
                        {
                            signup.user.Latitude = ipLoc.Latitude;
                            signup.user.Longitude = ipLoc.Longitude;
                            signup.user.Address = ipLoc.AddressFromIp;
                        }
                        else
                        {
                            ViewBag.Error = "We couldn't detect your location. Please select it on the map or type an address hint.";
                            return View(signup);
                        }
                    }
                    else
                    {
                        ViewBag.Error = "We couldn't detect your IP. Please select your location on the map or type an address hint.";
                        return View(signup);
                    }
                }
            }

            // =========================
            // Normalize + pre-validate
            // =========================
            signup.user.Email = signup.user.Email?.Trim().ToLowerInvariant();
            signup.user.phoneNumber = signup.user.phoneNumber?.Trim();

            if (string.IsNullOrWhiteSpace(signup.user.Email) || string.IsNullOrWhiteSpace(signup.user.HashPassword))
            {
                ViewBag.Error = "Email and password are required.";
                return View(signup);
            }

            var emailHash = _passwordHasher.HashComparison(signup.user.Email);
            var phoneHash = !string.IsNullOrWhiteSpace(signup.user.phoneNumber)
                ? _passwordHasher.HashComparison(signup.user.phoneNumber)
                : null;

            if (await _db.Users.AsNoTracking().AnyAsync(u => u.HashEmail == emailHash))
            {
                ViewBag.Error = "This email is already registered";
                return View(signup);
            }

            if (!string.IsNullOrWhiteSpace(phoneHash) &&
                await _db.Users.AsNoTracking().AnyAsync(u => u.HashPhoneNumber == phoneHash))
            {
                ViewBag.Error = "This phone number is already registered";
                return View(signup);
            }

            // =========================
            // Encrypt/hash
            // =========================
            signup.user.HashPassword = _passwordHasher.HashData(signup.user.HashPassword);
            signup.user.HashEmail = emailHash;

            if (!string.IsNullOrWhiteSpace(signup.user.phoneNumber))
            {
                signup.user.HashPhoneNumber = phoneHash;
                signup.user.phoneNumber = _dataProtection.Encrypt(signup.user.phoneNumber);
            }

            signup.user.Email = _dataProtection.Encrypt(signup.user.Email);

            // =========================
            // Defaults
            // =========================
            signup.user.JoinDate = DateTime.UtcNow;
            signup.user.Verified = false;
            signup.user.Blocked = false;
            signup.user.FailedLoginAttempts = 0;

            signup.user.OtpHash = "";
            signup.user.OtpExpiresAt = null;
            signup.user.OtpLastSentAt = null;
            signup.user.OtpAttempts = 0;
            signup.user.OtpRequestsCount = 0;

            signup.user.PasswordResetOtpHash = null;
            signup.user.PasswordResetOtpExpiresAt = null;
            signup.user.PasswordResetOtpAttempts = 0;
            signup.user.LastMailSentResetPasswordAt = null;
            signup.user.PasswordOtpResetCount = 0;

            signup.user.MailActionsCount = 0;
            signup.user.MailActionsResetAt = DateTime.UtcNow;
            signup.user.MailBlockedUntil = null;
            signup.user.ValidationOtpWindowResetAt = DateTime.UtcNow;
            signup.user.ResetOtpWindowResetAt = DateTime.UtcNow;
            signup.user.OtpVerifyBlockedUntil = null;
            signup.user.ResetOtpVerifyBlockedUntil = null;

            // =========================
            // Validate type + get TypeID
            // =========================
            var typeName = signup.type.TypeName?.Trim();

            if (string.IsNullOrWhiteSpace(typeName))
            {
                ViewBag.Error = "Invalid user type";
                return View(signup);
            }

            var userTypeId = await _db.UserTypes.AsNoTracking()
                .Where(t => t.TypeName == typeName)
                .Select(t => t.TypeID)
                .FirstOrDefaultAsync();

            if (userTypeId <= 0)
            {
                ViewBag.Error = "Invalid user type (not found in UserTypes table).";
                return View(signup);
            }

            // =========================
            // DB work with ExecutionStrategy + Transaction
            // =========================
            var strategy = _db.Database.CreateExecutionStrategy();

            string? decryptedEmailAfterCommit = null;

            try
            {
                await strategy.ExecuteAsync(async () =>
                {
                    await using var transaction = await _db.Database.BeginTransactionAsync();
                    try
                    {
                        // =========================
                        // Insert User first (to get UserId)
                        // =========================
                        signup.user.UserTypeID = userTypeId;

                        _db.Users.Add(signup.user);
                        await _db.SaveChangesAsync();

                        var newUserId = signup.user.UserId;

                        // =========================
                        // ✅ Create Wallet for this new user
                        // =========================
                        var wallet = new Wallet
                        {
                            UserId = newUserId,
                            Balance = 0m,
                            ReservedBalance = 0m,
                            CreatedAt = DateTime.UtcNow
                        };

                        _db.Wallets.Add(wallet);
                        await _db.SaveChangesAsync(); // to generate wallet.Id

                        // =========================
                        // ✅ Optional: Welcome Bonus
                        // =========================
                        if (welcomeBonus > 0m)
                        {
                            wallet.Balance += welcomeBonus;

                            var welcomeTxn = new WalletTransaction
                            {
                                WalletId = wallet.Id,
                                Type = WalletTxnType.Bonus,
                                Status = WalletTxnStatus.Succeeded,
                                Amount = welcomeBonus,
                                BalanceAfter = wallet.Balance,
                                Currency = "EGP",
                                Note = "Welcome bonus",
                                IdempotencyKey = $"welcome-bonus-{newUserId}",
                                CreatedAt = DateTime.UtcNow
                            };

                            _db.WalletTransactions.Add(welcomeTxn);
                            await _db.SaveChangesAsync();
                        }

                        // =========================
                        // Insert profile by type (with FK UserID)
                        // =========================
                        if (typeName == "Individual")
                        {
                            if (signup.individual == null)
                                throw new InvalidOperationException("Individual data is missing.");

                            signup.individual.UserID = newUserId;   // ✅ FK
                            _db.Individuals.Add(signup.individual);
                        }
                        else if (typeName == "Factory")
                        {
                            if (signup.factory == null)
                                throw new InvalidOperationException("Factory data is missing.");

                            signup.factory.UserID = newUserId;      // ✅ FK
                            _db.Factories.Add(signup.factory);
                        }
                        else if (typeName == "Craftsman")
                        {
                            if (signup.craftsman == null)
                                throw new InvalidOperationException("Craftsman data is missing.");

                            signup.craftsman.UserID = newUserId;    // ✅ FK
                            _db.Craftsmen.Add(signup.craftsman);
                        }
                        else if (typeName == "Admin")
                        {
                            if (signup.admin == null)
                                throw new InvalidOperationException("Admin data is missing.");

                            signup.admin.UserID = newUserId;        // ✅ FK
                            _db.Admins.Add(signup.admin);
                        }
                        else
                        {
                            throw new InvalidOperationException("Invalid user type");
                        }

                        await _db.SaveChangesAsync();

                        await transaction.CommitAsync();

                        decryptedEmailAfterCommit = _dataProtection.Decrypt(signup.user.Email!);
                    }
                    catch
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                });

                // =========================
                // Email after commit (non-critical)
                // =========================
                try
                {
                    await _emailTemplate.SendEmailAsync(
                        _emailTemplate.CreateWelcomeEmail(decryptedEmailAfterCommit!, signup.user.FullName)
                    );

                    TempData["VerificationMessage"] =
                        $"Welcome {signup.user.FullName}! Please go to settings to verify your account.";
                    TempData["ShowVerificationAlert"] = "true";
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send welcome email");
                    TempData["VerificationMessage"] =
                        "Account created! But we couldn't send the welcome email. Contact support.";
                    TempData["ShowVerificationAlert"] = "true";
                }

                return RedirectToAction("Login", "Auth");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Signup failed");
                ViewBag.Error = "An error occurred during registration. Please try again.";
                return View(signup);
            }
        }

        [HttpGet]
        public IActionResult ForgetPassword() => View(new ForgetPasswordModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgetPassword(ForgetPasswordModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Always show same message to prevent user enumeration
            ViewBag.SuccessMessage =
                "If an account exists with this email, you will receive a password reset OTP shortly.";

            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                var emailInput = model.Email?.Trim().ToLowerInvariant();
                if (string.IsNullOrWhiteSpace(emailInput))
                {
                    await Task.Delay(Random.Shared.Next(800, 1500));
                    return View(new ForgetPasswordModel());
                }

                var emailHash = _passwordHasher.HashComparison(emailInput);

                var user = await _db.Users.FirstOrDefaultAsync(u => u.HashEmail == emailHash);

                // If not found -> same response, timing padding
                if (user == null)
                {
                    await Task.Delay(Random.Shared.Next(800, 1500));
                    await transaction.RollbackAsync();
                    return View(new ForgetPasswordModel());
                }

                // If account blocked -> we still respond same (no disclosure)
                if (user.Blocked)
                {
                    await Task.Delay(Random.Shared.Next(800, 1500));
                    await transaction.RollbackAsync();
                    return View(new ForgetPasswordModel());
                }

                // Generate OTP (OtpService handles global + per-flow limits/cooldown)
                var otpCode = await _otpService.SendOTPResetPassword(user.UserId);

                if (string.IsNullOrEmpty(otpCode))
                {
                    await Task.Delay(Random.Shared.Next(800, 1500));
                    await transaction.RollbackAsync();
                    return View(new ForgetPasswordModel());
                }

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                // Send email outside DB transaction is ok, but you already committed above.
                try
                {
                    var decryptedEmail = _dataProtection.Decrypt(user.Email!);

                    await _emailTemplate.SendEmailAsync(
                        _emailTemplate.CreatePasswordResetOtpEmail(decryptedEmail, otpCode, user.FullName)
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send password reset OTP email");
                }

                // return view
                return RedirectToAction("ResetPassword", "Auth");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "ForgetPassword transaction failed");

                // same message regardless
                ViewBag.SuccessMessage =
                    "If an account exists with this email, you will receive a password reset OTP shortly.";

                return View(new ForgetPasswordModel());
            }
        }

        [HttpGet]
        public IActionResult ResetPassword() => View(new ResetPasswordModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var emailInput = model.Email?.Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(emailInput))
            {
                ViewBag.Error = "Invalid email, OTP, or password.";
                return View(new ResetPasswordModel());
            }

            if (string.IsNullOrWhiteSpace(model.NewPassword) ||
                model.NewPassword != model.ConfirmPassword)
            {
                ViewBag.Error = "Invalid email, OTP, or password.";
                return View(new ResetPasswordModel());
            }

            var emailHash = _passwordHasher.HashComparison(emailInput);

            // Load user with UserType for claims
            var user = await _db.Users
                .Include(u => u.UserType)
                .FirstOrDefaultAsync(u => u.HashEmail == emailHash);

            if (user == null || user.Blocked)
            {
                ViewBag.Error = "Invalid email, OTP, or password.";
                return View(new ResetPasswordModel());
            }

            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                // Verify reset OTP
                if (string.IsNullOrWhiteSpace(model.OTP) ||
                    !await _otpService.VerifyOTPPasswordAsync(user.UserId, model.OTP.Trim()))
                {
                    await transaction.RollbackAsync();
                    ViewBag.Error = "Invalid email, OTP, or password.";
                    return View(new ResetPasswordModel());
                }

                // Track user inside transaction
                var trackedUser = await _db.Users
                    .Include(u => u.UserType)
                    .FirstOrDefaultAsync(u => u.UserId == user.UserId);

                if (trackedUser == null)
                    throw new Exception("User not found during transaction");

                // Update password
                trackedUser.HashPassword = _passwordHasher.HashData(model.NewPassword);
                trackedUser.FailedLoginAttempts = 0;

                // Defense in depth: invalidate reset OTP fields
                trackedUser.PasswordResetOtpHash = null;
                trackedUser.PasswordResetOtpExpiresAt = null;
                trackedUser.PasswordResetOtpAttempts = 0;
                trackedUser.ResetOtpVerifyBlockedUntil = null;

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                // AUTO LOGIN

                // Decrypt identifiers
                var decryptedEmail = _dataProtection.Decrypt(trackedUser.Email!);
                var decryptedPhone = trackedUser.phoneNumber != null
                    ? _dataProtection.Decrypt(trackedUser.phoneNumber)
                    : null;

                // clear and commit a fresh session state
                HttpContext.Session.Clear();
                await HttpContext.Session.CommitAsync();

                HttpContext.Session.SetString("UserEmail", decryptedEmail);
                HttpContext.Session.SetInt32("UserID", trackedUser.UserId);
                HttpContext.Session.SetString("UserName", trackedUser.FullName ?? "");
                HttpContext.Session.SetInt32("UserTypeID", trackedUser.UserTypeID);
                HttpContext.Session.SetString("PhoneNumber", decryptedPhone ?? "");

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, decryptedEmail),
                    new Claim(ClaimTypes.NameIdentifier, trackedUser.UserId.ToString()),
                    new Claim("IsVerified", trackedUser.Verified ? "true" : "false"),
                    new Claim("UserType", trackedUser.UserType.TypeName)
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30),
                    AllowRefresh = true
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties
                );

                TempData["Success"] = "Password updated successfully.";
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "ResetPassword failed");
                ViewBag.Error = "Invalid email, OTP, or password.";
                return View(new ResetPasswordModel());
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

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
            }

            return RedirectToAction("Index", "Home");
        }
    }
}