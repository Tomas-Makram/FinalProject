using EcoRecyclersGreenTech.Data.Orders;
using EcoRecyclersGreenTech.Data.Users;
using EcoRecyclersGreenTech.Models;
using EcoRecyclersGreenTech.Models.CraftsMan;
using EcoRecyclersGreenTech.Models.FactoryStore;
using EcoRecyclersGreenTech.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using StripeCheckoutSession = Stripe.Checkout.Session;
using StripeCheckoutSessionService = Stripe.Checkout.SessionService;
using System.Globalization;
using System.Security.Claims;
using static EcoRecyclersGreenTech.Data.Stores.EnumsProductStatus;

namespace EcoRecyclersGreenTech.Controllers
{
    [Authorize]
    public class CraftsManController : Controller
    {
        private readonly DBContext _db;
        private readonly IFactoryStoreService _factoryService;
        private readonly IDataCiphers _dataCiphers;
        private readonly IFilterAIService _filterAIService;
        private readonly StripePaymentService _stripePaymentService;
        private readonly ILogger<CraftsManController> _logger;
        private readonly IOptions<PricingOptions> _pricingOptions;

        private User? _currentUser;
        private int _currentUserId;
        private bool _isLoggedIn;

        // show verify alert only for these pages
        private static readonly HashSet<string> _showVerifyAlertActions = new(StringComparer.OrdinalIgnoreCase)
        {
            "Index",
            "PublicMaterialStore","PublicMachineStore","PublicRentalStore","PublicAuctionStore","PublicJobStore",
            "MyMaterialOrders","MyMachineOrders","MyRentalOrders","MyAuctionOrders","MyJobOrders",
        };

        public CraftsManController(DBContext db, IFactoryStoreService factoryService, IDataCiphers dataCiphers, ILogger<CraftsManController> logger, IFilterAIService filterAIService, StripePaymentService stripePaymentService, IOptions<PricingOptions> pricingOptions)
        {
            _db = db;
            _factoryService = factoryService;
            _dataCiphers = dataCiphers;
            _logger = logger;
            _filterAIService = filterAIService;
            _stripePaymentService = stripePaymentService;
            _pricingOptions = pricingOptions;
        }

        // load user + common ViewBags once
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            try
            {
                var actionName = context.ActionDescriptor.RouteValues.TryGetValue("action", out var a) ? a : "";

                var state = await ApplyLoginStateAsync(
                    showVerifyAlert: _showVerifyAlertActions.Contains(actionName ?? "")
                );

                _isLoggedIn = state.ok;
                _currentUserId = state.userId;
                _currentUser = state.user;

                if (!_isLoggedIn || _currentUser == null || _currentUserId == 0)
                {
                    context.Result = RedirectToAction("Login", "Auth");
                    return;
                }

                if (_currentUser.Blocked)
                {
                    TempData["Error"] = "Your account is blocked. Contact support.";
                    context.Result = RedirectToAction("Index", "Home");
                    return;
                }

                // Unverified should NOT redirect to Login
                // deny access, but show message
                if (!_currentUser.Verified)
                {
                    TempData["Warning"] = "Your account needs verification to continue. Please go to settings to verify.";
                    context.Result = RedirectToAction("Index", "Home");
                    return;
                }

                // common email bag
                ViewBag.Email = DecryptOrRaw(_currentUser.Email) ?? "";
                await next();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CraftsManController OnActionExecutionAsync failed.");
                TempData["Error"] = "Something went wrong. Please try again.";
                context.Result = RedirectToAction("Index", "Home");
            }
        }

        private async Task<(bool ok, int userId, User? user)> ApplyLoginStateAsync(bool showVerifyAlert = false)
        {
            var userId = GetCurrentUserId();
            if (userId <= 0)
            {
                ViewBag.UserLoggedIn = false;
                return (false, 0, null);
            }

            ViewBag.UserLoggedIn = true;

            var user = await _db.Users
                .AsNoTracking()
                .Include(u => u.UserType)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
            {
                ViewBag.UserLoggedIn = false;
                return (false, 0, null);
            }

            // wallet summary
            var wallet = await _db.Wallets
                .AsNoTracking()
                .Where(w => w.UserId == user.UserId)
                .Select(w => new { w.Balance, w.ReservedBalance })
                .FirstOrDefaultAsync();

            ViewBag.WalletBalance = wallet?.Balance ?? 0m;
            ViewBag.WalletReserved = wallet?.ReservedBalance ?? 0m;
            ViewBag.WalletCurrency = "EGP";

            // same bag names you already use
            ViewBag.type = user.UserType?.TypeName;
            ViewBag.IsVerified = user.Verified;
            ViewBag.NeedsVerification = !user.Verified;

            ViewBag.UserType = "CraftsMan";
            ViewBag.userName = user.FullName ?? "";

            if (showVerifyAlert && !user.Verified)
            {
                TempData["VerificationMessage"] = $"Welcome {user.FullName}! Please go to settings to verification your account.";
                TempData["ShowVerificationAlert"] = "true";
            }

            return (true, user.UserId, user);
        }

        private int GetCurrentUserId()
        {
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(claim, out var id) && id > 0) return id;

