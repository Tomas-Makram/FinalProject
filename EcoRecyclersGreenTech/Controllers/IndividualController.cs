using EcoRecyclersGreenTech.Data.Orders;
using EcoRecyclersGreenTech.Data.Users;
using EcoRecyclersGreenTech.Models;
using EcoRecyclersGreenTech.Models.FactoryStore;
using EcoRecyclersGreenTech.Models.IndividualStore;
using EcoRecyclersGreenTech.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using static EcoRecyclersGreenTech.Data.Stores.EnumsProductStatus;

namespace EcoRecyclersGreenTech.Controllers
{
    [Authorize]
    public class IndividualController : Controller
    {
        private readonly DBContext _db;
        private readonly IFactoryStoreService _storeService;
        private readonly IDataCiphers _dataCiphers;
        private readonly IFilterAIService _filterAIService;
        private readonly ILogger<IndividualController> _logger;

        // Cached per-request (no repeat in every action)
        private User? _currentUser;
        private int _currentUserId;
        private bool _isLoggedIn;

        // Actions that should show verify alert (as you were doing)
        private static readonly HashSet<string> _showVerifyAlertActions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "JobDetails",
            "MyJobs",
            "Join",
            "Unjoin"
        };

        public IndividualController(DBContext db, IFactoryStoreService storeService, IDataCiphers dataCiphers, IFilterAIService filterAIService, ILogger<IndividualController> logger)
        {
            _db = db;
            _storeService = storeService;
            _dataCiphers = dataCiphers;
            _filterAIService = filterAIService;
            _logger = logger;
        }

        // login state + layout ViewBag + verification alert + blocked check
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            try
            {
                var actionName = context.ActionDescriptor.RouteValues.TryGetValue("action", out var a) ? a : "";

                var (ok, userId, user) = await ApplyLoginStateAsync(
                    showVerifyAlert: _showVerifyAlertActions.Contains(actionName!)
                );

                _isLoggedIn = ok;
                _currentUserId = userId;
                _currentUser = user;

                // Not logged in -> Auth/Login
                if (!ok || user == null)
                {
                    context.Result = RedirectToAction("Login", "Auth");
                    return;
                }

                // Blocked -> Home/Home
                if (user.Blocked)
                {
                    TempData["Error"] = "Your account is blocked. Contact support.";
                    context.Result = RedirectToAction("Home", "Home");
                    return;
                }

                // Decrypt email for layout (only once)
                if (!string.IsNullOrWhiteSpace(user.Email))
                {
                    try { ViewBag.Email = _dataCiphers.Decrypt(user.Email); }
                    catch { ViewBag.Email = user.Email; } // safe fallback
                }

                await next();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "IndividualController OnActionExecutionAsync failed.");

