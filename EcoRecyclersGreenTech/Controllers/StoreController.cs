using EcoRecyclersGreenTech.Data.Users;
using EcoRecyclersGreenTech.Models;
using EcoRecyclersGreenTech.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EcoRecyclersGreenTech.Controllers
{
    [Authorize]
    public class StoreController : Controller
    {
        private readonly DBContext _db;
        private readonly IDataCiphers _dataProtection;
        private readonly IFactoryStoreService _storeService;
        private readonly ILogger<StoreController> _logger;

        // Cached per-request (avoid repeated DB/session/claims reads)
        private User? _currentUser;
        private int _currentUserId;
        private string _currentUserTypeName = "";
        private bool _isVerified;

        public StoreController(DBContext db, IDataCiphers dataProtection, ILogger<StoreController> logger, IFactoryStoreService storeService)
        {
            _db = db;
            _dataProtection = dataProtection;
            _logger = logger;
            _storeService = storeService;
        }

        // userId + type + verified
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            try
            {
                var (ok, userId, user) = await ApplyUserStateAsync();
                if (!ok || user == null)
                {
                    context.Result = RedirectToAction("Login", "Auth");
                    return;
                }

                _currentUserId = userId;
                _currentUser = user;

                // Type name cached
                _currentUserTypeName = user.UserType?.TypeName ?? "";

                // Verified cached (claims OR session OR db)
                _isVerified =
                    (User.HasClaim(c => c.Type == "IsVerified" && c.Value == "true"))
                    || (string.Equals(HttpContext.Session.GetString("IsVerified"), "true", StringComparison.OrdinalIgnoreCase))
                    || user.Verified;

                // Layout essentials (if your _Layout needs them)
                ViewBag.UserLoggedIn = true;
                ViewBag.IsVerified = user.Verified;
                ViewBag.NeedsVerification = !user.Verified;
                ViewBag.type = _currentUserTypeName;

                // Keep email behavior safe (optional)
                var sessionEmail = HttpContext.Session.GetString("UserEmail");
                if (!string.IsNullOrWhiteSpace(sessionEmail))
                    ViewBag.Email = sessionEmail;

                // Blocked safety (since controller is authorized area)
                if (user.Blocked)
                {
                    TempData["ErrorMessage"] = "Your account is blocked. Contact support.";
                    context.Result = RedirectToAction("Index", "Home");
                    return;
                }

                await next();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "StoreController OnActionExecutionAsync failed.");
                TempData["ErrorMessage"] = "Something went wrong. Please try again.";
                context.Result = RedirectToAction("Index", "Home");
            }
        }

        // Single place to resolve the current user (claims/session + DB fallback)
        private async Task<(bool ok, int userId, User? user)> ApplyUserStateAsync()
        {
            // Claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int userId = 0;

            if (!string.IsNullOrWhiteSpace(userIdClaim) && int.TryParse(userIdClaim, out var idFromClaims))
                userId = idFromClaims;

            // Session fallback
            if (userId <= 0)
                userId = HttpContext.Session.GetInt32("UserID") ?? 0;

            if (userId <= 0)
                return (false, 0, null);

            // Load user + type once
            var user = await _db.Users
                .AsNoTracking()
                .Include(u => u.UserType)
                .FirstOrDefaultAsync(u => u.UserID == userId);

            if (user == null)
                return (false, 0, null);

            // cache type name in session
            if (!string.IsNullOrWhiteSpace(user.UserType?.TypeName))
                HttpContext.Session.SetString("UserTypeName", user.UserType.TypeName);

            // cache verified in session
            HttpContext.Session.SetString("IsVerified", user.Verified ? "true" : "false");

            return (true, user.UserID, user);
        }

        // Index
        [HttpGet]
        public IActionResult Index()
        {
            try
            {
                // Individual verified -> Individual/Index
                if (string.Equals(_currentUserTypeName, "Individual", StringComparison.OrdinalIgnoreCase) && _isVerified)
                {
                    return RedirectToAction("Index", "Individual");
                }

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Store/Index failed.");
                TempData["ErrorMessage"] = "Failed to redirect. Please try again.";
                return RedirectToAction("Index", "Home");
            }
        }
    }
}