            return HttpContext.Session.GetInt32("UserID") ?? 0;
        }

        private string? DecryptOrRaw(string? v)
        {
            if (string.IsNullOrWhiteSpace(v)) return null;
            try { return _dataCiphers.Decrypt(v); }
            catch { return v; }
        }

        private (decimal available, decimal balance, decimal reserved) WalletAvailableFromDb(decimal balance, decimal reserved)
        {
            var available = Math.Round(balance - reserved, 2, MidpointRounding.AwayFromZero);
            if (available < 0m) available = 0m;
            return (available, balance, reserved);
        }

        private static double HaversineKm(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371.0;
            static double ToRad(double x) => x * Math.PI / 180.0;

            var dLat = ToRad(lat2 - lat1);
            var dLon = ToRad(lon2 - lon1);

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRad(lat1)) * Math.Cos(ToRad(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        private static decimal Round2(decimal x) => Math.Round(x, 2, MidpointRounding.AwayFromZero);

        // Dashboard
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                int userId = _currentUserId;
                var today = DateTime.UtcNow.Date;

                //  Public Stores
                int publicMaterials = await _db.MaterialStores
                    .AsNoTracking().Include(x => x.Seller)
                    .CountAsync(x => x.Status == ProductStatus.Available && x.Seller != null && x.Seller.Verified);

                int publicMachines = await _db.MachineStores
                    .AsNoTracking().Include(x => x.Seller)
                    .CountAsync(x => x.Status == ProductStatus.Available && x.Seller != null && x.Seller.Verified);

                int publicRentals = await _db.RentalStores
                    .AsNoTracking().Include(x => x.Owner)
                    .CountAsync(x => x.Status == ProductStatus.Available && x.Owner != null && x.Owner.Verified);

                int publicAuctions = await _db.AuctionStores
                    .AsNoTracking().Include(x => x.Seller)
                    .CountAsync(x => x.Status == ProductStatus.Available && x.Seller != null && x.Seller.Verified);

                // My Orders Counts 
                int myMaterialOrders = await _db.MaterialOrders.AsNoTracking()
                    .CountAsync(o => o.BuyerID == userId &&
                        (o.Status == EnumsOrderStatus.Pending || o.Status == EnumsOrderStatus.Processing));

                int myMachineOrders = await _db.MachineOrders.AsNoTracking()
                    .CountAsync(o => o.BuyerID == userId &&
                        (o.Status == EnumsOrderStatus.Pending || o.Status == EnumsOrderStatus.Processing));

                int myRentalOrders = await _db.RentalOrders.AsNoTracking()
                    .CountAsync(o => o.BuyerID == userId &&
                        o.Status != EnumsOrderStatus.Delivered &&
                        o.Status != EnumsOrderStatus.Completed &&
                        o.Status != EnumsOrderStatus.PickedUp);

                int myAuctionOrders = await _db.AuctionOrders.AsNoTracking()
                    .CountAsync(o => o.WinnerID == userId &&
                        o.Status != EnumsOrderStatus.Delivered &&
                        o.Status != EnumsOrderStatus.Completed &&
                        o.Status != EnumsOrderStatus.PickedUp);

                // Recent Pending
                var materialRecent = await _db.MaterialOrders.AsNoTracking()
                    .Where(o => o.BuyerID == userId &&
                        (o.Status == EnumsOrderStatus.Pending || o.Status == EnumsOrderStatus.Processing))
                    .OrderByDescending(o => o.OrderDate)
                    .Select(o => new RecentPendingOrderModel
                    {
                        Id = o.MaterialOrderID,
                        Type = "Material",
                        OrderNumber = $"MAT-{o.MaterialOrderID}",
                        SellerOrOwner = o.MaterialStore.Seller.FullName ?? "Seller",
                        Amount = o.TotalPrice,
                        Status = o.Status.ToString(),
                        OrderDate = o.OrderDate,
                        DetailsController = "CraftsMan",
                        DetailsAction = "MaterialOrderDetails"
                    })
                    .Take(20)
                    .ToListAsync();

                var machineRecent = await _db.MachineOrders.AsNoTracking()
                    .Where(o => o.BuyerID == userId &&
                        (o.Status == EnumsOrderStatus.Pending || o.Status == EnumsOrderStatus.Processing))
                    .OrderByDescending(o => o.OrderDate)
                    .Select(o => new RecentPendingOrderModel
                    {
                        Id = o.MachineOrderID,
                        Type = "Machine",
                        OrderNumber = $"MAC-{o.MachineOrderID}",
                        SellerOrOwner = o.MachineStore.Seller.FullName ?? "Seller",
                        Amount = o.TotalPrice,
                        Status = o.Status.ToString(),
                        OrderDate = o.OrderDate,
                        DetailsController = "CraftsMan",
                        DetailsAction = "MachineOrderDetails"
                    })
                    .Take(20)
                    .ToListAsync();

                var rentalRecent = await _db.RentalOrders.AsNoTracking()
                    .Where(o => o.BuyerID == userId &&
                        o.Status != EnumsOrderStatus.Delivered &&
                        o.Status != EnumsOrderStatus.Completed &&
                        o.Status != EnumsOrderStatus.PickedUp)
                    .OrderByDescending(o => o.OrderDate)
                    .Select(o => new RecentPendingOrderModel
                    {
                        Id = o.RentalOrderID,
                        Type = "Rental",
                        OrderNumber = $"REN-{o.RentalOrderID}",
                        SellerOrOwner = o.RentalStore.Owner.FullName ?? "Owner",
                        Amount = o.AmountPaid,
                        Status = o.Status.ToString(),
                        OrderDate = o.OrderDate,
                        DetailsController = "CraftsMan",
                        DetailsAction = "RentalOrderDetails"
                    })
                    .Take(20)
                    .ToListAsync();

                var auctionRecent = await _db.AuctionOrders.AsNoTracking()
                    .Where(o => o.WinnerID == userId &&
                        o.Status != EnumsOrderStatus.Delivered &&
                        o.Status != EnumsOrderStatus.Completed &&
                        o.Status != EnumsOrderStatus.PickedUp)
                    .OrderByDescending(o => o.OrderDate)
                    .Select(o => new RecentPendingOrderModel
                    {
                        Id = o.AuctionOrderID,
                        Type = "Auction",
                        OrderNumber = $"AUC-{o.AuctionOrderID}",
                        SellerOrOwner = o.AuctionStore.Seller.FullName ?? "Seller",
                        Amount = o.AmountPaid,
                        Status = o.Status.ToString(),
                        OrderDate = o.OrderDate,
                        DetailsController = "CraftsMan",
                        DetailsAction = "AuctionOrderDetails"
                    })
                    .Take(20)
                    .ToListAsync();

                var recentUnified = materialRecent
                    .Concat(machineRecent)
                    .Concat(rentalRecent)
                    .Concat(auctionRecent)
                    .OrderByDescending(x => x.OrderDate)
                    .Take(15)
                    .ToList();

                var model = new CraftsManDashboardModel
                {
                    UserName = _currentUser?.FullName ?? "User",
                    IsVerified = _currentUser?.Verified ?? false,

                    PublicMaterials = publicMaterials,
                    PublicMachines = publicMachines,
                    PublicRentals = publicRentals,
                    PublicAuctions = publicAuctions,

                    MyPendingMaterialOrders = myMaterialOrders,
                    MyPendingMachineOrders = myMachineOrders,
                    MyPendingRentalOrders = myRentalOrders,
                    MyPendingAuctionOrders = myAuctionOrders,

                    RecentPending = recentUnified
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CraftsMan Index dashboard failed");
                return View(new CraftsManDashboardModel());
            }
        }

        // Public Store (Material/Machine/Rental/Auction)
        private async Task<IActionResult> PublicStoreCoreAsync(string type, CraftsManStoreFilterModel filter)
        {
            try
            {
                // craftsman profile for AI
                string? mySkill = null;
                int myYears = 0;

                var cm = await _db.Craftsmen.AsNoTracking()
                    .Where(x => x.UserID == _currentUserId)
                    .Select(x => new { x.SkillType, x.ExperienceYears })
                    .FirstOrDefaultAsync();

                if (cm != null)
                {
                    mySkill = cm.SkillType;
                    myYears = cm.ExperienceYears;
                }

                var s = new SearchFilterModel
                {
                    Keyword = filter.Q,
                    MinPrice = filter.MinPrice,
                    MaxPrice = filter.MaxPrice,
                    ExperienceLevel = filter.ExperienceLevel,
                    Location = filter.Location,
                    SortBy = filter.SortBy ?? "newest",
                    SortDir = filter.SortDir ?? "desc",
                    UserLat = _currentUser?.Latitude,
                    UserLng = _currentUser?.Longitude,
                    MaxDistanceKm = filter.MaxKm
                };

                List<FactoryStoreModel> list = type switch
                {
                    "Material" => await _factoryService.GetPublicMaterialsAsync(s),
                    "Machine" => await _factoryService.GetPublicMachinesAsync(s),
                    "Rental" => await _factoryService.GetPublicRentalsAsync(s),
                    "Auction" => await _factoryService.GetPublicAuctionsAsync(s),
                    _ => await _factoryService.GetPublicMaterialsAsync(s)
                };

                list = list.Where(x => string.Equals(x.Type, type, StringComparison.OrdinalIgnoreCase)).ToList();

                // distance Calculate
                var distanceMap = new Dictionary<int, double>();
                if (_currentUser?.Latitude is not null && _currentUser.Longitude is not null)
                {
                    var uLat = (double)_currentUser.Latitude.Value;
                    var uLng = (double)_currentUser.Longitude.Value;

                    foreach (var it in list)
                    {
                        double? lat = null;
                        double? lng = null;

                        if (it.Type == "Rental" && it.RentalLatitude.HasValue && it.RentalLongitude.HasValue)
                        {
                            lat = (double)it.RentalLatitude.Value;
                            lng = (double)it.RentalLongitude.Value;
                        }

                        if (lat.HasValue && lng.HasValue)
                            distanceMap[it.Id] = HaversineKm(uLat, uLng, lat.Value, lng.Value);
                    }

                    if (filter.MaxKm.HasValue)
                    {
                        var max = filter.MaxKm.Value;
                        list = list.Where(x => distanceMap.TryGetValue(x.Id, out var km) && km <= max).ToList();
                    }

                    if (string.Equals(filter.SortBy, "distance", StringComparison.OrdinalIgnoreCase))
                    {
                        bool asc = string.Equals(filter.SortDir, "asc", StringComparison.OrdinalIgnoreCase);
                        list = (asc
                                ? list.OrderBy(x => distanceMap.ContainsKey(x.Id) ? distanceMap[x.Id] : double.MaxValue)
                                : list.OrderByDescending(x => distanceMap.ContainsKey(x.Id) ? distanceMap[x.Id] : -1)
                            )
                            .ThenByDescending(x => x.CreatedAt)
                            .ToList();
                    }
                }

                // AI skill filter
                var matchMap = new Dictionary<int, double>();
                if (filter.UseSmartSkillFilter)
                {
                    var skill = (mySkill ?? "").Trim();
                    if (string.IsNullOrWhiteSpace(skill))
                    {
                        TempData["Warning"] = "AI enabled but no SkillType saved for your craftsman profile.";
                    }
                    else
                    {
                        var tokens = _filterAIService.Tokenize(skill).Take(6).ToList();

                        if (tokens.Count > 0)
                        {
                            list = list.Where(x =>
                                _filterAIService.ContainsAnyToken(x.Name, tokens) ||
                                _filterAIService.ContainsAnyToken(x.Type, tokens) ||
                                _filterAIService.ContainsAnyToken(x.JobType, tokens) ||
                                _filterAIService.ContainsAnyToken(x.Description, tokens) ||
                                _filterAIService.ContainsAnyToken(x.RequiredSkills, tokens)
                            ).ToList();
                        }

                        foreach (var x in list)
                        {
                            var combined = _filterAIService.Combine(
                                x.Name, x.Type, x.JobType,
                                x.Description, x.RequiredSkills, x.ExperienceLevel
                            );

                            matchMap[x.Id] = _filterAIService.Similarity(skill, combined);
                        }

                        var th = Math.Max(0.10, filter.SmartThreshold);

                        list = list
                            .Where(x => matchMap.TryGetValue(x.Id, out var sc) && sc >= th)
                            .OrderByDescending(x => matchMap[x.Id])
                            .ThenByDescending(x => x.CreatedAt)
                            .ToList();
                    }
                }

                ViewBag.Filter = filter;
                ViewBag.UserAddress = _currentUser?.Address;
                ViewBag.MySkillType = mySkill;
                ViewBag.MyExperienceYears = myYears;
                ViewBag.MatchMap = matchMap;
                ViewBag.DistanceMap = distanceMap;

                ViewBag.Title = type switch
                {
                    "Material" => "Materials",
                    "Machine" => "Machines",
                    "Rental" => "Rentals",
                    "Auction" => "Auctions",
                    _ => "Store"
                };

                return View("PublicStoreList", list);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PublicStoreCoreAsync failed for type={Type}", type);
                TempData["Error"] = "Failed to load store list. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        // Public Stores (Routes)
        [HttpGet] public Task<IActionResult> PublicMaterialStore(CraftsManStoreFilterModel filter) => PublicStoreCoreAsync("Material", filter);
        [HttpGet] public Task<IActionResult> PublicMachineStore(CraftsManStoreFilterModel filter) => PublicStoreCoreAsync("Machine", filter);
        [HttpGet] public Task<IActionResult> PublicRentalStore(CraftsManStoreFilterModel filter) => PublicStoreCoreAsync("Rental", filter);
        [HttpGet] public Task<IActionResult> PublicAuctionStore(CraftsManStoreFilterModel filter) => PublicStoreCoreAsync("Auction", filter);

        // Load seller contact into ViewBag
        private async Task LoadSellerContactAsync(int sellerUserId)
        {
            var seller = await _db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.UserId == sellerUserId);
            if (seller == null) return;

            ViewBag.SellerEmail = DecryptOrRaw(seller.Email) ?? seller.Email ?? "";
            ViewBag.SellerPhone = DecryptOrRaw(seller.phoneNumber) ?? seller.phoneNumber ?? "";
        }

        /////////////////////////// Material Management ///////////////////////////
        [HttpGet]
        public async Task<IActionResult> MaterialDetails(int id)
        {
            var item = await _factoryService.GetPublicMaterialDetailsAsync(id);
            if (item == null)
            {
                TempData["Error"] = "This material is no longer available.";
                return RedirectToAction(nameof(PublicMaterialStore));
            }

            await LoadSellerContactAsync(item.SellerUserId);

            var pricing = _pricingOptions.Value;
            ViewBag.AuctionDepositPercent = pricing.AuctionDepositPercent;

            return View("PublicDetails", item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateMaterialOrder(int id, int quantity, string payMode, decimal walletAmount, decimal payAmount)
        {
            if (quantity <= 0)
            {
                TempData["Error"] = "Invalid quantity.";
                return RedirectToAction(nameof(MaterialDetails), new { id });
            }

            var material = await _db.MaterialStores.AsNoTracking()
                .Where(m => m.MaterialID == id)
                .Select(m => new { m.MaterialID, m.Price, m.Quantity, m.Status })
                .FirstOrDefaultAsync();

            if (material == null)
            {
                TempData["Error"] = "Material not found.";
                return RedirectToAction(nameof(MaterialDetails), new { id });
            }

            if (material.Quantity <= 0)
            {
                TempData["Error"] = "This material is out of stock.";
                return RedirectToAction(nameof(MaterialDetails), new { id });
            }

            var pricing = _pricingOptions.Value;

            payMode = (payMode ?? "stripe").Trim().ToLowerInvariant();

            var total = Round2(material.Price * quantity);
            var depositRequired = Round2(total * pricing.DepositPercent);

            // clamp payAmount
            payAmount = Math.Max(0m, payAmount);
            if (payAmount < depositRequired) payAmount = depositRequired;
            if (payAmount > total) payAmount = total;
            payAmount = Round2(payAmount);

            // wallet available
            var walletRow = await _db.Wallets.AsNoTracking()
                .Where(w => w.UserId == _currentUserId)
                .Select(w => new { w.Balance, w.ReservedBalance })
                .FirstOrDefaultAsync();

            var available = walletRow == null ? 0m : WalletAvailableFromDb(walletRow.Balance, walletRow.ReservedBalance).available;

            walletAmount = Math.Max(0m, walletAmount);

            if (payMode == "split")
            {
                walletAmount = Math.Min(walletAmount, available);
                walletAmount = Math.Min(walletAmount, payAmount);
                walletAmount = Round2(walletAmount);
            }
            else
            {
                walletAmount = 0m;
                payMode = "stripe";
            }

            var stripeAmount = Round2(payAmount - walletAmount);
            if (stripeAmount < 0m) stripeAmount = 0m;

            // wallet-only
            if (stripeAmount <= 0m)
            {
                var paidNow = Round2(walletAmount);

                var res = await _factoryService.PlaceMaterialOrderAsync(
                    buyerId: _currentUserId,
                    materialId: id,
                    quantity: quantity,
                    depositPaid: paidNow,
                    walletUsed: walletAmount,
                    provider: "Wallet",
                    providerPaymentId: "WALLET-" + Guid.NewGuid().ToString("N"),
                    ct: HttpContext.RequestAborted
                );

                TempData[res.Success ? "Success" : "Error"] = res.Message;
                return res.Success
                    ? RedirectToAction(nameof(MaterialOrderDetails), new { id = res.idObj })
                    : RedirectToAction(nameof(MaterialDetails), new { id });
            }

            // stripe checkout
            var successUrl = Url.Action(nameof(ConfirmMaterialOrderPayment), "CraftsMan", null, Request.Scheme)!;
            var cancelUrl = Url.Action(nameof(MaterialDetails), "CraftsMan", new { id }, Request.Scheme)!;

            var inv = CultureInfo.InvariantCulture;

            var metadata = new Dictionary<string, string>
            {
                ["UserId"] = _currentUserId.ToString(inv),
                ["MaterialId"] = id.ToString(inv),
                ["Quantity"] = quantity.ToString(inv),

                ["PayMode"] = payMode,
                ["WalletAmount"] = walletAmount.ToString("0.00", inv),

                ["Total"] = total.ToString("0.00", inv),
                ["DepositRequired"] = depositRequired.ToString("0.00", inv),
                ["PayAmount"] = payAmount.ToString("0.00", inv),
                ["StripeAmount"] = stripeAmount.ToString("0.00", inv),

                ["ClientOrderKey"] = Guid.NewGuid().ToString("N")
            };

            var stripeAmountCents = (long)Math.Round(stripeAmount * 100m, 0, MidpointRounding.AwayFromZero);

            var payResult = await _stripePaymentService.CreateCheckoutSessionAsync(
                successUrl: successUrl,
                cancelUrl: cancelUrl,
                name: $"Payment for Material #{id}",
                amountCents: stripeAmountCents,
                currency: "egp",
                metadata: metadata
            );

            if (!payResult.Success || string.IsNullOrWhiteSpace(payResult.RedirectUrl))
            {
                TempData["Error"] = payResult.Message ?? "Payment initialization failed.";
                return RedirectToAction(nameof(MaterialDetails), new { id });
            }

            return Redirect(payResult.RedirectUrl);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmMaterialOrderPayment(string session_id, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(session_id))
                return BadRequest("Missing session_id");

            StripeCheckoutSession session;
            try
            {
                session = new StripeCheckoutSessionService().Get(session_id);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Failed to read Stripe session: " + ex.Message;
                return RedirectToAction(nameof(PublicMaterialStore));
            }

            var status = "";
            try { status = session.PaymentStatus; }
            catch { try { status = session.Status; } catch { } }

            if (!string.Equals(status, "paid", StringComparison.OrdinalIgnoreCase))
            {
                TempData["Error"] = "Payment not completed.";
                return RedirectToAction(nameof(PublicMaterialStore));
            }

            if (session.Metadata == null ||
                !session.Metadata.TryGetValue("UserId", out var userIdStr) ||
                !int.TryParse(userIdStr, out var userId))
            {
                TempData["Error"] = "Missing UserId in payment metadata.";
                return RedirectToAction(nameof(PublicMaterialStore));
            }

            if (!(User?.Identity?.IsAuthenticated ?? false))
            {
                var returnUrl = Url.Action(nameof(ConfirmMaterialOrderPayment), "CraftsMan", new { session_id }, Request.Scheme);
                return RedirectToAction("Login", "Auth", new { returnUrl });
            }

            var currentUserIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(currentUserIdStr, out var currentUserId) || currentUserId != userId)
            {
                TempData["Error"] = "Payment user mismatch.";
                return RedirectToAction(nameof(PublicMaterialStore));
            }

            int materialId = int.Parse(session.Metadata["MaterialId"]);
            int quantity = int.Parse(session.Metadata["Quantity"]);

            var inv = CultureInfo.InvariantCulture;
            decimal walletAmount = decimal.Parse(session.Metadata["WalletAmount"], inv);
            decimal depositRequired = decimal.Parse(session.Metadata["DepositRequired"], inv);
            decimal payAmount = decimal.Parse(session.Metadata["PayAmount"], inv);
            decimal stripeAmount = decimal.Parse(session.Metadata["StripeAmount"], inv);

            if (payAmount < depositRequired) payAmount = depositRequired;
            if (walletAmount > payAmount) walletAmount = payAmount;

            var paidNow = Round2(walletAmount + stripeAmount);
            if (paidNow > payAmount) paidNow = payAmount;
            if (paidNow < depositRequired) paidNow = depositRequired;

            var res = await _factoryService.PlaceMaterialOrderAsync(
                buyerId: currentUserId,
                materialId: materialId,
                quantity: quantity,
                depositPaid: paidNow,
                walletUsed: walletAmount,
                provider: "Stripe",
                providerPaymentId: session.PaymentIntentId ?? session.Id,
                ct: ct
            );

            TempData[res.Success ? "Success" : "Error"] = res.Message;

            return res.Success
                ? RedirectToAction(nameof(MaterialOrderDetails), new { id = res.idObj })
                : RedirectToAction(nameof(MaterialDetails), new { id = materialId });
        }

        [HttpGet]
        public async Task<IActionResult> MyMaterialOrders()
        {
            var list = await _factoryService.GetMaterialOrdersForBuyerAsync(_currentUserId);
            ViewBag.Title = "My Material Orders";
            return View("Materials/MyMaterialOrders", list);
        }

        [HttpGet]
        public async Task<IActionResult> MaterialOrderDetails(int id)
        {
            var (order, item) = await _factoryService.GetMaterialOrderDetailsAsync(_currentUserId, id);
            if (order == null || item == null) return NotFound();

            ViewBag.OrderId = order.MaterialOrderID;
            ViewBag.OrderStatus = order.Status.ToString();
            ViewBag.OrderDate = order.OrderDate;
            ViewBag.CanCancel = item.CanCancel;

            ViewBag.ExpectedArrivalDate = order.ExpectedArrivalDate;
            ViewBag.CancelUntil = order.CancelUntil;
            ViewBag.OrderQuantity = order.Quantity;

            ViewBag.UnitPrice = order.UnitPrice;
            ViewBag.PickupLocation = order.PickupLocation;

            ViewBag.TotalPrice = order.TotalPrice;
            ViewBag.DepositPaid = order.DepositPaid;

            await LoadSellerContactAsync(item.SellerUserId);

            return View("Materials/MaterialOrderDetails", item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelMaterialOrder(int orderId)
        {
            var res = await _factoryService.CancelMaterialOrderAsync(_currentUserId, orderId);
            TempData[res.Success ? "Success" : "Error"] = res.Message;
            return RedirectToAction(nameof(MyMaterialOrders));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMaterialOrder(int orderId)
        {
            var res = await _factoryService.HideMaterialOrderForBuyerAsync(_currentUserId, orderId);
            TempData[res.Success ? "Success" : "Error"] = res.Message;
            return RedirectToAction(nameof(MyMaterialOrders));
        }

        /////////////////////////// Machine Management ///////////////////////////
        [HttpGet]
        public async Task<IActionResult> MachineDetails(int id)
        {
            var item = await _factoryService.GetPublicMachineDetailsAsync(id);
            if (item == null)
            {
                TempData["Error"] = "This machine is no longer available.";
                return RedirectToAction(nameof(PublicMachineStore));
            }

            await LoadSellerContactAsync(item.SellerUserId);

            var pricing = _pricingOptions.Value;
            ViewBag.AuctionDepositPercent = pricing.AuctionDepositPercent;

            return View("PublicDetails", item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateMachineOrder(int id, int quantity, string payMode, decimal walletAmount, decimal payAmount)
        {
            if (quantity <= 0)
            {
                TempData["Error"] = "Invalid quantity.";
                return RedirectToAction(nameof(MachineDetails), new { id });
            }

            var machine = await _db.MachineStores.AsNoTracking()
                .Where(m => m.MachineID == id)
                .Select(m => new { m.MachineID, m.Price, m.Quantity, m.Status })
                .FirstOrDefaultAsync();

            if (machine == null)
            {
                TempData["Error"] = "Machine not found.";
                return RedirectToAction(nameof(MachineDetails), new { id });
            }

            if (machine.Quantity <= 0)
            {
                TempData["Error"] = "This machine is out of stock.";
                return RedirectToAction(nameof(MachineDetails), new { id });
            }

            var pricing = _pricingOptions.Value;

            payMode = (payMode ?? "stripe").Trim().ToLowerInvariant();

            var total = Round2(machine.Price * quantity);
            var depositRequired = Round2(total * pricing.DepositPercent);

            payAmount = Math.Max(0m, payAmount);
            if (payAmount < depositRequired) payAmount = depositRequired;
            if (payAmount > total) payAmount = total;
            payAmount = Round2(payAmount);

            var walletRow = await _db.Wallets.AsNoTracking()
                .Where(w => w.UserId == _currentUserId)
                .Select(w => new { w.Balance, w.ReservedBalance })
                .FirstOrDefaultAsync();

            var available = walletRow == null ? 0m : WalletAvailableFromDb(walletRow.Balance, walletRow.ReservedBalance).available;

            walletAmount = Math.Max(0m, walletAmount);

            if (payMode == "split")
            {
                walletAmount = Math.Min(walletAmount, available);
                walletAmount = Math.Min(walletAmount, payAmount);
                walletAmount = Round2(walletAmount);
            }
            else
            {
                walletAmount = 0m;
                payMode = "stripe";
            }

            var stripeAmount = Round2(payAmount - walletAmount);
            if (stripeAmount < 0m) stripeAmount = 0m;

            if (stripeAmount <= 0m)
            {
                var paidNow = Round2(walletAmount);

                var res = await _factoryService.PlaceMachineOrderAsync(
                    buyerId: _currentUserId,
                    machineId: id,
                    quantity: quantity,
                    paidAmount: paidNow,
                    walletUsed: walletAmount,
                    provider: "Wallet",
                    providerPaymentId: "WALLET-" + Guid.NewGuid().ToString("N"),
                    ct: HttpContext.RequestAborted
                );

                TempData[res.Success ? "Success" : "Error"] = res.Message;
                return res.Success
                    ? RedirectToAction(nameof(MachineOrderDetails), new { id = res.idObj })
                    : RedirectToAction(nameof(MachineDetails), new { id });
            }

            var successUrl = Url.Action(nameof(ConfirmMachineOrderPayment), "CraftsMan", null, Request.Scheme)!;
            var cancelUrl = Url.Action(nameof(MachineDetails), "CraftsMan", new { id }, Request.Scheme)!;

            var inv = CultureInfo.InvariantCulture;

            var metadata = new Dictionary<string, string>
            {
                ["UserId"] = _currentUserId.ToString(inv),
                ["MachineId"] = id.ToString(inv),
                ["Quantity"] = quantity.ToString(inv),

                ["PayMode"] = payMode,
                ["WalletAmount"] = walletAmount.ToString("0.00", inv),

                ["Total"] = total.ToString("0.00", inv),
                ["DepositRequired"] = depositRequired.ToString("0.00", inv),

                ["PayAmount"] = payAmount.ToString("0.00", inv),
                ["StripeAmount"] = stripeAmount.ToString("0.00", inv),

                ["ClientOrderKey"] = Guid.NewGuid().ToString("N")
            };

            var stripeAmountCents = (long)Math.Round(stripeAmount * 100m, 0, MidpointRounding.AwayFromZero);

            var payResult = await _stripePaymentService.CreateCheckoutSessionAsync(
                successUrl: successUrl,
                cancelUrl: cancelUrl,
                name: $"Payment for Machine #{id}",
                amountCents: stripeAmountCents,
                currency: "egp",
                metadata: metadata
            );

            if (!payResult.Success || string.IsNullOrWhiteSpace(payResult.RedirectUrl))
            {
                TempData["Error"] = payResult.Message ?? "Payment initialization failed.";
                return RedirectToAction(nameof(MachineDetails), new { id });
            }

            return Redirect(payResult.RedirectUrl);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmMachineOrderPayment(string session_id, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(session_id))
                return BadRequest("Missing session_id");

            StripeCheckoutSession session;
            try
            {
                session = new StripeCheckoutSessionService().Get(session_id);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Failed to read Stripe session: " + ex.Message;
                return RedirectToAction(nameof(PublicMachineStore));
            }

            var status = "";
            try { status = session.PaymentStatus; }
            catch { try { status = session.Status; } catch { } }

            if (!string.Equals(status, "paid", StringComparison.OrdinalIgnoreCase))
            {
                TempData["Error"] = "Payment not completed.";
                return RedirectToAction(nameof(PublicMachineStore));
            }

            if (session.Metadata == null ||
                !session.Metadata.TryGetValue("UserId", out var userIdStr) ||
                !int.TryParse(userIdStr, out var userId))
            {
                TempData["Error"] = "Missing UserId in payment metadata.";
                return RedirectToAction(nameof(PublicMachineStore));
            }

            if (!(User?.Identity?.IsAuthenticated ?? false))
            {
                var returnUrl = Url.Action(nameof(ConfirmMachineOrderPayment), "CraftsMan", new { session_id }, Request.Scheme);
                return RedirectToAction("Login", "Auth", new { returnUrl });
            }

            var currentUserIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(currentUserIdStr, out var currentUserId) || currentUserId != userId)
            {
                TempData["Error"] = "Payment user mismatch.";
                return RedirectToAction(nameof(PublicMachineStore));
            }

            int machineId = int.Parse(session.Metadata["MachineId"]);
            int quantity = int.Parse(session.Metadata["Quantity"]);

            var inv = CultureInfo.InvariantCulture;
            decimal walletAmount = decimal.Parse(session.Metadata["WalletAmount"], inv);
            decimal depositRequired = decimal.Parse(session.Metadata["DepositRequired"], inv);
            decimal payAmount = decimal.Parse(session.Metadata["PayAmount"], inv);
            decimal stripeAmount = decimal.Parse(session.Metadata["StripeAmount"], inv);

            if (payAmount < depositRequired) payAmount = depositRequired;
            if (walletAmount > payAmount) walletAmount = payAmount;

            var paidNow = Round2(walletAmount + stripeAmount);
            if (paidNow > payAmount) paidNow = payAmount;
            if (paidNow < depositRequired) paidNow = depositRequired;

            var res = await _factoryService.PlaceMachineOrderAsync(
                buyerId: currentUserId,
                machineId: machineId,
                quantity: quantity,
                paidAmount: paidNow,
                walletUsed: walletAmount,
                provider: "Stripe",
                providerPaymentId: session.PaymentIntentId ?? session.Id,
                ct: ct
            );

            TempData[res.Success ? "Success" : "Error"] = res.Message;

            return res.Success
                ? RedirectToAction(nameof(MachineOrderDetails), new { id = res.idObj })
                : RedirectToAction(nameof(MachineDetails), new { id = machineId });
        }

        [HttpGet]
        public async Task<IActionResult> MyMachineOrders()
        {
            var list = await _factoryService.GetMachineOrdersForBuyerAsync(_currentUserId);
            ViewBag.Title = "My Machine Orders";
            return View("Machines/MyMachineOrders", list);
        }

        [HttpGet]
        public async Task<IActionResult> MachineOrderDetails(int id)
        {
            var (order, item) = await _factoryService.GetMachineOrderDetailsAsync(_currentUserId, id);
            if (order == null || item == null) return NotFound();

            ViewBag.OrderId = order.MachineOrderID;
            ViewBag.OrderStatus = order.Status.ToString();
            ViewBag.OrderDate = order.OrderDate;
            ViewBag.CanCancel = item.CanCancel;

            ViewBag.ExpectedArrivalDate = order.ExpectedArrivalDate;
            ViewBag.CancelUntil = order.CancelUntil;
            ViewBag.OrderQuantity = order.Quantity;

            ViewBag.UnitPrice = order.UnitPrice;
            ViewBag.PickupLocation = order.PickupLocation;

            ViewBag.TotalPrice = order.TotalPrice;
            ViewBag.DepositPaid = order.DepositPaid;

            await LoadSellerContactAsync(item.SellerUserId);

            return View("Machines/MachineOrderDetails", item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelMachineOrder(int orderId)
        {
            var res = await _factoryService.CancelMachineOrderAsync(_currentUserId, orderId);
            TempData[res.Success ? "Success" : "Error"] = res.Message;
            return RedirectToAction(nameof(MyMachineOrders));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMachineOrder(int orderId)
        {
            var res = await _factoryService.HideMachineOrderForBuyerAsync(_currentUserId, orderId);
            TempData[res.Success ? "Success" : "Error"] = res.Message;
            return RedirectToAction(nameof(MyMachineOrders));
        }

        /////////////////////////// Rentals Management ///////////////////////////
        [HttpGet]
        public async Task<IActionResult> RentalDetails(int id)
        {
            int? viewerId = User.Identity?.IsAuthenticated == true ? _currentUserId : (int?)null;

            var item = await _factoryService.GetPublicRentalDetailsAsync(id, viewerId);
            if (item == null)
            {
                TempData["Error"] = "This rental is no longer available.";
                return RedirectToAction(nameof(PublicRentalStore));
            }

            if (viewerId.HasValue)
            {
                var myOrder = await _db.RentalOrders.AsNoTracking()
                    .Where(o => o.RentalStoreID == id && o.BuyerID == viewerId.Value)
                    .OrderByDescending(o => o.OrderDate)
                    .Select(o => new { o.RentalOrderID, o.Status })
                    .FirstOrDefaultAsync();

                if (myOrder != null)
                {
                    ViewBag.MyOrderId = myOrder.RentalOrderID;
                    ViewBag.MyOrderStatus = myOrder.Status.ToString();
                }

                var wallet = await _db.Wallets.AsNoTracking()
                    .Where(w => w.UserId == viewerId.Value)
                    .Select(w => new { w.Balance })
                    .FirstOrDefaultAsync();

                ViewBag.WalletBalance = wallet?.Balance ?? 0m;
                ViewBag.WalletCurrency = "EGP";
            }
            else
            {
                ViewBag.WalletBalance = 0m;
                ViewBag.WalletCurrency = "EGP";
            }

            if (!item.IsPrivate)
                await LoadSellerContactAsync(item.SellerUserId);

            ViewBag.AuctionDepositPercent = _pricingOptions.Value.AuctionDepositPercent;
            return View("PublicDetails", item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRentalOrder(int id, string payMode, decimal walletAmount, decimal payAmount)
        {
            var rental = await _db.RentalStores.AsNoTracking()
                .Where(r => r.RentalID == id)
                .Select(r => new { r.RentalID, r.PricePerMonth, r.Status, r.OwnerID })
                .FirstOrDefaultAsync();

            if (rental == null)
            {
                TempData["Error"] = "Rental not found.";
                return RedirectToAction(nameof(PublicRentalStore));
            }

            if (rental.OwnerID == _currentUserId)
            {
                TempData["Error"] = "You can't order your own listing.";
                return RedirectToAction(nameof(RentalDetails), new { id });
            }

            if (rental.Status != ProductStatus.Available)
            {
                TempData["Error"] = "This rental is not available.";
                return RedirectToAction(nameof(RentalDetails), new { id });
            }

            payMode = (payMode ?? "stripe").Trim().ToLowerInvariant();
            walletAmount = Math.Max(0m, walletAmount);

            var required = Round2(rental.PricePerMonth * 3m);
            if (required <= 0m)
            {
                TempData["Error"] = "Invalid rental price.";
                return RedirectToAction(nameof(RentalDetails), new { id });
            }

            // required upfront exactly
            payAmount = Math.Max(0m, payAmount);
            if (payAmount < required) payAmount = required;
            if (payAmount > required) payAmount = required;
            payAmount = Round2(payAmount);

            var walletRow = await _db.Wallets.AsNoTracking()
                .Where(w => w.UserId == _currentUserId)
                .Select(w => new { w.Balance, w.ReservedBalance })
                .FirstOrDefaultAsync();

            var available = walletRow == null ? 0m : WalletAvailableFromDb(walletRow.Balance, walletRow.ReservedBalance).available;

            if (payMode == "split")
            {
                walletAmount = Math.Min(walletAmount, available);
                walletAmount = Math.Min(walletAmount, payAmount);
                walletAmount = Round2(walletAmount);
            }
            else
            {
                walletAmount = 0m;
                payMode = "stripe";
            }

            var stripeAmount = Round2(payAmount - walletAmount);
            if (stripeAmount < 0m) stripeAmount = 0m;

            if (stripeAmount <= 0m)
            {
                var res = await _factoryService.PlaceRentalOrderAsync(
                    buyerId: _currentUserId,
                    rentalId: id,
                    amountPaid: payAmount,
                    walletUsed: walletAmount,
                    provider: "Wallet",
                    providerPaymentId: "WALLET-" + Guid.NewGuid().ToString("N"),
                    ct: HttpContext.RequestAborted
                );

                TempData[res.Success ? "Success" : "Error"] = res.Message;
                return res.Success
                    ? RedirectToAction(nameof(MyRentalOrders))
                    : RedirectToAction(nameof(RentalDetails), new { id });
            }

            var successUrl = Url.Action(nameof(ConfirmRentalOrderPayment), "CraftsMan", null, Request.Scheme)!;
            var cancelUrl = Url.Action(nameof(RentalDetails), "CraftsMan", new { id }, Request.Scheme)!;

            var inv = CultureInfo.InvariantCulture;

            var metadata = new Dictionary<string, string>
            {
                ["UserId"] = _currentUserId.ToString(inv),
                ["RentalId"] = id.ToString(inv),

                ["WalletAmount"] = walletAmount.ToString("0.00", inv),
                ["PayAmount"] = payAmount.ToString("0.00", inv),
                ["StripeAmount"] = stripeAmount.ToString("0.00", inv),

                ["Months"] = "3",
                ["ClientOrderKey"] = Guid.NewGuid().ToString("N")
            };

            var stripeAmountCents = (long)Math.Round(stripeAmount * 100m, 0, MidpointRounding.AwayFromZero);

            var payResult = await _stripePaymentService.CreateCheckoutSessionAsync(
                successUrl: successUrl,
                cancelUrl: cancelUrl,
                name: $"Rental booking (3 months) #{id}",
                amountCents: stripeAmountCents,
                currency: "egp",
                metadata: metadata
            );

            if (!payResult.Success || string.IsNullOrWhiteSpace(payResult.RedirectUrl))
            {
                TempData["Error"] = payResult.Message ?? "Payment initialization failed.";
                return RedirectToAction(nameof(RentalDetails), new { id });
            }

            return Redirect(payResult.RedirectUrl);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmRentalOrderPayment(string session_id, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(session_id))
                return BadRequest("Missing session_id");

            StripeCheckoutSession session;
            try
            {
                session = new StripeCheckoutSessionService().Get(session_id);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Failed to read Stripe session: " + ex.Message;
                return RedirectToAction(nameof(PublicRentalStore));
            }

            var status = "";
            try { status = session.PaymentStatus; }
            catch { try { status = session.Status; } catch { } }

            if (!string.Equals(status, "paid", StringComparison.OrdinalIgnoreCase))
            {
                TempData["Error"] = "Payment not completed.";
                return RedirectToAction(nameof(PublicRentalStore));
            }

            if (session.Metadata == null ||
                !session.Metadata.TryGetValue("UserId", out var userIdStr) ||
                !int.TryParse(userIdStr, out var userId))
            {
                TempData["Error"] = "Missing UserId in payment metadata.";
                return RedirectToAction(nameof(PublicRentalStore));
            }

            if (!(User?.Identity?.IsAuthenticated ?? false))
            {
                var returnUrl = Url.Action(nameof(ConfirmRentalOrderPayment), "CraftsMan", new { session_id }, Request.Scheme);
                return RedirectToAction("Login", "Auth", new { returnUrl });
            }

            var currentUserIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(currentUserIdStr, out var currentUserId) || currentUserId != userId)
            {
                TempData["Error"] = "Payment user mismatch.";
                return RedirectToAction(nameof(PublicRentalStore));
            }

            int rentalId = int.Parse(session.Metadata["RentalId"]);
            var inv = CultureInfo.InvariantCulture;

            decimal walletAmount = decimal.Parse(session.Metadata["WalletAmount"], inv);
            decimal payAmount = decimal.Parse(session.Metadata["PayAmount"], inv);
            decimal stripeAmount = decimal.Parse(session.Metadata["StripeAmount"], inv);

            walletAmount = Round2(Math.Max(0m, walletAmount));
            payAmount = Round2(Math.Max(0m, payAmount));
            stripeAmount = Round2(Math.Max(0m, stripeAmount));

            var paidNow = Round2(walletAmount + stripeAmount);

            if (Math.Abs(paidNow - payAmount) > 0.01m)
            {
                TempData["Error"] = "Payment amount mismatch.";
                return RedirectToAction(nameof(RentalDetails), new { id = rentalId });
            }

            var res = await _factoryService.PlaceRentalOrderAsync(
                buyerId: currentUserId,
                rentalId: rentalId,
                amountPaid: paidNow,
                walletUsed: walletAmount,
                provider: "Stripe",
                providerPaymentId: session.PaymentIntentId ?? session.Id,
                ct: ct
            );

            TempData[res.Success ? "Success" : "Error"] = res.Message;

            return res.Success
                ? RedirectToAction(nameof(MyRentalOrders))
                : RedirectToAction(nameof(RentalDetails), new { id = rentalId });
        }

        [HttpGet]
        public async Task<IActionResult> MyRentalOrders(CancellationToken ct)
        {
            var list = await _factoryService.GetRentalOrdersForBuyerAsync(_currentUserId, take: 300, ct);
            ViewBag.Title = "My Rental Orders";
            return View("Rentals/MyRentalOrders", list);
        }

        [HttpGet]
        public async Task<IActionResult> RentalOrderDetails(int id, CancellationToken ct)
        {
            var (order, item) = await _factoryService.GetRentalOrderDetailsAsync(_currentUserId, id, ct);
            if (order == null || item == null) return NotFound();

            ViewBag.OrderId = order.RentalOrderID;
            ViewBag.OrderStatus = order.Status.ToString();
            ViewBag.OrderDate = order.OrderDate;

            if (!string.IsNullOrWhiteSpace(item.SellerEmail)) item.SellerEmail = DecryptOrRaw(item.SellerEmail) ?? item.SellerEmail;
            if (!string.IsNullOrWhiteSpace(item.SellerPhone)) item.SellerPhone = DecryptOrRaw(item.SellerPhone) ?? item.SellerPhone;

            ViewBag.CanDeletePending = order.Status == EnumsOrderStatus.Pending;

            ViewBag.AmountPaid = order.AmountPaid;
            ViewBag.MonthsPaid = 3;

            return View("Rentals/RentalOrderDetails", (order, item));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelRentalOrder(int orderId, CancellationToken ct)
        {
            var res = await _factoryService.CancelOrDeleteRentalOrderByBuyerAsync(_currentUserId, orderId, ct);
            TempData[res.Success ? "Success" : "Error"] = res.Message;
            return RedirectToAction(nameof(MyRentalOrders));
        }

        /////////////////////////// Auction Management ///////////////////////////
        [HttpGet]
        public async Task<IActionResult> AuctionDetails(int id)
        {
            var item = await _factoryService.GetPublicAuctionDetailsAsync(id);
            if (item == null)
            {
                TempData["Error"] = "This auction is no longer available.";
                return RedirectToAction(nameof(PublicAuctionStore));
            }

            await LoadSellerContactAsync(item.SellerUserId);

            ViewBag.AuctionDepositPercent = _pricingOptions.Value.AuctionDepositPercent;
            return View("PublicDetails", item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAuctionOrder(int id, string payMode, decimal walletAmount, decimal payAmount, decimal bidAmount)
        {
            payMode = (payMode ?? "stripe").Trim().ToLowerInvariant();
            walletAmount = Math.Max(0m, walletAmount);

            bidAmount = Round2(Math.Max(0m, bidAmount));
            payAmount = Round2(Math.Max(0m, payAmount));

            if (bidAmount <= 0m)
            {
                TempData["Error"] = "Invalid bid amount.";
                return RedirectToAction(nameof(AuctionDetails), new { id });
            }

            var auction = await _db.AuctionStores.AsNoTracking()
                .Where(a => a.AuctionID == id)
                .Select(a => new
                {
                    a.AuctionID,
                    a.SellerID,
                    a.Status,
                    a.StartDate,
                    a.EndDate,
                    a.StartPrice
                })
                .FirstOrDefaultAsync();

            if (auction == null)
            {
                TempData["Error"] = "Auction not found.";
                return RedirectToAction(nameof(PublicAuctionStore));
            }

            if (auction.SellerID == _currentUserId)
            {
                TempData["Error"] = "You can't bid on your own auction.";
                return RedirectToAction(nameof(AuctionDetails), new { id });
            }

            if (auction.Status != ProductStatus.Available)
            {
                TempData["Error"] = "This auction is not available.";
                return RedirectToAction(nameof(AuctionDetails), new { id });
            }

            if (DateTime.UtcNow < auction.StartDate)
            {
                TempData["Error"] = $"Auction has not started yet. Starts at {auction.StartDate.ToLocalTime():yyyy-MM-dd HH:mm}.";
                return RedirectToAction(nameof(AuctionDetails), new { id });
            }

            if (!auction.EndDate.HasValue || DateTime.UtcNow >= auction.EndDate.Value)
            {
                TempData["Error"] = "Auction already ended.";
                return RedirectToAction(nameof(AuctionDetails), new { id });
            }

            var topBid = await _db.AuctionOrders.AsNoTracking()
                .Where(o => o.AuctionStoreID == id
                            && o.Status != EnumsOrderStatus.Cancelled
                            && o.Status != EnumsOrderStatus.DeletedByBuyer
                            && o.Status != EnumsOrderStatus.DeletedBySeller)
                .MaxAsync(o => (decimal?)o.BidAmount) ?? 0m;

            if (topBid > 0m)
            {
                if (bidAmount <= topBid)
                {
                    TempData["Error"] = $"Bid must be higher than current top bid ({topBid:0.00}).";
                    return RedirectToAction(nameof(AuctionDetails), new { id });
                }
            }
            else
            {
                if (bidAmount < auction.StartPrice)
                {
                    TempData["Error"] = $"Bid must be at least start price ({auction.StartPrice:0.00}).";
                    return RedirectToAction(nameof(AuctionDetails), new { id });
                }
            }

            var pricing = _pricingOptions.Value;
            var depPct = pricing.AuctionDepositPercent;
            if (depPct <= 0m) depPct = 0.30m;

            var depositRequired = Round2(bidAmount * depPct);
            if (depositRequired <= 0m)
            {
                TempData["Error"] = "Invalid deposit calculated.";
                return RedirectToAction(nameof(AuctionDetails), new { id });
            }

            // deposit must be exact
            if (payAmount < depositRequired) payAmount = depositRequired;
            if (payAmount > depositRequired) payAmount = depositRequired;
            payAmount = Round2(payAmount);

            var walletRow = await _db.Wallets.AsNoTracking()
                .Where(w => w.UserId == _currentUserId)
                .Select(w => new { w.Balance, w.ReservedBalance })
                .FirstOrDefaultAsync();

            var available = walletRow == null ? 0m : WalletAvailableFromDb(walletRow.Balance, walletRow.ReservedBalance).available;

            if (payMode == "split")
            {
                walletAmount = Math.Min(walletAmount, available);
                walletAmount = Math.Min(walletAmount, payAmount);
                walletAmount = Round2(walletAmount);
            }
            else
            {
                walletAmount = 0m;
                payMode = "stripe";
            }

            var stripeAmount = Round2(payAmount - walletAmount);
            if (stripeAmount < 0m) stripeAmount = 0m;

            if (stripeAmount <= 0m)
            {
                var res = await _factoryService.PlaceAuctionOrderAsync(
                    winnerId: _currentUserId,
                    auctionId: id,
                    bidAmount: bidAmount,
                    amountPaid: payAmount,
                    walletUsed: walletAmount,
                    provider: "Wallet",
                    providerPaymentId: "WALLET-" + Guid.NewGuid().ToString("N"),
                    ct: HttpContext.RequestAborted
                );

                TempData[res.Success ? "Success" : "Error"] = res.Message;

                return res.Success
                    ? RedirectToAction(nameof(MyAuctionOrders))
                    : RedirectToAction(nameof(AuctionDetails), new { id });
            }

            var successUrl = Url.Action(nameof(ConfirmAuctionOrderPayment), "CraftsMan", null, Request.Scheme)!;
            var cancelUrl = Url.Action(nameof(AuctionDetails), "CraftsMan", new { id }, Request.Scheme)!;

            var inv = CultureInfo.InvariantCulture;

            var metadata = new Dictionary<string, string>
            {
                ["UserId"] = _currentUserId.ToString(inv),
                ["AuctionId"] = id.ToString(inv),

                ["BidAmount"] = bidAmount.ToString("0.00", inv),

                // ✅ use stable keys
                ["PayAmount"] = payAmount.ToString("0.00", inv),
                ["DepositRequired"] = depositRequired.ToString("0.00", inv),
                ["DepositPercent"] = depPct.ToString("0.00", inv),

                ["WalletAmount"] = walletAmount.ToString("0.00", inv),
                ["StripeAmount"] = stripeAmount.ToString("0.00", inv),

                ["ClientOrderKey"] = Guid.NewGuid().ToString("N")
            };

            var stripeAmountCents = (long)Math.Round(stripeAmount * 100m, 0, MidpointRounding.AwayFromZero);

            var payResult = await _stripePaymentService.CreateCheckoutSessionAsync(
                successUrl: successUrl,
                cancelUrl: cancelUrl,
                name: $"Auction deposit #{id}",
                amountCents: stripeAmountCents,
                currency: "egp",
                metadata: metadata
            );

            if (!payResult.Success || string.IsNullOrWhiteSpace(payResult.RedirectUrl))
            {
                TempData["Error"] = payResult.Message ?? "Payment initialization failed.";
                return RedirectToAction(nameof(AuctionDetails), new { id });
            }

            return Redirect(payResult.RedirectUrl);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmAuctionOrderPayment(string session_id, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(session_id))
                return BadRequest("Missing session_id");

            StripeCheckoutSession session;
            try
            {
                session = new StripeCheckoutSessionService().Get(session_id);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Failed to read Stripe session: " + ex.Message;
                return RedirectToAction(nameof(PublicAuctionStore));
            }

            var status = "";
            try { status = session.PaymentStatus; }
            catch { try { status = session.Status; } catch { } }

            if (!string.Equals(status, "paid", StringComparison.OrdinalIgnoreCase))
            {
                TempData["Error"] = "Payment not completed.";
                return RedirectToAction(nameof(PublicAuctionStore));
            }

            if (session.Metadata == null ||
                !session.Metadata.TryGetValue("UserId", out var userIdStr) ||
                !int.TryParse(userIdStr, out var userId))
            {
                TempData["Error"] = "Missing UserId in payment metadata.";
                return RedirectToAction(nameof(PublicAuctionStore));
            }

            if (!(User?.Identity?.IsAuthenticated ?? false))
            {
                var returnUrl = Url.Action(nameof(ConfirmAuctionOrderPayment), "CraftsMan", new { session_id }, Request.Scheme);
                return RedirectToAction("Login", "Auth", new { returnUrl });
            }

            var currentUserIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(currentUserIdStr, out var currentUserId) || currentUserId != userId)
            {
                TempData["Error"] = "Payment user mismatch.";
                return RedirectToAction(nameof(PublicAuctionStore));
            }

            int auctionId = int.Parse(session.Metadata["AuctionId"]);
            var inv = CultureInfo.InvariantCulture;

            // ✅ FIX: read correct keys (was Deposit before)
            decimal walletAmount = decimal.Parse(session.Metadata["WalletAmount"], inv);
            decimal payAmount = decimal.Parse(session.Metadata["PayAmount"], inv);
            decimal depositRequired = decimal.Parse(session.Metadata["DepositRequired"], inv);
            decimal stripeAmount = decimal.Parse(session.Metadata["StripeAmount"], inv);
            decimal bidAmount = decimal.Parse(session.Metadata["BidAmount"], inv);

            walletAmount = Round2(Math.Max(0m, walletAmount));
            payAmount = Round2(Math.Max(0m, payAmount));
            depositRequired = Round2(Math.Max(0m, depositRequired));
            stripeAmount = Round2(Math.Max(0m, stripeAmount));
            bidAmount = Round2(Math.Max(0m, bidAmount));

            // paidNow = wallet + stripe
            var paidNow = Round2(walletAmount + stripeAmount);

            // must equal deposit
            if (Math.Abs(paidNow - depositRequired) > 0.01m || Math.Abs(payAmount - depositRequired) > 0.01m)
            {
                TempData["Error"] = "Payment amount mismatch.";
                return RedirectToAction(nameof(AuctionDetails), new { id = auctionId });
            }

            var res = await _factoryService.PlaceAuctionOrderAsync(
                winnerId: currentUserId,
                auctionId: auctionId,
                bidAmount: bidAmount,
                amountPaid: paidNow,
                walletUsed: walletAmount,
                provider: "Stripe",
                providerPaymentId: session.PaymentIntentId ?? session.Id,
                ct: ct
            );

            TempData[res.Success ? "Success" : "Error"] = res.Message;

            return res.Success
                ? RedirectToAction(nameof(MyAuctionOrders))
                : RedirectToAction(nameof(AuctionDetails), new { id = auctionId });
        }

        [HttpGet]
        public async Task<IActionResult> MyAuctionOrders(CancellationToken ct)
        {
            var list = await _factoryService.GetAuctionOrdersForWinnerAsync(_currentUserId, take: 300, ct: ct);
            ViewBag.Title = "My Auction Orders";
            return View("Auction/MyAuctionOrders", list);
        }

        [HttpGet]
        public async Task<IActionResult> AuctionOrderDetails(int id, CancellationToken ct)
        {
            var (order, item) = await _factoryService.GetAuctionOrderDetailsAsync(_currentUserId, id, ct);
            if (order == null || item == null) return NotFound();

            ViewBag.OrderId = order.AuctionOrderID;
            ViewBag.OrderStatus = order.Status.ToString();
            ViewBag.OrderDate = order.OrderDate;

            ViewBag.CanDeletePending = order.Status == EnumsOrderStatus.Pending;

            ViewBag.BidAmount = order.BidAmount;
            ViewBag.DepositPaid = order.AmountPaid;

            return View("Auction/AuctionOrderDetails", (order, item));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelAuctionOrder(int orderId, CancellationToken ct)
        {
            var res = await _factoryService.CancelOrDeleteAuctionOrderByWinnerAsync(_currentUserId, orderId, ct);
            TempData[res.Success ? "Success" : "Error"] = res.Message;

            return RedirectToAction(nameof(MyAuctionOrders));
        }
    }
}