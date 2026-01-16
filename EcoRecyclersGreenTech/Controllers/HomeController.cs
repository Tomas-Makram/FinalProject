using System.Diagnostics;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using EcoRecyclersGreenTech.Models;
using EcoRecyclersGreenTech.Models.Home;
using EcoRecyclersGreenTech.Services;
using EcoRecyclersGreenTech.Data.Users;

namespace EcoRecyclersGreenTech.Controllers;

public class HomeController : Controller
{
    private readonly DBContext _db;
    private readonly ILogger<HomeController> _logger;
    private readonly DataHasher _dataHasher;
    private readonly IDataCiphers _dataProtection;
    private readonly IImageStorageService _images;
    private readonly IOtpService _otpService;
    private readonly IEmailTemplateService _emailsTemplate;
    private readonly ILocationService _locationService;

    // Cached per-request (no repeat in every action)
    private User? _currentUser;
    private int _currentUserId;
    private bool _isLoggedIn;

    private static readonly HashSet<string> _showVerifyAlertActions =new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "Index"
    };

    public HomeController(DBContext db, ILogger<HomeController> logger, DataHasher DataHasher, IDataCiphers dataProtection, IOtpService otpService, IEmailTemplateService emailsTemplate, IImageStorageService images, ILocationService locationService)
    {
        _db = db;
        _logger = logger;
        _dataHasher = DataHasher;
        _dataProtection = dataProtection;
        _images = images;
        _otpService = otpService;
        _emailsTemplate = emailsTemplate;
        _locationService = locationService;
    }

    // Session login state + Layout ViewBags + verify banner + blocked check
    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        try
        {
            var actionName = context.ActionDescriptor.RouteValues.TryGetValue("action", out var a) ? a : "";
            var showVerifyAlert = _showVerifyAlertActions.Contains(actionName!);

            var (ok, userId, user) = await ApplySessionLoginStateAsync(showVerifyAlert);

            _isLoggedIn = ok;
            _currentUserId = userId;
            _currentUser = user;

            var endpoint = context.HttpContext.GetEndpoint();
            var requiresAuth = endpoint?.Metadata?.GetMetadata<AuthorizeAttribute>() != null;

            if (requiresAuth && (!ok || user == null))
            {
                context.Result = RedirectToAction("Login", "Auth");
                return;
            }

            if (requiresAuth && user != null && user.Blocked)
            {
                TempData["ErrorMessage"] = "Your account is blocked. Contact support.";
                context.Result = RedirectToAction(nameof(Index));
                return;
            }

            await next();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "HomeController OnActionExecutionAsync failed.");

            TempData["ErrorMessage"] = "Something went wrong. Please try again.";
            context.Result = RedirectToAction(nameof(Index));
        }
    }

    // sets ViewBag.UserLoggedIn, ViewBag.Email, ViewBag.type, ViewBag.IsVerified, ViewBag.NeedsVerification
    private async Task<(bool ok, int userId, User? user)> ApplySessionLoginStateAsync(bool showVerifyAlert = false)
    {
        var emailSession = HttpContext.Session.GetString("UserEmail");
        var userIdSession = HttpContext.Session.GetInt32("UserID");
        var typeIdSession = HttpContext.Session.GetInt32("UserTypeID");

        if (string.IsNullOrWhiteSpace(emailSession) || !userIdSession.HasValue)
        {
            ViewBag.UserLoggedIn = false;
            return (false, 0, null);
        }

        ViewBag.UserLoggedIn = true;
        ViewBag.Email = emailSession;

        // Load user + type name once (no repeated queries in actions)
        var user = await _db.Users
            .AsNoTracking()
            .Include(u => u.UserType)
            .FirstOrDefaultAsync(u => u.UserID == userIdSession.Value);

        if (user == null)
        {
            ViewBag.UserLoggedIn = false;
            return (false, 0, null);
        }

        ViewBag.type = user.UserType?.TypeName ?? await _db.UserTypes
            .AsNoTracking()
            .Where(t => typeIdSession.HasValue && t.TypeID == typeIdSession.Value)
            .Select(t => t.TypeName)
            .FirstOrDefaultAsync();

        ViewBag.IsVerified = user.Verified;
        ViewBag.NeedsVerification = !user.Verified;

        if (showVerifyAlert && !user.Verified)
        {
            TempData["VerificationMessage"] =
                $"Welcome {user.FullName}! Please go to settings to verification your account.";
            TempData["ShowVerificationAlert"] = "true";
        }

        return (true, user.UserID, user);
    }

    // Index
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    // Setting
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Setting()
    {
        // Keep your original logic: use claims identity for authorized actions
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdStr, out var userId))
            return RedirectToAction("Login", "Auth");

        try
        {
            var user = await _db.Users
                .AsNoTracking()
                .Include(u => u.UserType)
                .FirstOrDefaultAsync(u => u.UserID == userId);

            if (user == null)
                return RedirectToAction("Login", "Auth");

            var now = DateTime.UtcNow;

            var email = user.Email != null ? _dataProtection.Decrypt(user.Email) : null;
            var phone = user.phoneNumber != null ? _dataProtection.Decrypt(user.phoneNumber) : null;

            var hasActiveOtp = !string.IsNullOrWhiteSpace(user.OtpHash)
                               && user.OtpExpiresAt.HasValue
                               && user.OtpExpiresAt.Value > now;

            var remainingAttempts = Math.Max(0, _otpService.GetMaxOtpAttempts() - user.OtpAttempts);

            var expiryRemaining = hasActiveOtp && user.OtpExpiresAt.HasValue
                ? user.OtpExpiresAt.Value - now
                : TimeSpan.Zero;
            if (expiryRemaining < TimeSpan.Zero) expiryRemaining = TimeSpan.Zero;

            var canResend = true;
            var resendRemaining = TimeSpan.Zero;
            if (user.OtpLastSentAt.HasValue)
            {
                var since = now - user.OtpLastSentAt.Value;
                var cooldown = TimeSpan.FromMinutes(_otpService.GetResendCooldownMinutes());
                if (since < cooldown)
                {
                    canResend = false;
                    resendRemaining = cooldown - since;
                }
            }

            var verifyBlockedUntil = user.OtpVerifyBlockedUntil.HasValue && user.OtpVerifyBlockedUntil.Value > now
                ? user.OtpVerifyBlockedUntil
                : null;

            var mailBlocked = user.MailBlockedUntil.HasValue && user.MailBlockedUntil.Value > now;

            var vm = new SettingModel
            {
                Profile = new ProfileSection
                {
                    FullName = user.FullName,
                    Email = email,
                    PhoneNumber = phone,
                    Address = user.Address,
                    ProfileImageUrl = user.UserProfileImgURL,
                    JoinDate = user.JoinDate,
                    Latitude = user.Latitude,
                    Longitude = user.Longitude
                },
                Account = new AccountSection
                {
                    UserTypeName = user.UserType?.TypeName,
                    Verified = user.Verified,
                    Blocked = user.Blocked
                },
                Verification = new VerificationSection
                {
                    HasActiveOtp = hasActiveOtp,
                    RemainingAttempts = remainingAttempts,
                    OtpExpiryRemaining = expiryRemaining,
                    CanResendOtp = canResend && !user.Verified && !user.Blocked && !mailBlocked,
                    ResendCooldownRemaining = resendRemaining,
                    VerifyBlockedUntil = verifyBlockedUntil,
                    MailBlocked = mailBlocked,
                    MailBlockedUntil = user.MailBlockedUntil,
                    OpenVerifyModal = (TempData["OpenVerifyModal"]?.ToString() == "true")
                },
                BasicInfo = new BasicInfoForm
                {
                    FullName = user.FullName,
                    Email = email,
                    PhoneNumber = phone,
                    Location = user.Address
                }
            };

            // Type-Specific by UserID (NOT RealTypeID)
            var typeName = user.UserType?.TypeName?.Trim().ToLowerInvariant();
            vm.UserTypeDetails.TypeName = user.UserType?.TypeName;

            if (typeName == "admin")
            {
                var admin = await _db.Admins.AsNoTracking().FirstOrDefaultAsync(a => a.UserID == userId);
                if (admin != null)
                {
                    vm.UserTypeDetails.AdminType = admin.AdminType;
                    vm.TypeSpecific.AdminType = admin.AdminType;
                }
            }
            else if (typeName == "craftsman")
            {
                var c = await _db.Craftsmen.AsNoTracking().FirstOrDefaultAsync(x => x.UserID == userId);
                if (c != null)
                {
                    vm.UserTypeDetails.SkillType = c.SkillType;
                    vm.UserTypeDetails.ExperienceYears = c.ExperienceYears;
                    vm.TypeSpecific.SkillType = c.SkillType;
                    vm.TypeSpecific.ExperienceYears = c.ExperienceYears;
                }
            }
            else if (typeName == "individual")
            {
                var i = await _db.Individuals.AsNoTracking().FirstOrDefaultAsync(x => x.UserID == userId);
                if (i != null)
                {
                    vm.UserTypeDetails.Occupation = i.Occupation;
                    vm.TypeSpecific.Occupation = i.Occupation;
                }
            }
            else if (typeName == "factory")
            {
                var f = await _db.Factories.AsNoTracking().FirstOrDefaultAsync(x => x.UserID == userId);
                if (f != null)
                {
                    vm.UserTypeDetails.FactoryName = f.FactoryName;
                    vm.UserTypeDetails.FactoryType = f.FactoryType;
                    vm.UserTypeDetails.Description = f.Description;

                    vm.TypeSpecific.FactoryName = f.FactoryName;
                    vm.TypeSpecific.FactoryType = f.FactoryType;
                    vm.TypeSpecific.Description = f.Description;

                    var imgs = new List<string>();
                    if (!string.IsNullOrWhiteSpace(f.FactoryImgURL1)) imgs.Add(f.FactoryImgURL1!);
                    if (!string.IsNullOrWhiteSpace(f.FactoryImgURL2)) imgs.Add(f.FactoryImgURL2!);
                    if (!string.IsNullOrWhiteSpace(f.FactoryImgURL3)) imgs.Add(f.FactoryImgURL3!);

                    vm.UserTypeDetails.FactoryImages = imgs;
                    vm.UserTypeDetails.CurrentFactoryImagesCount = imgs.Count;
                }
            }

            // Layout ViewBag already set in filter when session exists; but keep your old explicit set too
            ViewBag.UserLoggedIn = true;
            ViewBag.Email = email;
            ViewBag.type = user.UserType?.TypeName;

            return View(vm);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Setting GET failed");
            TempData["ErrorMessage"] = "Failed to load settings.";
            return RedirectToAction(nameof(Index));
        }
    }

    // Setting
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Setting(SettingModel vm)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdStr, out var userId))
            return RedirectToAction("Login", "Auth");

        var user = await _db.Users.Include(u => u.UserType).FirstOrDefaultAsync(u => u.UserID == userId);
        if (user == null)
            return RedirectToAction("Login", "Auth");

        if (user.Blocked)
        {
            TempData["ErrorMessage"] = "Your account is blocked. Contact support.";
            return RedirectToAction("Setting");
        }

        var errors = new List<string>();
        var saved = new List<string>();
        bool verificationShouldReset = false;

        // for safe rollback of uploaded images
        var cleanupNewUrls = new List<string>(); // if tx fails => delete these
        var strategy = _db.Database.CreateExecutionStrategy();

        try
        {
            await strategy.ExecuteAsync(async () =>
            {
                await using var tx = await _db.Database.BeginTransactionAsync();
                try
                {
                    var currentEmail = user.Email != null ? _dataProtection.Decrypt(user.Email) : null;
                    var currentPhone = user.phoneNumber != null ? _dataProtection.Decrypt(user.phoneNumber) : null;

                    // ---------- FullName ----------
                    if (!string.IsNullOrWhiteSpace(vm.BasicInfo?.FullName))
                    {
                        var newName = vm.BasicInfo.FullName.Trim();
                        if (!string.Equals(user.FullName, newName, StringComparison.Ordinal))
                        {
                            user.FullName = newName;
                            saved.Add("FullName");
                        }
                    }

                    // ---------- Location ----------
                    var loc = _locationService.ExtractAndValidateFromForm(
                        Request.Form["Latitude"].ToString(),
                        Request.Form["Longitude"].ToString(),
                        vm.BasicInfo?.Location
                    );

                    if (!loc.IsValid)
                        throw new InvalidOperationException(loc.Error);

                    var gpsOrMapProvided = loc.Latitude.HasValue && loc.Longitude.HasValue;
                    var userCleared = !gpsOrMapProvided && string.IsNullOrWhiteSpace(loc.Address);

                    if (userCleared)
                    {
                        user.Latitude = null;
                        user.Longitude = null;
                        user.Address = null;
                        saved.Add("LocationCleared");
                    }
                    else
                    {
                        if (gpsOrMapProvided)
                        {
                            user.Latitude = loc.Latitude;
                            user.Longitude = loc.Longitude;

                            if (string.IsNullOrWhiteSpace(loc.Address))
                            {
                                var rev = await _locationService.ReverseGeocodeAsync(loc.Latitude!.Value, loc.Longitude!.Value);
                                user.Address = rev;
                            }
                            else
                            {
                                user.Address = loc.Address;
                            }

                            saved.Add("Location");
                        }
                        else
                        {
                            if (!string.IsNullOrWhiteSpace(loc.Address))
                            {
                                var geo = await _locationService.GetLocationFromAddressAsync(loc.Address);
                                if (geo != null)
                                {
                                    user.Latitude = geo.Latitude;
                                    user.Longitude = geo.Longitude;
                                    user.Address = string.IsNullOrWhiteSpace(geo.NormalizedAddress) ? loc.Address : geo.NormalizedAddress;
                                }
                                else
                                {
                                    user.Latitude = null;
                                    user.Longitude = null;
                                    user.Address = loc.Address;
                                }

                                saved.Add("Location");
                            }
                        }
                    }

                    // ---------- Email ----------
                    if (!string.IsNullOrWhiteSpace(vm.BasicInfo?.Email))
                    {
                        var newEmail = vm.BasicInfo.Email.Trim().ToLowerInvariant();
                        if (!string.Equals(newEmail, currentEmail, StringComparison.OrdinalIgnoreCase))
                        {
                            if (!new EmailAddressAttribute().IsValid(newEmail))
                            {
                                errors.Add("Email format invalid (not saved).");
                            }
                            else
                            {
                                var newHash = _dataHasher.HashComparison(newEmail);
                                var exists = await _db.Users.AnyAsync(u => u.UserID != user.UserID && u.HashEmail == newHash);
                                if (exists)
                                {
                                    errors.Add("Email already exists (not saved).");
                                }
                                else
                                {
                                    user.Email = _dataProtection.Encrypt(newEmail);
                                    user.HashEmail = newHash;
                                    verificationShouldReset = true;
                                    saved.Add("Email");
                                }
                            }
                        }
                    }

                    // ---------- Phone ----------
                    if (!string.IsNullOrWhiteSpace(vm.BasicInfo?.PhoneNumber))
                    {
                        var newPhone = vm.BasicInfo.PhoneNumber.Trim();
                        if (!string.Equals(newPhone, currentPhone, StringComparison.Ordinal))
                        {
                            if (!new PhoneAttribute().IsValid(newPhone))
                            {
                                errors.Add("Phone format invalid (not saved).");
                            }
                            else
                            {
                                var newHash = _dataHasher.HashComparison(newPhone);
                                var exists = await _db.Users.AnyAsync(u => u.UserID != user.UserID && u.HashPhoneNumber == newHash);
                                if (exists)
                                {
                                    errors.Add("Phone already exists (not saved).");
                                }
                                else
                                {
                                    user.phoneNumber = _dataProtection.Encrypt(newPhone);
                                    user.HashPhoneNumber = newHash;
                                    verificationShouldReset = true;
                                    saved.Add("PhoneNumber");
                                }
                            }
                        }
                    }

                    // ---------- Profile Image ----------
                    if (vm.ProfileImage != null)
                    {
                        if (vm.ProfileImage.DeleteProfileImage)
                        {
                            var oldUrl = user.UserProfileImgURL;
                            user.UserProfileImgURL = null;
                            saved.Add("ProfileImageDeleted");

                            await _db.SaveChangesAsync();
                            await tx.CommitAsync();

                            await _images.DeleteAsync(oldUrl);
                            return;
                        }
                        else if (vm.ProfileImage.ProfileImageFile != null && vm.ProfileImage.ProfileImageFile.Length > 0)
                        {
                            var oldUrl = user.UserProfileImgURL;

                            var newUrl = await _images.ReplaceAsync(vm.ProfileImage.ProfileImageFile, "profile", oldUrl);
                            if (!string.IsNullOrWhiteSpace(newUrl) && !string.Equals(newUrl, oldUrl, StringComparison.Ordinal))
                            {
                                user.UserProfileImgURL = newUrl;
                                cleanupNewUrls.Add(newUrl);
                                saved.Add("ProfileImage");
                            }
                        }
                    }

                    // ---------- Type Specific by UserID ----------
                    var typeName = user.UserType?.TypeName?.Trim().ToLowerInvariant();

                    if (typeName == "admin")
                    {
                        var admin = await _db.Admins.FirstOrDefaultAsync(a => a.UserID == userId);

                        if (admin == null)
                        {
                            admin = new Admin
                            {
                                UserID = userId,
                                AdminType = vm.TypeSpecific?.AdminType?.Trim()
                            };
                            _db.Admins.Add(admin);
                            saved.Add("AdminType");
                        }
                        else
                        {
                            var newAdminType = vm.TypeSpecific?.AdminType?.Trim();
                            if (!string.Equals(admin.AdminType ?? "", newAdminType ?? "", StringComparison.Ordinal))
                            {
                                admin.AdminType = newAdminType;
                                saved.Add("AdminType");
                            }
                        }
                    }
                    else if (typeName == "craftsman")
                    {
                        var c = await _db.Craftsmen.FirstOrDefaultAsync(x => x.UserID == userId);

                        var newSkill = vm.TypeSpecific?.SkillType?.Trim();
                        var newYears = vm.TypeSpecific?.ExperienceYears;

                        if (c == null)
                        {
                            c = new Craftsman
                            {
                                UserID = userId,
                                SkillType = newSkill,
                                ExperienceYears = newYears!.Value
                            };
                            _db.Craftsmen.Add(c);
                            saved.Add("SkillType");
                            saved.Add("ExperienceYears");
                        }
                        else
                        {
                            if (!string.Equals(c.SkillType ?? "", newSkill ?? "", StringComparison.Ordinal))
                            {
                                c.SkillType = newSkill;
                                saved.Add("SkillType");
                            }

                            if (c.ExperienceYears != newYears)
                            {
                                c.ExperienceYears = newYears!.Value;
                                saved.Add("ExperienceYears");
                            }
                        }
                    }
                    else if (typeName == "individual")
                    {
                        var i = await _db.Individuals.FirstOrDefaultAsync(x => x.UserID == userId);
                        var newOcc = vm.TypeSpecific?.Occupation?.Trim();

                        if (i == null)
                        {
                            i = new Individual
                            {
                                UserID = userId,
                                Occupation = newOcc
                            };
                            _db.Individuals.Add(i);
                            saved.Add("Occupation");
                        }
                        else
                        {
                            if (!string.Equals(i.Occupation ?? "", newOcc ?? "", StringComparison.Ordinal))
                            {
                                i.Occupation = newOcc;
                                saved.Add("Occupation");
                            }
                        }
                    }
                    else if (typeName == "factory")
                    {
                        var factory = await _db.Factories.FirstOrDefaultAsync(f => f.UserID == userId);
                        if (factory != null)
                        {
                            if (!string.IsNullOrWhiteSpace(vm.TypeSpecific?.FactoryName))
                            {
                                factory.FactoryName = vm.TypeSpecific.FactoryName.Trim();
                                saved.Add("FactoryName");
                            }
                            if (!string.IsNullOrWhiteSpace(vm.TypeSpecific?.FactoryType))
                            {
                                factory.FactoryType = vm.TypeSpecific.FactoryType.Trim();
                                saved.Add("FactoryType");
                            }
                            if (!string.IsNullOrWhiteSpace(vm.TypeSpecific?.Description))
                            {
                                factory.Description = vm.TypeSpecific.Description.Trim();
                                saved.Add("FactoryDescription");
                            }

                            // slot1
                            if (vm.FactoryImages.DeleteSlot1)
                            {
                                var old = factory.FactoryImgURL1;
                                factory.FactoryImgURL1 = null;
                                saved.Add("FactoryImage1Deleted");

                                await _db.SaveChangesAsync();
                                await tx.CommitAsync();
                                await _images.DeleteAsync(old);
                                return;
                            }
                            else if (vm.FactoryImages.Slot1File != null && vm.FactoryImages.Slot1File.Length > 0)
                            {
                                var old = factory.FactoryImgURL1;
                                var newUrl = await _images.ReplaceAsync(vm.FactoryImages.Slot1File, "factory", old);
                                factory.FactoryImgURL1 = newUrl;
                                if (!string.IsNullOrWhiteSpace(newUrl)) cleanupNewUrls.Add(newUrl);
                                saved.Add("FactoryImage1");
                            }

                            // slot2
                            if (vm.FactoryImages.DeleteSlot2)
                            {
                                var old = factory.FactoryImgURL2;
                                factory.FactoryImgURL2 = null;
                                saved.Add("FactoryImage2Deleted");

                                await _db.SaveChangesAsync();
                                await tx.CommitAsync();
                                await _images.DeleteAsync(old);
                                return;
                            }
                            else if (vm.FactoryImages.Slot2File != null && vm.FactoryImages.Slot2File.Length > 0)
                            {
                                var old = factory.FactoryImgURL2;
                                var newUrl = await _images.ReplaceAsync(vm.FactoryImages.Slot2File, "factory", old);
                                factory.FactoryImgURL2 = newUrl;
                                if (!string.IsNullOrWhiteSpace(newUrl)) cleanupNewUrls.Add(newUrl);
                                saved.Add("FactoryImage2");
                            }

                            // slot3
                            if (vm.FactoryImages.DeleteSlot3)
                            {
                                var old = factory.FactoryImgURL3;
                                factory.FactoryImgURL3 = null;
                                saved.Add("FactoryImage3Deleted");

                                await _db.SaveChangesAsync();
                                await tx.CommitAsync();
                                await _images.DeleteAsync(old);
                                return;
                            }
                            else if (vm.FactoryImages.Slot3File != null && vm.FactoryImages.Slot3File.Length > 0)
                            {
                                var old = factory.FactoryImgURL3;
                                var newUrl = await _images.ReplaceAsync(vm.FactoryImages.Slot3File, "factory", old);
                                factory.FactoryImgURL3 = newUrl;
                                if (!string.IsNullOrWhiteSpace(newUrl)) cleanupNewUrls.Add(newUrl);
                                saved.Add("FactoryImage3");
                            }
                        }
                    }

                    // ---------- Verification Reset ----------
                    if (verificationShouldReset)
                    {
                        user.Verified = false;
                        user.OtpHash = "";
                        user.OtpExpiresAt = null;
                        user.OtpAttempts = 0;
                        user.OtpLastSentAt = null;
                        user.OtpVerifyBlockedUntil = null;
                        saved.Add("VerificationReset");
                    }

                    await _db.SaveChangesAsync();
                    await tx.CommitAsync();
                }
                catch
                {
                    await tx.RollbackAsync();
                    throw;
                }
            });

            cleanupNewUrls.Clear();

            if (saved.Count == 0 && errors.Count == 0)
                TempData["InfoMessage"] = "No changes detected.";
            else
            {
                if (saved.Count > 0)
                    TempData["SuccessMessage"] = $"Saved: {string.Join(", ", saved)}";

                if (errors.Count > 0)
                    TempData["WarningMessage"] = $"Some items were not saved: {string.Join(" | ", errors)}";
            }

            return RedirectToAction("Setting");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Setting POST failed");

            foreach (var url in cleanupNewUrls.Distinct())
            {
                try { await _images.DeleteAsync(url); } catch { }
            }

            TempData["ErrorMessage"] = "A system error occurred while updating settings.";
            return RedirectToAction("Setting");
        }
    }

    // ReverseGeocode
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> ReverseGeocode(decimal lat, decimal lng, CancellationToken ct)
    {
        try
        {
            var address = await _locationService.ReverseGeocodeAsync(lat, lng, ct);
            return Json(new { address });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ReverseGeocode failed");
            return Json(new { address = "" });
        }
    }

    // ChangePassword
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(SettingModel vm)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdStr, out var userId)) return RedirectToAction("Login", "Auth");

        if (string.IsNullOrWhiteSpace(vm.PasswordChange.CurrentPassword)
            || string.IsNullOrWhiteSpace(vm.PasswordChange.NewPassword)
            || string.IsNullOrWhiteSpace(vm.PasswordChange.ConfirmNewPassword))
        {
            TempData["PasswordError"] = "Invalid password input.";
            return RedirectToAction("Setting");
        }

        try
        {
            var user = await _db.Users.FindAsync(userId);
            if (user == null) return RedirectToAction("Login", "Auth");

            if (!_dataHasher.VerifyHashed(vm.PasswordChange.CurrentPassword!, user.HashPassword!))
            {
                TempData["PasswordError"] = "Current password is incorrect.";
                return RedirectToAction("Setting");
            }

            user.HashPassword = _dataHasher.HashData(vm.PasswordChange.NewPassword!);

            // changing password resets verification too
            user.Verified = false;
            user.OtpHash = "";
            user.OtpExpiresAt = null;
            user.OtpAttempts = 0;
            user.OtpLastSentAt = null;
            user.OtpVerifyBlockedUntil = null;

            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Password changed. Please verify your account again.";
            return RedirectToAction("Setting");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ChangePassword failed");
            TempData["PasswordError"] = "Failed to change password.";
            return RedirectToAction("Setting");
        }
    }

    // StartVerification
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> StartVerification()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdStr, out var userId)) return RedirectToAction("Login", "Auth");

        var user = await _db.Users.FindAsync(userId);
        if (user == null) return RedirectToAction("Login", "Auth");

        try
        {
            if (user.Blocked)
            {
                TempData["OtpError"] = "Your account is blocked.";
                return RedirectToAction("Setting");
            }

            if (user.Verified)
            {
                TempData["InfoMessage"] = "Your account is already verified.";
                return RedirectToAction("Setting");
            }

            // if never sent any OTP before => send one
            var neverSentBefore = string.IsNullOrWhiteSpace(user.OtpHash) && !user.OtpLastSentAt.HasValue;

            if (neverSentBefore)
            {
                var otp = await _otpService.ResendOtpAsync(user.UserID, sent: false);
                if (string.IsNullOrWhiteSpace(otp))
                {
                    TempData["OtpError"] = "Unable to send OTP now. Please try later.";
                    TempData["OpenVerifyModal"] = "true";
                    return RedirectToAction("Setting");
                }

                var email = user.Email != null ? _dataProtection.Decrypt(user.Email) : null;
                if (string.IsNullOrWhiteSpace(email))
                {
                    TempData["OtpError"] = "Email not found for your account.";
                    return RedirectToAction("Setting");
                }

                await _emailsTemplate.SendEmailAsync(
                    _emailsTemplate.CreateOtpVerificationEmail(email, otp, user.FullName)
                );

                TempData["InfoMessage"] = "We sent a verification code to your email. Please enter it.";
            }

            // If already sent before => do NOT send again, just open modal
            TempData["OpenVerifyModal"] = "true";
            return RedirectToAction("Setting");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "StartVerification failed");
            TempData["OtpError"] = "Failed to start verification.";
            TempData["OpenVerifyModal"] = "true";
            return RedirectToAction("Setting");
        }
    }

    // GetOtpStatus
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetOtpStatus()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdStr, out var userId)) return Unauthorized();

        var user = await _db.Users.FindAsync(userId);
        if (user == null) return Unauthorized();

        var now = DateTime.UtcNow;

        var hasActiveOtp = !string.IsNullOrWhiteSpace(user.OtpHash)
                           && user.OtpExpiresAt.HasValue
                           && user.OtpExpiresAt.Value > now;

        var remainingAttempts = Math.Max(0, _otpService.GetMaxOtpAttempts() - user.OtpAttempts);

        var expiryRemaining = TimeSpan.Zero;
        if (hasActiveOtp && user.OtpExpiresAt.HasValue)
        {
            expiryRemaining = user.OtpExpiresAt.Value - now;
            if (expiryRemaining < TimeSpan.Zero) expiryRemaining = TimeSpan.Zero;
        }

        var canResend = true;
        var resendRemaining = TimeSpan.Zero;

        if (user.OtpLastSentAt.HasValue)
        {
            var since = now - user.OtpLastSentAt.Value;
            var cooldown = TimeSpan.FromMinutes(_otpService.GetResendCooldownMinutes());
            if (since < cooldown)
            {
                canResend = false;
                resendRemaining = cooldown - since;
                if (resendRemaining < TimeSpan.Zero) resendRemaining = TimeSpan.Zero;
            }
        }

        var mailBlocked = user.MailBlockedUntil.HasValue && user.MailBlockedUntil.Value > now;
        if (mailBlocked) canResend = false;

        return Json(new
        {
            hasActiveOtp,
            remainingAttempts,
            expirySeconds = (int)Math.Max(0, expiryRemaining.TotalSeconds),
            canResendOtp = canResend && !user.Verified && !user.Blocked,
            resendCooldownSeconds = (int)Math.Max(0, resendRemaining.TotalSeconds),
            verified = user.Verified,
            blocked = user.Blocked,
            verifyBlockedUntil = user.OtpVerifyBlockedUntil
        });
    }

    // ResendOtp
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResendOtp()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdStr, out var userId)) return RedirectToAction("Login", "Auth");

        var user = await _db.Users.FindAsync(userId);
        if (user == null) return RedirectToAction("Login", "Auth");

        try
        {
            if (user.Blocked) { TempData["OtpError"] = "Your account is blocked."; return RedirectToAction("Setting"); }
            if (user.Verified) { TempData["InfoMessage"] = "Your account is already verified."; return RedirectToAction("Setting"); }

            var otp = await _otpService.ResendOtpAsync(user.UserID, sent: false);
            if (string.IsNullOrWhiteSpace(otp))
            {
                TempData["OtpError"] = "Please wait before requesting a new code.";
                TempData["OpenVerifyModal"] = "true";
                return RedirectToAction("Setting");
            }

            var email = user.Email != null ? _dataProtection.Decrypt(user.Email) : null;
            if (string.IsNullOrWhiteSpace(email))
            {
                TempData["OtpError"] = "Email not found for your account.";
                return RedirectToAction("Setting");
            }

            await _emailsTemplate.SendEmailAsync(
                _emailsTemplate.CreateOtpVerificationEmail(email, otp, user.FullName)
            );

            TempData["SuccessMessage"] = "Verification code sent to your email.";
            TempData["OpenVerifyModal"] = "true";
            return RedirectToAction("Setting");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ResendOtp failed");
            TempData["OtpError"] = "Failed to resend OTP.";
            TempData["OpenVerifyModal"] = "true";
            return RedirectToAction("Setting");
        }
    }

    // VerifyOtp
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> VerifyOtp(SettingModel vm)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdStr, out var userId)) return RedirectToAction("Login", "Auth");

        var user = await _db.Users.Include(u => u.UserType).FirstOrDefaultAsync(u => u.UserID == userId);
        if (user == null) return RedirectToAction("Login", "Auth");

        try
        {
            if (user.Blocked)
            {
                TempData["OtpError"] = "Your account is blocked.";
                return RedirectToAction("Setting");
            }

            if (user.Verified)
            {
                TempData["InfoMessage"] = "Your account is already verified.";
                return RedirectToAction("Setting");
            }

            var now = DateTime.UtcNow;
            var hasActiveOtp = !string.IsNullOrWhiteSpace(user.OtpHash)
                               && user.OtpExpiresAt.HasValue
                               && user.OtpExpiresAt.Value > now;

            if (!hasActiveOtp)
            {
                TempData["OtpError"] = "No active code. Please click Resend Code.";
                TempData["OpenVerifyModal"] = "true";
                return RedirectToAction("Setting");
            }

            if (string.IsNullOrWhiteSpace(vm.VerifyOtp?.OtpCode)
                || !System.Text.RegularExpressions.Regex.IsMatch(vm.VerifyOtp.OtpCode, @"^\d{6}$"))
            {
                TempData["OtpError"] = "Invalid OTP format.";
                TempData["OpenVerifyModal"] = "true";
                return RedirectToAction("Setting");
            }

            var ok = await _otpService.VerifyOtpAsync(user.UserID, vm.VerifyOtp.OtpCode.Trim());
            if (!ok)
            {
                var remaining = Math.Max(0, _otpService.GetMaxOtpAttempts() - user.OtpAttempts);
                TempData["OtpError"] = remaining <= 0
                    ? "Max attempts exceeded. Please resend a new code."
                    : $"Invalid code. {remaining} attempt(s) remaining.";
                TempData["OpenVerifyModal"] = "true";
                return RedirectToAction("Setting");
            }

            // refresh cookie claims and Update Session
            var decryptedEmail = user.Email != null ? _dataProtection.Decrypt(user.Email) : "";
            var decryptedPhone = user.phoneNumber != null ? _dataProtection.Decrypt(user.phoneNumber) : "";

            HttpContext.Session.SetString("UserEmail", decryptedEmail);
            HttpContext.Session.SetInt32("UserID", user.UserID);
            HttpContext.Session.SetString("UserName", user.FullName ?? "");
            HttpContext.Session.SetInt32("UserTypeID", user.UserTypeID);
            HttpContext.Session.SetString("PhoneNumber", decryptedPhone ?? "");

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, decryptedEmail),
                new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()),
                new Claim("IsVerified", "true"),
                new Claim(ClaimTypes.Role, user.UserType?.TypeName ?? ""),
                new Claim("UserType", user.UserType?.TypeName ?? "")
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity),
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30),
                    AllowRefresh = true
                }
            );

            TempData["SuccessMessage"] = "Account verified successfully!";
            return RedirectToAction("Setting");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "VerifyOtp failed");
            TempData["OtpError"] = "Verification failed.";
            TempData["OpenVerifyModal"] = "true";
            return RedirectToAction("Setting");
        }
    }

    // AuthTesting
    [Authorize]
    public IActionResult AuthTesting()
    {
        return Json("User authenticated Sucess");
    }

    // Error
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
