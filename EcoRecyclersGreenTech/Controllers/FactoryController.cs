using EcoRecyclersGreenTech.Models;
using EcoRecyclersGreenTech.Models.FactoryStore;
using EcoRecyclersGreenTech.Models.FactoryStore.Orders;
using EcoRecyclersGreenTech.Models.FactoryStore.Products;
using EcoRecyclersGreenTech.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EcoRecyclersGreenTech.Controllers
{
    [Authorize]
    public class FactoryController : Controller
    {
        private readonly IFactoryStoreService _storeService;
        private readonly DBContext _db;
        private readonly IDataCiphers _dataCiphers;
        private readonly ILocationService _locationService;
        private readonly ILogger<FactoryController> _logger;

        public FactoryController(IFactoryStoreService storeService, ILogger<FactoryController> logger, DBContext db, IDataCiphers dataCiphers, ILocationService locationService)
        {
            _storeService = storeService;
            _logger = logger;
            _db = db;
            _dataCiphers = dataCiphers;
            _locationService = locationService;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out int userId))
                return userId;

            return HttpContext.Session.GetInt32("UserID") ?? 0;
        }

        private string GetCurrentUserType()
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (!string.IsNullOrWhiteSpace(role))
                return role;

            // fallback: session stores TypeName as string (not ID)
            return HttpContext.Session.GetString("UserTypeName") ?? string.Empty;
        }

        private string GetCurrentUserTypeName()
        {
            // claims أولاً
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (!string.IsNullOrWhiteSpace(role))
                return role;

            var typeName = User.FindFirst("UserType")?.Value;
            if (!string.IsNullOrWhiteSpace(typeName))
                return typeName;

            // session fallback
            return HttpContext.Session.GetString("UserTypeName") ?? string.Empty;
        }

        private bool IsFactoryUser()
        {
            return GetCurrentUserType() == "Factory";
        }

        private bool IsVerifiedFactory()
        {
            var isVerified = HttpContext.Session.GetString("IsVerified");
            return User.HasClaim(c => c.Type == "IsVerified" && c.Value == "true") || isVerified == "true";
        }

        [HttpGet]
        public async Task<IActionResult> ReverseGeocode(decimal lat, decimal lng, CancellationToken ct)
        {
            var address = await _locationService.ReverseGeocodeAsync(lat, lng, ct);
            return Json(new { address });
        }

        /////////////////////////// Dashboard ////////////////////////////////////

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (!IsFactoryUser())
                return RedirectToAction("Index", "Home");

            var factoryId = GetCurrentUserId();

            if (!IsVerifiedFactory())
            {
                TempData["Warning"] = "Your factory account needs verification to access the store. Please complete your verification.";
                return RedirectToAction("Index", "Home");
            }

            ViewBag.UserLoggedIn = true;

            var userId = HttpContext.Session.GetInt32("UserID");
            if (userId.HasValue)
            {
                // الأفضل Include عشان UserType
                var user = await _db.Users
                    .Include(u => u.UserType)
                    .FirstOrDefaultAsync(u => u.UserID == userId.Value);

                if (user != null)
                {
                    ViewBag.Email = _dataCiphers.Decrypt(user.Email!);
                    ViewBag.Type = user.UserType?.TypeName ?? GetCurrentUserTypeName();
//                    ViewBag.UserType = user.UserType?.TypeName ?? GetCurrentUserTypeName(); // ✅ نفس الاسم اللي في الـ View
                    ViewBag.UserName = user.FullName ?? "";
                    ViewBag.IsVerified = user.Verified;
                    ViewBag.NeedsVerification = !user.Verified;

                    if (!user.Verified)
                    {
                        TempData["VerificationMessage"] = $"Welcome {user.FullName}! Please go to settings to verification your account.";
                        TempData["ShowVerificationAlert"] = "true";
                    }
                }
            }
            else
            {
                ViewBag.UserLoggedIn = false;
            }

            var dashboardData = await _storeService.GetDashboardStatsAsync(factoryId);
            return View(dashboardData);
        }

        /////////////////////////// Material Management ///////////////////////////
        [HttpGet]
        public async Task<IActionResult> Materials()
        {
            if (!IsFactoryUser() || !IsVerifiedFactory())
                return RedirectToAction("Index", "Home");

            ViewBag.UserLoggedIn = true;

            var factoryId = GetCurrentUserId();
            if (factoryId == 0)
            {
                ViewBag.UserLoggedIn = false;
                return RedirectToAction("Index", "Home");
            }

            // ✅ الأفضل Include عشان تجيب UserType.TypeName بدون Query تاني
            var user = await _db.Users
                .Include(u => u.UserType)
                .FirstOrDefaultAsync(u => u.UserID == factoryId);

            if (user != null)
            {
                ViewBag.Email = _dataCiphers.Decrypt(user.Email!);

                // ✅ توحيد اسم الـ ViewBag اللي هنستخدمه في الـ View
                ViewBag.Type = user.UserType?.TypeName ?? GetCurrentUserTypeName();
                ViewBag.UserName = user.FullName ?? "";
                ViewBag.IsVerified = user.Verified;
                ViewBag.NeedsVerification = !user.Verified;

                if (!user.Verified)
                {
                    TempData["VerificationMessage"] =
                        $"Welcome {user.FullName}! Please go to settings to verification your account.";
                    TempData["ShowVerificationAlert"] = "true";
                }
            }
            else
            {
                ViewBag.UserLoggedIn = false;
            }

            var materials = await _storeService.GetMaterialsAsync(factoryId);
            return View("Material/Materials", materials);
        }

        [HttpGet]
        public IActionResult AddMaterial()
        {
            if (!IsFactoryUser() || !IsVerifiedFactory())
                return RedirectToAction("Index", "Home");

            return View("Material/AddMaterial");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMaterial(MaterialModel newMaterial)
        {
            if (!IsFactoryUser() || !IsVerifiedFactory())
                return RedirectToAction("Index", "Home");

            if (!ModelState.IsValid || newMaterial.Quantity < newMaterial.MinOrderQuantity)
                return View("Material/AddMaterial", newMaterial);

            var factoryId = GetCurrentUserId();

            if (!await _storeService.CanFactoryAddProductAsync(factoryId))
            {
                TempData["Error"] = "Your factory account is not authorized to add products.";
                return RedirectToAction("Materials");
            }

            var success = await _storeService.AddMaterialAsync(newMaterial, factoryId);

            if (success)
            {
                TempData["Success"] = "Material added successfully!";
                return RedirectToAction("Materials");
            }

            TempData["Error"] = "Failed to add material. Please try again.";
            return View("Material/AddMaterial", newMaterial);
        }

        [HttpGet]
        public async Task<IActionResult> EditMaterial(int id)
        {
            if (!IsFactoryUser() || !IsVerifiedFactory())
                return RedirectToAction("Index", "Home");

            var factoryId = GetCurrentUserId();
            var material = await _storeService.GetMaterialByIdAsync(id, factoryId);

            if (material == null)
            {
                TempData["Error"] = "Material not found or you don't have permission to edit it.";
                return RedirectToAction("Materials");
            }

            ViewBag.UserLoggedIn = true;

            var userId = HttpContext.Session.GetInt32("UserID");
            if (userId.HasValue)
            {
                // الأفضل Include عشان UserType
                var user = await _db.Users
                    .Include(u => u.UserType)
                    .FirstOrDefaultAsync(u => u.UserID == userId.Value);

                if (user != null)
                {
                    ViewBag.Email = _dataCiphers.Decrypt(user.Email!);
                    ViewBag.UserType = user.UserType?.TypeName ?? GetCurrentUserTypeName(); // ✅ نفس الاسم اللي في الـ View
                    ViewBag.UserName = user.FullName ?? "";
                    ViewBag.IsVerified = user.Verified;
                    ViewBag.NeedsVerification = !user.Verified;

                    if (!user.Verified)
                    {
                        TempData["VerificationMessage"] = $"Welcome {user.FullName}! Please go to settings to verification your account.";
                        TempData["ShowVerificationAlert"] = "true";
                    }
                }
            }
            else
            {
                ViewBag.UserLoggedIn = false;
            }

            // Can modify?
            if (!await _storeService.CanFactoryModifyProductAsync(id, factoryId, "Material"))
            {
                TempData["Warning"] = "This material has active orders. Some fields cannot be modified.";
                ViewBag.CanModify = false;
            }
            else
            {
                ViewBag.CanModify = true;
            }

            return View("Material/EditMaterial", material);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditMaterial(int id, MaterialModel editMaterial)
        {
            if (!IsFactoryUser() || !IsVerifiedFactory())
                return RedirectToAction("Index", "Home");

            if (!ModelState.IsValid)
                return View("Material/EditMaterial", editMaterial);

            var factoryId = GetCurrentUserId();
            var success = await _storeService.UpdateMaterialAsync(id, editMaterial, factoryId);

            if (success)
            {
                TempData["Success"] = "Material updated successfully!";
                return RedirectToAction("Materials");
            }

            TempData["Error"] = "Failed to update material. Please try again.";
            return View("Material/EditMaterial", editMaterial);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMaterial(DeleteProductModel deleteMaterial)
        {
            if (!IsFactoryUser() || !IsVerifiedFactory())
            {
                TempData["Error"] = "Access denied";
                return RedirectToAction(nameof(Materials));
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .Where(e => !string.IsNullOrWhiteSpace(e))
                    .ToList();

                TempData["Error"] = errors.Any()
                    ? string.Join(" | ", errors)
                    : "Invalid delete request.";

                return RedirectToAction(nameof(Materials));
            }

            var factoryId = GetCurrentUserId();
            var result = await _storeService.DeleteMaterialAsync(deleteMaterial.Id, factoryId);

            TempData[result.Success ? "Success" : "Error"] = result.Message;

            return RedirectToAction(nameof(Materials));
        }

        /////////////////////////// Machines Management ///////////////////////////

        [HttpGet]
        public async Task<IActionResult> Machines()
        {
            if (!IsFactoryUser() || !IsVerifiedFactory())
                return RedirectToAction("Index", "Home");

            ViewBag.UserLoggedIn = true;

            var factoryId = GetCurrentUserId();
            if (factoryId == 0)
            {
                ViewBag.UserLoggedIn = false;
                return RedirectToAction("Index", "Home");
            }

            // ✅ نفس ستايل Materials: Include UserType + ViewBags موحدة
            var user = await _db.Users
                .Include(u => u.UserType)
                .FirstOrDefaultAsync(u => u.UserID == factoryId);

            if (user != null)
            {
                ViewBag.Email = _dataCiphers.Decrypt(user.Email!);
                ViewBag.Type = user.UserType?.TypeName ?? GetCurrentUserTypeName();
                ViewBag.UserName = user.FullName ?? "";
                ViewBag.IsVerified = user.Verified;
                ViewBag.NeedsVerification = !user.Verified;

                if (!user.Verified)
                {
                    TempData["VerificationMessage"] =
                        $"Welcome {user.FullName}! Please go to settings to verification your account.";
                    TempData["ShowVerificationAlert"] = "true";
                }
            }
            else
            {
                ViewBag.UserLoggedIn = false;
            }

            var machines = await _storeService.GetMachinesAsync(factoryId);
            return View("Machine/Machines", machines);
        }

        [HttpGet]
        public IActionResult AddMachine()
        {
            if (!IsFactoryUser() || !IsVerifiedFactory())
                return RedirectToAction("Index", "Home");

            return View("Machine/AddMachine", new MachineModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMachine(MachineModel newMachine)
        {
            if (!IsFactoryUser() || !IsVerifiedFactory())
                return RedirectToAction("Index", "Home");

            // ✅ Manual validation (لو عايزها هنا كمان) — لكن عندنا IValidatableObject في VM فمش لازم
            if (newMachine.MinOrderQuantity.HasValue && newMachine.MinOrderQuantity.Value > newMachine.Quantity)
            {
                ModelState.AddModelError(nameof(newMachine.MinOrderQuantity),
                    "Minimum order quantity cannot be greater than available quantity.");
            }

            if (!ModelState.IsValid)
            {
                // ✅ يظهر للمستخدم في ValidationSummary + تحت الحقول
                return View("Machine/AddMachine", newMachine);
            }

            var factoryId = GetCurrentUserId();

            if (!await _storeService.CanFactoryAddProductAsync(factoryId))
            {
                TempData["Error"] = "Your factory account is not authorized to add products.";
                return RedirectToAction(nameof(Machines));
            }

            try
            {
                var success = await _storeService.AddMachineAsync(newMachine, factoryId);

                if (success)
                {
                    TempData["Success"] = "Machine added successfully!";
                    return RedirectToAction(nameof(Machines));
                }

                // ✅ ده خطأ عام من السيرفيس
                ModelState.AddModelError(string.Empty, "Failed to add machine. Please try again.");
                return View("Machine/AddMachine", newMachine);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding machine");
                ModelState.AddModelError(string.Empty, "Unexpected error occurred while adding the machine.");
                return View("Machine/AddMachine", newMachine);
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditMachine(int id)
        {
            if (!IsFactoryUser() || !IsVerifiedFactory())
                return RedirectToAction("Index", "Home");

            var factoryId = GetCurrentUserId();
            var machine = await _storeService.GetMachineByIdAsync(id, factoryId);

            if (machine == null)
            {
                TempData["Error"] = "Machine not found or you don't have permission to edit it.";
                return RedirectToAction(nameof(Machines));
            }

            ViewBag.UserLoggedIn = true;

            var userId = HttpContext.Session.GetInt32("UserID");
            if (userId.HasValue)
            {
                var user = await _db.Users
                    .Include(u => u.UserType)
                    .FirstOrDefaultAsync(u => u.UserID == userId.Value);

                if (user != null)
                {
                    ViewBag.Email = _dataCiphers.Decrypt(user.Email!);
                    ViewBag.UserType = user.UserType?.TypeName ?? GetCurrentUserTypeName();
                    ViewBag.UserName = user.FullName ?? "";
                    ViewBag.IsVerified = user.Verified;
                    ViewBag.NeedsVerification = !user.Verified;

                    if (!user.Verified)
                    {
                        TempData["VerificationMessage"] =
                            $"Welcome {user.FullName}! Please go to settings to verification your account.";
                        TempData["ShowVerificationAlert"] = "true";
                    }
                }
            }
            else
            {
                ViewBag.UserLoggedIn = false;
            }

            // ✅ نفس فكرة Material: منع تعديل بعض الحقول لو فيه Orders
            if (!await _storeService.CanFactoryModifyProductAsync(id, factoryId, "Machine"))
            {
                TempData["Warning"] = "This machine has active orders. Some fields cannot be modified.";
                ViewBag.CanModify = false;
            }
            else
            {
                ViewBag.CanModify = true;
            }

            return View("Machine/EditMachine", machine);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditMachine(int id, MachineModel editMachine)
        {
            if (!IsFactoryUser() || !IsVerifiedFactory())
                return RedirectToAction("AccessDenied", "Home");

            if (!ModelState.IsValid)
                return View("Machine/EditMachine", editMachine);

            var factoryId = GetCurrentUserId();
            var success = await _storeService.UpdateMachineAsync(id, editMachine, factoryId);

            if (success)
            {
                TempData["Success"] = "Machine updated successfully!";
                return RedirectToAction(nameof(Machines));
            }

            TempData["Error"] = "Failed to update machine. Please try again.";
            return View("Machine/EditMachine", editMachine);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMachine(DeleteProductModel deleteMaterial)
        {
            if (!IsFactoryUser() || !IsVerifiedFactory())
            {
                TempData["Error"] = "Access denied";
                return RedirectToAction(nameof(Machines));
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .Where(e => !string.IsNullOrWhiteSpace(e))
                    .ToList();

                TempData["Error"] = errors.Any()
                    ? string.Join(" | ", errors)
                    : "Invalid delete request.";

                return RedirectToAction(nameof(Machines));
            }

            var factoryId = GetCurrentUserId();

            // ✅ الأفضل: خليه ServiceResult زي Materials (لو انت عدلت الـService)
            // لو لسه DeleteMachineAsync بيرجع bool، هنخليه TempData بناءً عليه:
            var success = await _storeService.DeleteMachineAsync(deleteMaterial.Id, factoryId);

            TempData[success.Success ? "Success" : "Error"] = success.Message;
                //? "Machine deleted successfully!"
                //: "Failed to delete machine.";

            return RedirectToAction(nameof(Machines));
        }

        /////////////////////////// Rentals Management ///////////////////////////

        [HttpGet]
        public async Task<IActionResult> Rentals()
        {
            if (!IsFactoryUser() || !IsVerifiedFactory())
                return RedirectToAction("Index", "Home");

            ViewBag.UserLoggedIn = true;

            var factoryId = GetCurrentUserId();
            if (factoryId == 0)
            {
                ViewBag.UserLoggedIn = false;
                return RedirectToAction("Index", "Home");
            }

            var user = await _db.Users
                .Include(u => u.UserType)
                .FirstOrDefaultAsync(u => u.UserID == factoryId);

            if (user != null)
            {
                ViewBag.Email = _dataCiphers.Decrypt(user.Email!);
                ViewBag.Type = user.UserType?.TypeName ?? GetCurrentUserTypeName();
                ViewBag.UserName = user.FullName ?? "";
                ViewBag.IsVerified = user.Verified;
                ViewBag.NeedsVerification = !user.Verified;

                if (!user.Verified)
                {
                    TempData["VerificationMessage"] =
                        $"Welcome {user.FullName}! Please go to settings to verification your account.";
                    TempData["ShowVerificationAlert"] = "true";
                }
            }
            else
            {
                ViewBag.UserLoggedIn = false;
            }

            var rentals = await _storeService.GetRentalsAsync(factoryId);
            return View("Rental/Rentals", rentals);
        }

        [HttpGet]
        public IActionResult AddRental()
        {
            if (!IsFactoryUser() || !IsVerifiedFactory())
                return RedirectToAction("Index", "Home");

            // ✅ default
            return View("Rental/AddRental", new RentalModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddRental(RentalModel newRental, CancellationToken ct)
        {
            if (!IsFactoryUser() || !IsVerifiedFactory())
                return RedirectToAction("Index", "Home");

            if (!ModelState.IsValid)
                return View(newRental);

            var factoryId = GetCurrentUserId();

            if (!await _storeService.CanFactoryAddProductAsync(factoryId))
            {
                TempData["Error"] = "Your factory account is not authorized to add products.";
                return RedirectToAction(nameof(Rentals));
            }

            try
            {
                var result = await _storeService.AddRentalAsync(newRental, factoryId, ct);
                if (result.Success)
                {
                    TempData["Success"] = result.Message;
                    return RedirectToAction(nameof(Rentals));
                }

                ModelState.AddModelError(string.Empty, result.Message);
                return View("Rental/AddRental", newRental);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding rental");
                ModelState.AddModelError(string.Empty, "Unexpected error occurred while adding the rental property.");
                return View("Rental/AddRental", newRental);
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditRental(int id)
        {
            if (!IsFactoryUser() || !IsVerifiedFactory())
                return RedirectToAction("AccessDenied", "Home");

            var factoryId = GetCurrentUserId();
            var rental = await _storeService.GetRentalByIdAsync(id, factoryId);

            if (rental == null)
            {
                TempData["Error"] = "Rental property not found or you don't have permission to edit it.";
                return RedirectToAction(nameof(Rentals));
            }

            ViewBag.UserLoggedIn = true;

            var userId = HttpContext.Session.GetInt32("UserID");
            if (userId.HasValue)
            {
                var user = await _db.Users
                    .Include(u => u.UserType)
                    .FirstOrDefaultAsync(u => u.UserID == userId.Value);

                if (user != null)
                {
                    ViewBag.Email = _dataCiphers.Decrypt(user.Email!);
                    ViewBag.UserType = user.UserType?.TypeName ?? GetCurrentUserTypeName();
                    ViewBag.UserName = user.FullName ?? "";
                    ViewBag.IsVerified = user.Verified;
                    ViewBag.NeedsVerification = !user.Verified;

                    if (!user.Verified)
                    {
                        TempData["VerificationMessage"] =
                            $"Welcome {user.FullName}! Please go to settings to verification your account.";
                        TempData["ShowVerificationAlert"] = "true";
                    }
                }
            }
            else
            {
                ViewBag.UserLoggedIn = false;
            }

            if (!await _storeService.CanFactoryModifyProductAsync(id, factoryId, "Rental"))
            {
                TempData["Warning"] = "This rental has active orders. Some fields cannot be modified.";
                ViewBag.CanModify = false;
            }
            else
            {
                ViewBag.CanModify = true;
            }

            return View("Rental/EditRental", rental);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRental(int id, RentalModel editRental, CancellationToken ct)
        {
            if (!IsFactoryUser() || !IsVerifiedFactory())
                return RedirectToAction("AccessDenied", "Home");

            if (!ModelState.IsValid)
                return View("Rental/EditRental", editRental);

            var factoryId = GetCurrentUserId();

            try
            {
                var result = await _storeService.UpdateRentalAsync(id, editRental, factoryId, ct);
                if (result.Success)
                {
                    TempData["Success"] = result.Message;
                    return RedirectToAction(nameof(Rentals));
                }

                ModelState.AddModelError(string.Empty, result.Message);
                return View("Rental/EditRental", editRental);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating rental");
                ModelState.AddModelError(string.Empty, "Unexpected error occurred while updating the rental property.");
                return View("Rental/EditRental", editRental);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRental(DeleteProductModel deleteRental)
        {
            if (!IsFactoryUser() || !IsVerifiedFactory())
            {
                TempData["Error"] = "Access denied";
                return RedirectToAction(nameof(Rentals));
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .Where(e => !string.IsNullOrWhiteSpace(e))
                    .ToList();

                TempData["Error"] = errors.Any()
                    ? string.Join(" | ", errors)
                    : "Invalid delete request.";

                return RedirectToAction(nameof(Rentals));
            }

            var factoryId = GetCurrentUserId();

            try
            {
                var result = await _storeService.DeleteRentalAsync(deleteRental.Id, factoryId);
                TempData[result.Success ? "Success" : "Error"] = result.Message;
                return RedirectToAction(nameof(Rentals));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting rental");
                TempData["Error"] = "An error occurred while deleting the rental property.";
                return RedirectToAction(nameof(Rentals));
            }
        }

        /////////////////////////// Auctions Management ///////////////////////////

        [HttpGet]
        public async Task<IActionResult> Auctions()
        {
            if (!IsFactoryUser() || !IsVerifiedFactory())
                return RedirectToAction("Index", "Home");

            ViewBag.UserLoggedIn = true;

            var factoryId = GetCurrentUserId();
            if (factoryId == 0)
            {
                ViewBag.UserLoggedIn = false;
                return RedirectToAction("Index", "Home");
            }

            // ✅ نفس ستايل Materials/Machines
            var user = await _db.Users
                .Include(u => u.UserType)
                .FirstOrDefaultAsync(u => u.UserID == factoryId);

            if (user != null)
            {
                ViewBag.Email = _dataCiphers.Decrypt(user.Email!);
                ViewBag.Type = user.UserType?.TypeName ?? GetCurrentUserTypeName();
                ViewBag.UserName = user.FullName ?? "";
                ViewBag.IsVerified = user.Verified;
                ViewBag.NeedsVerification = !user.Verified;

                if (!user.Verified)
                {
                    TempData["VerificationMessage"] =
                        $"Welcome {user.FullName}! Please go to settings to verification your account.";
                    TempData["ShowVerificationAlert"] = "true";
                }
            }
            else
            {
                ViewBag.UserLoggedIn = false;
            }

            var auctions = await _storeService.GetAuctionsAsync(factoryId);
            return View("Auction/Auctions", auctions);
        }

        [HttpGet]
        public IActionResult AddAuction()
        {
            if (!IsFactoryUser() || !IsVerifiedFactory())
                return RedirectToAction("Index", "Home");

            return View("Auction/AddAuction", new AuctionModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddAuction(AuctionModel newAuction)
        {
            if (!IsFactoryUser() || !IsVerifiedFactory())
                return RedirectToAction("Index", "Home");

            if (!ModelState.IsValid)
                return View("Auction/AddAuction", newAuction);

            var factoryId = GetCurrentUserId();

            if (!await _storeService.CanFactoryAddProductAsync(factoryId))
            {
                TempData["Error"] = "Your factory account is not authorized to add products.";
                return RedirectToAction(nameof(Auctions));
            }

            try
            {
                var success = await _storeService.AddAuctionAsync(newAuction, factoryId);

                if (success)
                {
                    TempData["Success"] = "Auction added successfully!";
                    return RedirectToAction(nameof(Auctions));
                }

                ModelState.AddModelError(string.Empty, "Failed to add auction. Please try again.");
                return View("Auction/AddAuction", newAuction);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding auction");
                ModelState.AddModelError(string.Empty, "Unexpected error occurred while adding the auction.");
                return View("Auction/AddAuction", newAuction);
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditAuction(int id)
        {
            if (!IsFactoryUser() || !IsVerifiedFactory())
                return RedirectToAction("AccessDenied", "Home");

            var factoryId = GetCurrentUserId();
            var auction = await _storeService.GetAuctionByIdAsync(id, factoryId);

            if (auction == null)
            {
                TempData["Error"] = "Auction not found or you don't have permission to edit it.";
                return RedirectToAction(nameof(Auctions));
            }

            ViewBag.UserLoggedIn = true;

            var userId = HttpContext.Session.GetInt32("UserID");
            if (userId.HasValue)
            {
                var user = await _db.Users
                    .Include(u => u.UserType)
                    .FirstOrDefaultAsync(u => u.UserID == userId.Value);

                if (user != null)
                {
                    ViewBag.Email = _dataCiphers.Decrypt(user.Email!);
                    ViewBag.UserType = user.UserType?.TypeName ?? GetCurrentUserTypeName();
                    ViewBag.UserName = user.FullName ?? "";
                    ViewBag.IsVerified = user.Verified;
                    ViewBag.NeedsVerification = !user.Verified;

                    if (!user.Verified)
                    {
                        TempData["VerificationMessage"] =
                            $"Welcome {user.FullName}! Please go to settings to verification your account.";
                        TempData["ShowVerificationAlert"] = "true";
                    }
                }
            }
            else
            {
                ViewBag.UserLoggedIn = false;
            }

            // ✅ نفس فكرة Material/Machine: منع تعديل بعض الحقول لو فيه Orders
            if (!await _storeService.CanFactoryModifyProductAsync(id, factoryId, "Auction"))
            {
                TempData["Warning"] = "This auction has active orders. Some fields cannot be modified.";
                ViewBag.CanModify = false;
            }
            else
            {
                ViewBag.CanModify = true;
            }

            return View("Auction/EditAuction", auction);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAuction(int id, AuctionModel editAuction)
        {
            if (!IsFactoryUser() || !IsVerifiedFactory())
                return RedirectToAction("Index", "Home");

            if (!ModelState.IsValid)
                return View("Auction/EditAuction", editAuction);

            var factoryId = GetCurrentUserId();

            try
            {
                var success = await _storeService.UpdateAuctionAsync(id, editAuction, factoryId);

                if (success)
                {
                    TempData["Success"] = "Auction updated successfully!";
                    return RedirectToAction(nameof(Auctions));
                }

                TempData["Error"] = "Failed to update auction. Please try again.";
                return View("Auction/EditAuction", editAuction);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating auction");
                ModelState.AddModelError(string.Empty, "Unexpected error occurred while updating the auction.");
                return View("Auction/EditAuction", editAuction);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAuction(DeleteProductModel deleteAuction)
        {
            if (!IsFactoryUser() || !IsVerifiedFactory())
            {
                TempData["Error"] = "Access denied";
                return RedirectToAction(nameof(Auctions));
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .Where(e => !string.IsNullOrWhiteSpace(e))
                    .ToList();

                TempData["Error"] = errors.Any()
                    ? string.Join(" | ", errors)
                    : "Invalid delete request.";

                return RedirectToAction(nameof(Auctions));
            }

            var factoryId = GetCurrentUserId();

            try
            {
                // ✅ نفس فكرة Machine (لو عندك DeleteAuctionAsync بيرجع ServiceResult)
                // لو بيرجع bool بس: هنعدّل تحت
                var result = await _storeService.DeleteAuctionAsync(deleteAuction.Id, factoryId);

                TempData[result.Success ? "Success" : "Error"] = result.Message;
                return RedirectToAction(nameof(Auctions));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting auction");
                TempData["Error"] = "An error occurred while deleting the auction.";
                return RedirectToAction(nameof(Auctions));
            }
        }

        /////////////////////////// Jobs Management ///////////////////////////

        [HttpGet]
        public async Task<IActionResult> Jobs()
        {
            if (!IsFactoryUser() || !IsVerifiedFactory())
                return RedirectToAction("Index", "Home");

            ViewBag.UserLoggedIn = true;

            var factoryId = GetCurrentUserId();
            if (factoryId == 0)
            {
                ViewBag.UserLoggedIn = false;
                return RedirectToAction("Index", "Home");
            }

            // ✅ نفس ستايل Materials/Machines/Rentals
            var user = await _db.Users
                .Include(u => u.UserType)
                .FirstOrDefaultAsync(u => u.UserID == factoryId);

            if (user != null)
            {
                ViewBag.Email = _dataCiphers.Decrypt(user.Email!);
                ViewBag.Type = user.UserType?.TypeName ?? GetCurrentUserTypeName();
                ViewBag.UserName = user.FullName ?? "";
                ViewBag.IsVerified = user.Verified;
                ViewBag.NeedsVerification = !user.Verified;

                if (!user.Verified)
                {
                    TempData["VerificationMessage"] =
                        $"Welcome {user.FullName}! Please go to settings to verification your account.";
                    TempData["ShowVerificationAlert"] = "true";
                }
            }
            else
            {
                ViewBag.UserLoggedIn = false;
            }

            var jobs = await _storeService.GetJobsAsync(factoryId);
            return View("Job/Jobs" , jobs);
        }

        [HttpGet]
        public async Task<IActionResult> AddJob()
        {
            if (!IsFactoryUser() || !IsVerifiedFactory())
                return RedirectToAction("Index", "Home");

            ViewBag.UserLoggedIn = true;

            var factoryId = GetCurrentUserId();
            if (factoryId == 0)
            {
                ViewBag.UserLoggedIn = false;
                return RedirectToAction("Index", "Home");
            }

            var user = await _db.Users
                .Include(u => u.UserType)
                .FirstOrDefaultAsync(u => u.UserID == factoryId);

            if (user != null)
            {
                ViewBag.Email = _dataCiphers.Decrypt(user.Email!);
                ViewBag.Type = user.UserType?.TypeName ?? GetCurrentUserTypeName();
                ViewBag.UserName = user.FullName ?? "";
                ViewBag.IsVerified = user.Verified;
                ViewBag.NeedsVerification = !user.Verified;

                if (!user.Verified)
                {
                    TempData["VerificationMessage"] =
                        $"Welcome {user.FullName}! Please go to settings to verification your account.";
                    TempData["ShowVerificationAlert"] = "true";
                }
            }
            else
            {
                ViewBag.UserLoggedIn = false;
            }

            // ✅ default زي AddMachine/AddRental
            return View("Job/AddJob", new JobModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddJob(JobModel newJob, CancellationToken ct)
        {
            if (!IsFactoryUser() || !IsVerifiedFactory())
                return RedirectToAction("Index", "Home");

            if (!ModelState.IsValid)
                return View(newJob);

            var factoryId = GetCurrentUserId();

            if (!await _storeService.CanFactoryAddProductAsync(factoryId))
            {
                TempData["Error"] = "Your factory account is not authorized to add products.";
                return RedirectToAction(nameof(Jobs));
            }

            try
            {
                // ✅ ServiceResult زي Rentals
                var result = await _storeService.AddJobAsync(newJob, factoryId, ct);

                if (result.Success)
                {
                    TempData["Success"] = result.Message;
                    return RedirectToAction(nameof(Jobs));
                }

                ModelState.AddModelError(string.Empty, result.Message);
                return View("Job/AddJob", newJob);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding job");
                ModelState.AddModelError(string.Empty, "Unexpected error occurred while adding the job posting.");
                return View("Job/AddJob", newJob);
            }
        }
 
        [HttpGet]
        public async Task<IActionResult> EditJob(int id)
        {
            if (!IsFactoryUser() || !IsVerifiedFactory())
                return RedirectToAction("Index", "Home");

            var factoryId = GetCurrentUserId();
            var job = await _storeService.GetJobByIdAsync(id, factoryId);

            if (job == null)
            {
                TempData["Error"] = "Job posting not found or you don't have permission to edit it.";
                return RedirectToAction(nameof(Jobs));
            }

            ViewBag.UserLoggedIn = true;

            var userId = HttpContext.Session.GetInt32("UserID");
            if (userId.HasValue)
            {
                var user = await _db.Users
                    .Include(u => u.UserType)
                    .FirstOrDefaultAsync(u => u.UserID == userId.Value);

                if (user != null)
                {
                    ViewBag.Email = _dataCiphers.Decrypt(user.Email!);
                    ViewBag.UserType = user.UserType?.TypeName ?? GetCurrentUserTypeName();
                    ViewBag.UserName = user.FullName ?? "";
                    ViewBag.IsVerified = user.Verified;
                    ViewBag.NeedsVerification = !user.Verified;

                    if (!user.Verified)
                    {
                        TempData["VerificationMessage"] =
                            $"Welcome {user.FullName}! Please go to settings to verification your account.";
                        TempData["ShowVerificationAlert"] = "true";
                    }
                }
            }
            else
            {
                ViewBag.UserLoggedIn = false;
            }

            // ✅ Lock editing if has any orders
            var hasOrders = await _db.JobOrders.AsNoTracking().AnyAsync(o => o.JobStoreID == id);
            ViewBag.CanModify = !hasOrders;

            job.Id = id;
            return View("Job/EditJob", job);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditJob(int id, JobModel editJob, CancellationToken ct)
        {
            if (!IsFactoryUser() || !IsVerifiedFactory())
                return RedirectToAction("Index", "Home");

            // ✅ prevent update if job has orders
            var hasOrders = await _db.JobOrders.AsNoTracking().AnyAsync(o => o.JobStoreID == id, ct);
            if (hasOrders)
            {
                TempData["Error"] = "You cannot edit this job because it already has orders.";
                return RedirectToAction(nameof(EditJob), new { id });
            }

            if (!ModelState.IsValid)
                return View("Job/EditJob", editJob);

            var factoryId = GetCurrentUserId();

            try
            {
                // ✅ uses your service update (already modified from previous message)
                var result = await _storeService.UpdateJobAsync(id, editJob, factoryId, ct);

                if (result.Success)
                {
                    TempData["Success"] = result.Message;
                    return RedirectToAction(nameof(Jobs));
                }

                ModelState.AddModelError(string.Empty, result.Message);
                return View("Job/EditJob", editJob);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating job");
                ModelState.AddModelError(string.Empty, "Unexpected error occurred while updating the job posting.");
                return View("Job/EditJob", editJob);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateJobOrderStatus(int jobId, int jobOrderId, string status, CancellationToken ct)
        {
            if (!IsFactoryUser() || !IsVerifiedFactory())
                return RedirectToAction("Index", "Home");

            var factoryId = GetCurrentUserId();
            if (factoryId == 0) return RedirectToAction("Index", "Home");

            // ✅ تأكد job تبع المصنع
            var owns = await _db.JobStores.AsNoTracking()
                .AnyAsync(j => j.JobID == jobId && j.PostedBy == factoryId, ct);

            if (!owns)
            {
                TempData["Error"] = "Access denied.";
                return RedirectToAction(nameof(Jobs));
            }

            var order = await _db.JobOrders
                .FirstOrDefaultAsync(o => o.JobOrderID == jobOrderId && o.JobStoreID == jobId, ct);

            if (order == null)
            {
                TempData["Error"] = "Order not found.";
                return RedirectToAction(nameof(JobDetails), new { id = jobId });
            }

            if (!Enum.TryParse<EcoRecyclersGreenTech.Data.Orders.JobOrderStatus>(status, true, out var st))
            {
                TempData["Error"] = "Invalid status value.";
                return RedirectToAction(nameof(JobDetails), new { id = jobId });
            }

            order.Status = st;
            await _db.SaveChangesAsync(ct);

            TempData["Success"] = "Order status updated.";
            return RedirectToAction(nameof(JobDetails), new { id = jobId });
        }

        [HttpGet]
        public async Task<IActionResult> JobDetails(int id, CancellationToken ct)
        {
            if (!IsFactoryUser() || !IsVerifiedFactory())
                return RedirectToAction("Index", "Home");

            var factoryId = GetCurrentUserId();
            if (factoryId == 0) return RedirectToAction("Index", "Home");

            ViewBag.UserLoggedIn = true;

            var userId = HttpContext.Session.GetInt32("UserID");
            if (userId.HasValue)
            {
                var user = await _db.Users
                    .Include(u => u.UserType)
                    .FirstOrDefaultAsync(u => u.UserID == userId.Value);

                if (user != null)
                {
                    ViewBag.Email = _dataCiphers.Decrypt(user.Email!);
                    ViewBag.UserType = user.UserType?.TypeName ?? GetCurrentUserTypeName();
                    ViewBag.UserName = user.FullName ?? "";
                    ViewBag.IsVerified = user.Verified;
                    ViewBag.NeedsVerification = !user.Verified;

                    if (!user.Verified)
                    {
                        TempData["VerificationMessage"] =
                            $"Welcome {user.FullName}! Please go to settings to verification your account.";
                        TempData["ShowVerificationAlert"] = "true";
                    }
                }
            }
            else
            {
                ViewBag.UserLoggedIn = false;
            }

            // Job نفسه
            var job = await _storeService.GetJobByIdAsync(id, factoryId);
            if (job == null)
            {
                TempData["Error"] = "Job not found.";
                return RedirectToAction(nameof(Jobs));
            }

            // هل فيه Orders؟
            var hasOrders = await _db.JobOrders.AsNoTracking().AnyAsync(o => o.JobStoreID == id, ct);

            // Orders + Users
            var orders = await _storeService.GetJobOrdersForFactoryAsync(id, factoryId, ct);

            // Decrypt emails داخل controller (أفضل من view)
            foreach (var o in orders)
            {
                if (!string.IsNullOrWhiteSpace(o.Email))
                    o.Email = _dataCiphers.Decrypt(o.Email);
            }

            var vm = new JobDetailsModel
            {
                Job = job,
                CanEdit = !hasOrders,
                OrdersCount = orders.Count,
                Orders = orders
            };

            return View("Job/JobDetails", vm); // Views/Factory/JobDetails.cshtml
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteJob(DeleteProductModel deleteJob)
        {
            if (!IsFactoryUser() || !IsVerifiedFactory())
            {
                TempData["Error"] = "Access denied";
                return RedirectToAction(nameof(Jobs));
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .Where(e => !string.IsNullOrWhiteSpace(e))
                    .ToList();

                TempData["Error"] = errors.Any()
                    ? string.Join(" | ", errors)
                    : "Invalid delete request.";

                return RedirectToAction(nameof(Jobs));
            }

            var factoryId = GetCurrentUserId();

            try
            {
                // ✅ ServiceResult زي Rentals/Auctions
                var result = await _storeService.DeleteJobAsync(deleteJob.Id, factoryId);
                TempData[result.Success ? "Success" : "Error"] = result.Message;
                return RedirectToAction(nameof(Jobs));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting job");
                TempData["Error"] = "An error occurred while deleting the job posting.";
                return RedirectToAction(nameof(Jobs));
            }
        }

        // ==================== PUBLIC MARKETPLACE ====================
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Marketplace(string? type = null)
        {
            var filter = new SearchFilterModel
            {
                ProductType = type
            };

            ViewBag.ProductType = type;

            switch (type?.ToLower())
            {
                case "materials":
                    var materials = await _storeService.GetPublicMaterialsAsync(filter);
                    return View("Marketplace/Materials", materials);

                case "machines":
                    var machines = await _storeService.GetPublicMachinesAsync(filter);
                    return View("Marketplace/Machines", machines);

                case "rentals":
                    var rentals = await _storeService.GetPublicRentalsAsync(filter);
                    return View("Marketplace/Rentals", rentals);

                case "auctions":
                    var auctions = await _storeService.GetPublicAuctionsAsync(filter);
                    return View("Marketplace/Auctions", auctions);

                case "jobs":
                    var jobs = await _storeService.GetPublicJobsAsync(filter);
                    return View("Marketplace/Jobs", jobs);

                default:
                    var allProducts = new
                    {
                        Materials = await _storeService.GetPublicMaterialsAsync(),
                        Machines = await _storeService.GetPublicMachinesAsync(),
                        Rentals = await _storeService.GetPublicRentalsAsync(),
                        Auctions = await _storeService.GetPublicAuctionsAsync(),
                        Jobs = await _storeService.GetPublicJobsAsync()
                    };
                    return View("Marketplace/Index", allProducts);
            }
        }

        // ==================== STATUS CHANGE ====================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleProductStatus([FromBody] StatusChangeModel model)
        {
            if (!IsFactoryUser() || !IsVerifiedFactory())
                return Json(new { success = false, message = "Access denied" });

            var factoryId = GetCurrentUserId();
            var success = await _storeService.ToggleProductStatusAsync(model.Id, model.Status!, factoryId, model.ProductType!);

            return Json(new
            {
                success = success,
                message = success ? $"Status updated to {model.Status}" : "Failed to update status"
            });
        }

        // ==================== AJAX ENDPOINTS ====================
        [HttpGet]
        public async Task<IActionResult> GetDashboardStats()
        {
            if (!IsFactoryUser() || !IsVerifiedFactory())
                return Json(new { error = "Access denied" });

            var factoryId = GetCurrentUserId();
            var stats = await _storeService.GetDashboardStatsAsync(factoryId);

            return Json(new
            {
                totalProducts = stats.TotalProducts,
                activeMaterials = stats.ActiveMaterials,
                activeMachines = stats.ActiveMachines,
                activeRentals = stats.ActiveRentals,
                activeAuctions = stats.ActiveAuctions,
                activeJobs = stats.ActiveJobs,
                pendingOrders = stats.PendingOrders
            });
        }
    }
}