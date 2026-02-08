using EcoRecyclersGreenTech.Data.Users;
using EcoRecyclersGreenTech.Models;
using EcoRecyclersGreenTech.Models.Store;
using EcoRecyclersGreenTech.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using static EcoRecyclersGreenTech.Data.Stores.EnumsProductStatus;

namespace EcoRecyclersGreenTech.Controllers
{
    [Authorize]
    public class StoreController : Controller
    {
        private readonly DBContext _db;
        private readonly IDataCiphers _dataProtection;
        private readonly IFactoryStoreService _storeService;
        private readonly ILogger<StoreController> _logger;

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

                // Verified cached
                _isVerified =
                    (User.HasClaim(c => c.Type == "IsVerified" && c.Value == "true"))
                    || (string.Equals(HttpContext.Session.GetString("IsVerified"), "true", StringComparison.OrdinalIgnoreCase))
                    || user.Verified;

                // Layout essentials
                ViewBag.UserLoggedIn = true;
                ViewBag.IsVerified = user.Verified;
                ViewBag.NeedsVerification = !user.Verified;
                ViewBag.type = _currentUserTypeName;

                // Wallet summary
                var wallet = await _db.Wallets
                    .AsNoTracking()
                    .Where(w => w.UserId == user.UserId)
                    .Select(w => new { w.Balance, w.ReservedBalance })
                    .FirstOrDefaultAsync();

                ViewBag.WalletBalance = wallet?.Balance ?? 0m;
                ViewBag.WalletReserved = wallet?.ReservedBalance ?? 0m;
                ViewBag.WalletCurrency = "EGP";

                // Keep email behavior safe
                var sessionEmail = HttpContext.Session.GetString("UserEmail");
                if (!string.IsNullOrWhiteSpace(sessionEmail))
                    ViewBag.Email = sessionEmail;

                // Blocked safety
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
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
                return (false, 0, null);

            // cache type name in session
            if (!string.IsNullOrWhiteSpace(user.UserType?.TypeName))
                HttpContext.Session.SetString("UserTypeName", user.UserType.TypeName);

            // cache verified in session
            HttpContext.Session.SetString("IsVerified", user.Verified ? "true" : "false");

            return (true, user.UserId, user);
        }