                // Safe fallback (base state)
                TempData["Error"] = "Something went wrong. Please try again.";
                context.Result = RedirectToAction("Home", "Home");
            }
        }

        // Single DB hit for user + type, sets layout ViewBags once
        private async Task<(bool ok, int userId, User? user)> ApplyLoginStateAsync(bool showVerifyAlert = false)
        {
            var emailFromSession = HttpContext.Session.GetString("UserEmail");
            var userId = HttpContext.Session.GetInt32("UserID");

            if (string.IsNullOrWhiteSpace(emailFromSession) || !userId.HasValue)
            {
                ViewBag.UserLoggedIn = false;
                return (false, 0, null);
            }

            ViewBag.UserLoggedIn = true;

            // Load full user with type in one query
            var user = await _db.Users
                .AsNoTracking()
                .Include(u => u.UserType)
                .FirstOrDefaultAsync(u => u.UserID == userId.Value);

            if (user == null)
            {
                ViewBag.UserLoggedIn = false;
                return (false, 0, null);
            }

            ViewBag.type = user.UserType?.TypeName;
            ViewBag.IsVerified = user.Verified;
            ViewBag.NeedsVerification = !user.Verified;

            // keep session email also available (some layouts use it)
            ViewBag.EmailSession = emailFromSession;

            if (showVerifyAlert && !user.Verified)
            {
                TempData["VerificationMessage"] =
                    $"Welcome {user.FullName}! Please go to settings to verification your account.";
                TempData["ShowVerificationAlert"] = "true";
            }

            return (true, user.UserID, user);
        }

        [HttpGet]
        public async Task<IActionResult> Index(IndividualJobsFilterVM filter)
        {
            // _currentUser is already validated + not blocked from the action filter
            var user = _currentUser!;
            var userId = _currentUserId;

            try
            {
                var myOcc = await _db.Individuals
                    .AsNoTracking()
                    .Where(i => i.UserID == userId)
                    .Select(i => i.Occupation)
                    .FirstOrDefaultAsync();

                var s = new SearchFilterModel
                {
                    Keyword = filter.Q,
                    MinPrice = filter.MinSalary,
                    MaxPrice = filter.MaxSalary,
                    ExperienceLevel = filter.ExperienceLevel,
                    Location = filter.Location,
                    SortBy = filter.SortBy ?? "newest",
                    SortDir = filter.SortDir ?? "desc",
                    UserLat = user.Latitude,
                    UserLng = user.Longitude,
                    MaxDistanceKm = filter.MaxKm
                };

                var list = await _storeService.GetPublicJobsAsync(s);

                // AI Map
                var matchMap = new Dictionary<int, double>();

                if (filter.UseSmartOccupationFilter)
                {
                    var occ = (myOcc ?? "").Trim();

                    if (string.IsNullOrWhiteSpace(occ))
                    {
                        TempData["Warning"] = "AI filter enabled but no occupation is saved in your account";
                    }
                    else
                    {
                        // Pre filter tokens
                        var occTokens = _filterAIService.Tokenize(occ).Take(6).ToList();

                        if (occTokens.Count > 0)
                        {
                            list = list.Where(x =>
                                _filterAIService.ContainsAnyToken(x.JobType, occTokens) ||
                                _filterAIService.ContainsAnyToken(x.RequiredSkills, occTokens) ||
                                _filterAIService.ContainsAnyToken(x.Description, occTokens) ||
                                _filterAIService.ContainsAnyToken(x.ExperienceLevel, occTokens) ||
                                _filterAIService.ContainsAnyToken(x.EmploymentType, occTokens)
                            ).ToList();
                        }

                        // Score
                        foreach (var x in list)
                        {
                            var combined = _filterAIService.Combine(x.JobType, x.RequiredSkills, x.Description, x.ExperienceLevel, x.EmploymentType);
                            var score = _filterAIService.Similarity(occ, combined);
                            matchMap[x.Id] = score;
                        }

                        var th = Math.Max(0.10, filter.SmartThreshold);

                        list = list
                            .Where(x => matchMap.TryGetValue(x.Id, out var sc) && sc >= th)
                            .OrderByDescending(x => matchMap[x.Id])
                            .ThenByDescending(x => x.CreatedAt)
                            .ToList();
                    }
                }

                // Expiring soon
                var today = DateTime.UtcNow.Date;
                var expiringSoon = list
                    .Where(x => x.JobExpiryDate.HasValue)
                    .Select(x => new { Title = x.JobType, DaysLeft = (x.JobExpiryDate!.Value.Date - today).TotalDays })
                    .Where(x => x.DaysLeft > 0 && x.DaysLeft <= 7)
                    .Take(5)
                    .ToList();

                var vm = new IndividualJobsIndexVM
                {
                    Filter = filter,
                    UserAddress = user.Address,
                    UserLat = user.Latitude,
                    UserLng = user.Longitude,
                    MyOccupation = myOcc,

                    ExpiringSoonCount = expiringSoon.Count,
                    ExpiringSoonTitles = expiringSoon.Select(x => x.Title ?? "Job").ToList(),

                    Jobs = list.Select(x => new IndividualJobCardVM
                    {
                        JobID = x.Id,
                        JobType = x.JobType,
                        Salary = x.JobSalary,
                        WorkHours = x.WorkHours,
                        Location = x.JobLocation,
                        Latitude = x.JobLatitude,
                        Longitude = x.JobLongitude,
                        CreatedAt = x.CreatedAt,
                        ExpiryDate = x.JobExpiryDate,
                        ExperienceLevel = x.ExperienceLevel,
                        EmploymentType = x.EmploymentType,
                        RequiredSkills = x.RequiredSkills,
                        Description = x.Description,

                        FactoryUserID = x.SellerUserId,
                        FactoryName = x.SellerName,
                        FactoryVerified = x.IsVerifiedSeller,
                        FactoryProfileImgUrl = x.SellerProfileImgUrl,

                        MatchScore = matchMap.TryGetValue(x.Id, out var sc) ? sc : (double?)null,
                        DistanceKm = null
                    }).ToList()
                };

                return View(vm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Index failed.");
                TempData["Error"] = "Failed to load jobs. Please try again.";
                return RedirectToAction("Home", "Home");
            }
        }

        [HttpGet]
        public async Task<IActionResult> JobDetails(int id)
        {
            var userId = _currentUserId;

            try
            {
                var job = await _storeService.GetPublicJobDetailsAsync(id);
                if (job == null) return NotFound();

                var factory = await _db.Factories.AsNoTracking().FirstOrDefaultAsync(f => f.UserID == job.SellerUserId);

                var joinedCount = await _db.JobOrders.AsNoTracking().CountAsync(o => o.JobStoreID == id);

                // bring status too
                var myOrder = await _db.JobOrders.AsNoTracking()
                    .Where(o => o.JobStoreID == id && o.UserID == userId)
                    .Select(o => new { o.JobOrderID, o.Status })
                    .FirstOrDefaultAsync();

                var factoryUser = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserID == job.SellerUserId);

                var imgs = new List<string>();
                if (factory != null)
                {
                    if (!string.IsNullOrWhiteSpace(factory.FactoryImgURL1)) imgs.Add(factory.FactoryImgURL1!);
                    if (!string.IsNullOrWhiteSpace(factory.FactoryImgURL2)) imgs.Add(factory.FactoryImgURL2!);
                    if (!string.IsNullOrWhiteSpace(factory.FactoryImgURL3)) imgs.Add(factory.FactoryImgURL3!);
                }

                var vm = new IndividualJobDetailsVM
                {
                    JobID = job.Id,
                    JobType = job.JobType,
                    Salary = job.JobSalary,
                    WorkHours = job.WorkHours,
                    Location = job.JobLocation,
                    Latitude = job.JobLatitude,
                    Longitude = job.JobLongitude,
                    CreatedAt = job.CreatedAt,
                    ExpiryDate = job.JobExpiryDate,
                    Description = job.Description,
                    ExperienceLevel = job.ExperienceLevel,
                    EmploymentType = job.EmploymentType,
                    RequiredSkills = job.RequiredSkills,
                    Status = job.Status,

                    FactoryUserID = job.SellerUserId,
                    FactoryName = job.SellerName,
                    FactoryVerified = job.IsVerifiedSeller,
                    FactoryProfileImgUrl = job.SellerProfileImgUrl,

                    FactoryType = factory?.FactoryType,
                    FactoryDescription = factory?.Description,

                    FactoryAddress = factoryUser?.Address,

                    FactoryImageUrls = imgs,
                    JoinedCount = joinedCount,

                    IsJoined = myOrder != null,
                    MyOrderId = myOrder?.JobOrderID,

                    // new
                    MyOrderStatus = myOrder != null ? myOrder.Status.ToString() : null
                };

                return View(vm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "JobDetails failed for JobID={JobID}", id);
                TempData["Error"] = "Failed to load job details. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        public async Task<IActionResult> MyJobs()
        {
            var userId = _currentUserId;

            try
            {
                var todayUtc = DateTime.UtcNow.Date;

                var list = await (
                    from o in _db.JobOrders.AsNoTracking()
                    where o.UserID == userId
                    join j in _db.JobStores.AsNoTracking() on o.JobStoreID equals j.JobID into gj
                    from j in gj.DefaultIfEmpty()
                    join u in _db.Users.AsNoTracking() on (int?)j.PostedBy equals (int?)u.UserID into gu
                    from u in gu.DefaultIfEmpty()
                    select new MyJoinedJobVM
                    {
                        JobOrderID = o.JobOrderID,
                        JobID = o.JobStoreID,
                        OrderDate = o.OrderDate,

                        JobType = j != null ? j.JobType : "(Job Deleted)",
                        Salary = j != null ? j.Salary : null,
                        Location = j != null ? j.Location : null,
                        ExpiryDate = j != null ? j.ExpiryDate : null,

                        FactoryName = u != null ? u.FullName : "(Factory Deleted)",
                        FactoryVerified = u != null && u.Verified,

                        IsDeletedByFactory = (j == null) || (j.Status != ProductStatus.Available),
                        DeletedNote = ((j == null) || (j.Status != ProductStatus.Available))
                            ? "Deleted by the Factory"
                            : null,

                        IsExpired = (j != null && j.ExpiryDate.HasValue && j.ExpiryDate.Value.Date <= todayUtc),
                        DaysLeft = (j != null && j.ExpiryDate.HasValue)
                            ? (int?)(j.ExpiryDate.Value.Date - todayUtc).TotalDays
                            : null,

                        // JobOrder Status (Pending/Confirmed/...)
                        OrderStatus = o.Status.ToString()
                    }
                ).ToListAsync();

                // ORDER: Deleted -> Expired -> Active (closest expiry first) -> NoExpiry
                list = list
                    .OrderBy(x => x.IsDeletedByFactory ? 0 : 1)
                    .ThenBy(x =>
                        x.ExpiryDate.HasValue
                            ? (x.ExpiryDate.Value.Date <= todayUtc ? 0 : 1)
                            : 2)
                    .ThenBy(x => x.ExpiryDate ?? DateTime.MaxValue)
                    .ThenByDescending(x => x.OrderDate)
                    .ToList();

                return View("MyJobs", list);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MyJobs failed.");
                TempData["Error"] = "Failed to load your jobs. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Join(int jobId)
        {
            var userId = _currentUserId;

            try
            {
                var exists = await _db.JobStores.AsNoTracking()
                    .AnyAsync(x => x.JobID == jobId && x.Status == ProductStatus.Available);

                if (!exists)
                {
                    TempData["Error"] = "Job not found or not available.";
                    return RedirectToAction(nameof(Index));
                }

                var already = await _db.JobOrders.AsNoTracking()
                    .AnyAsync(o => o.UserID == userId && o.JobStoreID == jobId);

                if (already)
                {
                    TempData["Warning"] = "You already joined this job.";
                    return RedirectToAction(nameof(JobDetails), new { id = jobId });
                }

                var order = new JobOrder
                {
                    UserID = userId,
                    JobStoreID = jobId,
                    Status = JobOrderStatus.Pending,
                    OrderDate = DateTime.UtcNow,
                };

                _db.JobOrders.Add(order);
                await _db.SaveChangesAsync();

                TempData["Success"] = "Joined successfully.";
                return RedirectToAction(nameof(JobDetails), new { id = jobId });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Join DbUpdateException for JobID={JobID}", jobId);
                TempData["Error"] = "Database error while joining. Please try again.";
                return RedirectToAction(nameof(JobDetails), new { id = jobId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Join failed for JobID={JobID}", jobId);
                TempData["Error"] = "Failed to join. Please try again.";
                return RedirectToAction(nameof(JobDetails), new { id = jobId });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unjoin(int jobId)
        {
            var userId = _currentUserId;

            try
            {
                var order = await _db.JobOrders
                    .FirstOrDefaultAsync(o => o.UserID == userId && o.JobStoreID == jobId);

                if (order == null)
                {
                    TempData["Warning"] = "You are not joined in this job.";
                    return RedirectToAction(nameof(JobDetails), new { id = jobId });
                }

                _db.JobOrders.Remove(order);
                await _db.SaveChangesAsync();

                TempData["Success"] = "Registration removed.";
                return RedirectToAction(nameof(JobDetails), new { id = jobId });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Unjoin DbUpdateException for JobID={JobID}", jobId);
                TempData["Error"] = "Database error while removing registration. Please try again.";
                return RedirectToAction(nameof(JobDetails), new { id = jobId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unjoin failed for JobID={JobID}", jobId);
                TempData["Error"] = "Failed to unjoin. Please try again.";
                return RedirectToAction(nameof(JobDetails), new { id = jobId });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteJobOrder(int jobOrderId)
        {
            var userId = _currentUserId;

            try
            {
                var order = await _db.JobOrders
                    .FirstOrDefaultAsync(o => o.JobOrderID == jobOrderId && o.UserID == userId);

                if (order == null)
                {
                    TempData["Error"] = "Job order not found.";
                    return RedirectToAction(nameof(MyJobs));
                }

                _db.JobOrders.Remove(order);
                await _db.SaveChangesAsync();

                TempData["Success"] = "Job removed from your list successfully.";
                return RedirectToAction(nameof(MyJobs));
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "DeleteJobOrder DbUpdateException for JobOrderID={JobOrderID}", jobOrderId);
                TempData["Error"] = "Database error while deleting. Please try again.";
                return RedirectToAction(nameof(MyJobs));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DeleteJobOrder failed for JobOrderID={JobOrderID}", jobOrderId);
                TempData["Error"] = "Failed to delete job from your list. Please try again.";
                return RedirectToAction(nameof(MyJobs));
            }
        }
    }
}