        // Index
        [HttpGet]
        public async Task<IActionResult> Index(string? q, string? type)
        {
            q = (q ?? "").Trim();
            type = (type ?? "").Trim();

            // ✅ Materials
            var materialsQ = _db.MaterialStores
                .AsNoTracking()
                .Include(x => x.Seller)
                .Where(x => x.Status == ProductStatus.Available && x.Seller != null && x.Seller.Verified);

            if (!string.IsNullOrWhiteSpace(q))
            {
                materialsQ = materialsQ.Where(x =>
                    (x.ProductType != null && x.ProductType.Contains(q)) ||
                    (x.Description != null && x.Description.Contains(q)) ||
                    (x.Seller.FullName != null && x.Seller.FullName.Contains(q))
                );
            }

            // ✅ Machines
            var machinesQ = _db.MachineStores
                .AsNoTracking()
                .Include(x => x.Seller)
                .Where(x => x.Status == ProductStatus.Available && x.Seller != null && x.Seller.Verified);

            if (!string.IsNullOrWhiteSpace(q))
            {
                machinesQ = machinesQ.Where(x =>
                    (x.MachineType != null && x.MachineType.Contains(q)) ||
                    (x.Description != null && x.Description.Contains(q)) ||
                    (x.Brand != null && x.Brand.Contains(q)) ||
                    (x.Model != null && x.Model.Contains(q)) ||
                    (x.Seller.FullName != null && x.Seller.FullName.Contains(q))
                );
            }

            // ✅ Rentals
            var rentalsQ = _db.RentalStores
                .AsNoTracking()
                .Include(x => x.Owner)
                .Where(x => x.Status == ProductStatus.Available && x.Owner != null && x.Owner.Verified);

            if (!string.IsNullOrWhiteSpace(q))
            {
                rentalsQ = rentalsQ.Where(x =>
                    (x.Description != null && x.Description.Contains(q)) ||
                    (x.Address != null && x.Address.Contains(q)) ||
                    (x.Owner.FullName != null && x.Owner.FullName.Contains(q))
                );
            }

            // ✅ Auctions
            var auctionsQ = _db.AuctionStores
                .AsNoTracking()
                .Include(x => x.Seller)
                .Where(x => x.Status == ProductStatus.Available && x.Seller != null && x.Seller.Verified);

            if (!string.IsNullOrWhiteSpace(q))
            {
                auctionsQ = auctionsQ.Where(x =>
                    (x.ProductType != null && x.ProductType.Contains(q)) ||
                    (x.Description != null && x.Description.Contains(q)) ||
                    (x.Address != null && x.Address.Contains(q)) ||
                    (x.Seller.FullName != null && x.Seller.FullName.Contains(q))
                );
            }

            // ✅ Jobs
            var today = DateTime.UtcNow.Date;
            var jobsQ = _db.JobStores
                .AsNoTracking()
                .Include(x => x.User)
                .Where(x =>
                    x.Status == ProductStatus.Available &&
                    x.User != null && x.User.Verified &&
                    (!x.ExpiryDate.HasValue || x.ExpiryDate.Value.Date > today)
                );

            if (!string.IsNullOrWhiteSpace(q))
            {
                jobsQ = jobsQ.Where(x =>
                    (x.JobType != null && x.JobType.Contains(q)) ||
                    (x.Description != null && x.Description.Contains(q)) ||
                    (x.Location != null && x.Location.Contains(q)) ||
                    (x.RequiredSkills != null && x.RequiredSkills.Contains(q)) ||
                    (x.User.FullName != null && x.User.FullName.Contains(q))
                );
            }

            // ✅ Build results حسب type
            var list = new List<StoreCardModel>();

            bool allTypes = string.IsNullOrWhiteSpace(type);

            if (allTypes || type.Equals("Material", StringComparison.OrdinalIgnoreCase))
            {
                var rows = await materialsQ
                    .Select(x => new StoreCardModel
                    {
                        Id = x.MaterialID,
                        Type = "Material",
                        Name = x.ProductType ?? $"Material #{x.MaterialID}",
                        Description = x.Description,
                        Price = x.Price,
                        Currency = "EGP",
                        SellerName = x.Seller.FullName ?? "Seller",
                        Location = x.Address,
                        CreatedAt = x.CreatedAt,

                        DetailController = "CraftsMan",
                        DetailAction = "MaterialDetails"
                    })
                    .ToListAsync();

                list.AddRange(rows);
            }

            if (allTypes || type.Equals("Machine", StringComparison.OrdinalIgnoreCase))
            {
                var rows = await machinesQ
                    .Select(x => new StoreCardModel
                    {
                        Id = x.MachineID,
                        Type = "Machine",
                        Name = x.MachineType ?? $"Machine #{x.MachineID}",
                        Description = x.Description,
                        Price = x.Price,
                        Currency = "EGP",
                        SellerName = x.Seller.FullName ?? "Seller",
                        Location = x.Address,
                        CreatedAt = x.CreatedAt,

                        DetailController = "CraftsMan",
                        DetailAction = "MachineDetails"
                    })
                    .ToListAsync();

                list.AddRange(rows);
            }

            if (allTypes || type.Equals("Rental", StringComparison.OrdinalIgnoreCase))
            {
                var rows = await rentalsQ
                    .Select(x => new StoreCardModel
                    {
                        Id = x.RentalID,
                        Type = "Rental",
                        Name = $"Rental #{x.RentalID}",
                        Description = x.Description,
                        Price = x.PricePerMonth,
                        Currency = "EGP",
                        SellerName = x.Owner.FullName ?? "Owner",
                        Location = x.Address,
                        CreatedAt = x.AvailableFrom,

                        DetailController = "CraftsMan",
                        DetailAction = "RentalDetails"
                    })
                    .ToListAsync();

                list.AddRange(rows);
            }

            if (allTypes || type.Equals("Auction", StringComparison.OrdinalIgnoreCase))
            {
                var rows = await auctionsQ
                    .Select(x => new StoreCardModel
                    {
                        Id = x.AuctionID,
                        Type = "Auction",
                        Name = x.ProductType ?? $"Auction #{x.AuctionID}",
                        Description = x.Description,
                        Price = (x.CurrentTopBid ?? x.StartPrice),
                        Currency = "EGP",
                        SellerName = x.Seller.FullName ?? "Seller",
                        Location = x.Address,
                        CreatedAt = x.StartDate,

                        DetailController = "CraftsMan",
                        DetailAction = "AuctionDetails"
                    })
                    .ToListAsync();

                list.AddRange(rows);
            }

            if (allTypes || type.Equals("Job", StringComparison.OrdinalIgnoreCase))
            {
                var rows = await jobsQ
                    .Select(x => new StoreCardModel
                    {
                        Id = x.JobID,
                        Type = "Job",
                        Name = x.JobType ?? $"Job #{x.JobID}",
                        Description = x.Description,
                        Price = x.Salary,
                        Currency = "EGP",
                        SellerName = x.User.FullName ?? "Poster",
                        Location = x.Location,
                        CreatedAt = x.CreatedAt,

                        // غالبًا عندك صفحة تفاصيل للوظيفة في Individual/JobDetails أو CraftsMan/JobDetails
                        // لو عندك CraftsMan JobDetails غيّرها هنا
                        DetailController = "Individual",
                        DetailAction = "JobDetails"
                    })
                    .ToListAsync();

                list.AddRange(rows);
            }

            // ✅ ترتيب + تقليل النتائج لو تحب
            list = list
                .OrderByDescending(x => x.CreatedAt)
                .Take(600)
                .ToList();

            var model = new StoreIndexModel
            {
                Query = string.IsNullOrWhiteSpace(q) ? null : q,
                Type = string.IsNullOrWhiteSpace(type) ? null : type,
                Stores = list
            };

            return View(model);
        }
    }
}
