using EcoRecyclersGreenTech.Data.Orders;
using EcoRecyclersGreenTech.Data.Users;
using EcoRecyclersGreenTech.Models;
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

        #region Helpers

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

            // session stores TypeName as string
            return HttpContext.Session.GetString("UserTypeName") ?? string.Empty;
        }

        private string GetCurrentUserTypeName()
        {
            // claims first
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (!string.IsNullOrWhiteSpace(role))
                return role;

            var typeName = User.FindFirst("UserType")?.Value;
            if (!string.IsNullOrWhiteSpace(typeName))
                return typeName;

            // session fallback
            return HttpContext.Session.GetString("UserTypeName") ?? string.Empty;
        }

        private bool IsFactoryUser() => GetCurrentUserType() == "Factory";

        private bool IsVerifiedFactory()
        {
            var isVerified = HttpContext.Session.GetString("IsVerified");
            return User.HasClaim(c => c.Type == "IsVerified" && c.Value == "true") || isVerified == "true";
        }

        private string? DecryptOrRaw(string? v)
        {
            if (string.IsNullOrWhiteSpace(v)) return null;
            try { return _dataCiphers.Decrypt(v); }
            catch { return v; }
        }

        private async Task<bool> PopulateUserViewBagsAsync(int userId, CancellationToken ct = default)
        {
            var user = await _db.Users
                .Include(u => u.UserType)
                .Include(u => u.FactoryProfile)
                .Include(u => u.Wallet)
                .FirstOrDefaultAsync(u => u.UserId == userId, ct);

            if (user == null)
            {
                ViewBag.UserLoggedIn = false;
                return false;
            }

            ViewBag.UserLoggedIn = true;

            // Email
            ViewBag.Email = DecryptOrRaw(user.Email) ?? user.Email ?? "";
            ViewBag.Type = user.UserType?.TypeName ?? GetCurrentUserTypeName();
            ViewBag.UserName = user.FullName ?? "";
            ViewBag.IsVerified = user.Verified;
            ViewBag.NeedsVerification = !user.Verified;

            // Wallet
            var wallet = await _db.Wallets
                .AsNoTracking()
                .Where(w => w.UserId == user.UserId)
                .Select(w => new { w.Balance, w.ReservedBalance })
                .FirstOrDefaultAsync(ct);

            ViewBag.WalletBalance = wallet?.Balance ?? 0m;
            ViewBag.WalletReserved = wallet?.ReservedBalance ?? 0m;
            ViewBag.WalletCurrency = "EGP";

            if (!user.Verified)
            {
                TempData["VerificationMessage"] = $"Welcome {user.FullName}! Please go to settings to verification your account.";
                TempData["ShowVerificationAlert"] = "true";
            }

            return true;
        }

        private async Task PopulateFactoryLocationViewBagsAsync(int factoryId, CancellationToken ct = default)
        {
            var user = await _db.Users.AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserId == factoryId, ct);

            if (user == null)
            {
                ViewBag.FactoryAddress = null;
                ViewBag.FactoryLat = null;
                ViewBag.FactoryLng = null;
                ViewBag.FactoryHasLocation = false;
                return;
            }

            bool factoryHasLocation =
                !string.IsNullOrWhiteSpace(user.Address) ||
                (user.Latitude.HasValue && user.Longitude.HasValue);

            // keep the same ViewBag names you used in each view.
            ViewBag.FactoryAddress = user.Address;
            ViewBag.FactoryLat = user.Latitude;
            ViewBag.FactoryLng = user.Longitude;
            ViewBag.FactoryHasLocation = factoryHasLocation;
        }

        private async Task PopulateFactoryLocationViewBagsForAuctionAsync(int factoryId, CancellationToken ct = default)
        {
            // Auction Views were using FactoryLatitude/FactoryLongitude
            var user = await _db.Users.AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserId == factoryId, ct);

            if (user == null)
            {
                ViewBag.FactoryAddress = null;
                ViewBag.FactoryLatitude = null;
                ViewBag.FactoryLongitude = null;
                ViewBag.FactoryHasLocation = false;
                return;
            }

            bool factoryHasLocation =
                !string.IsNullOrWhiteSpace(user.Address) ||
                (user.Latitude.HasValue && user.Longitude.HasValue);

            ViewBag.FactoryAddress = user.Address;
            ViewBag.FactoryLatitude = user.Latitude;
            ViewBag.FactoryLongitude = user.Longitude;
            ViewBag.FactoryHasLocation = factoryHasLocation;
        }

        private bool ModelHasCustomLocation(string? address, decimal? lat, decimal? lng)
        {
            bool hasAddress = !string.IsNullOrWhiteSpace(address);
            bool hasCoords = lat.HasValue && lng.HasValue;
            return hasAddress || hasCoords;
        }

        private async Task<bool> FactoryHasLocationAsync(int factoryId, CancellationToken ct = default)
        {
            var user = await _db.Users.AsNoTracking()
                .Where(u => u.UserId == factoryId)
                .Select(u => new { u.Address, u.Latitude, u.Longitude })
                .FirstOrDefaultAsync(ct);

            if (user == null) return false;

            return !string.IsNullOrWhiteSpace(user.Address) ||
                   (user.Latitude.HasValue && user.Longitude.HasValue);
        }

        private IActionResult DenyToHome() => RedirectToAction("Index", "Home");

        #endregion
        [HttpGet]
        public async Task<IActionResult> ReverseGeocode(decimal lat, decimal lng, CancellationToken ct)
        {
            var address = await _locationService.ReverseGeocodeAsync(lat, lng, ct);
            return Json(new { address });
        }

        /////////////////////////// Dashboard ///////////////////////////

        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken ct)
        {
            if (!IsFactoryUser())
                return DenyToHome();

            var factoryId = GetCurrentUserId();

            if (!IsVerifiedFactory())
            {
                TempData["Warning"] = "Your factory account needs verification to access the store. Please complete your verification.";
                return DenyToHome();
            }

            // ViewBag based on Session user
            var sessionUserId = HttpContext.Session.GetInt32("UserID");
            if (sessionUserId.HasValue)
                await PopulateUserViewBagsAsync(sessionUserId.Value, ct);
            else
                ViewBag.UserLoggedIn = false;

            var dashboardData = await _storeService.GetDashboardStatsAsync(factoryId);
            return View(dashboardData);
        }

        /////////////////////////// Material Management ///////////////////////////

        [HttpGet]
        public async Task<IActionResult> Materials(CancellationToken ct)
        {
            if (!IsFactoryUser() || !IsVerifiedFactory())
                return DenyToHome();

            var factoryId = GetCurrentUserId();
            if (factoryId == 0)
            {
                ViewBag.UserLoggedIn = false;
                return DenyToHome();
            }

            // set viewbags
            await PopulateUserViewBagsAsync(factoryId, ct);

            var materials = await _storeService.GetMaterialsAsync(factoryId);
            return View("Material/Materials", materials);
        }

        [HttpGet]
        public async Task<IActionResult> AddMaterial(CancellationToken ct)
        {
            if (!IsFactoryUser() || !IsVerifiedFactory())
                return DenyToHome();

            var factoryId = GetCurrentUserId();
            await PopulateFactoryLocationViewBagsAsync(factoryId, ct);


            ViewBag.UserLoggedIn = await PopulateUserViewBagsAsync(factoryId, ct);

            // use factory location if exists
            var useFactoryLocation = (bool)(ViewBag.FactoryHasLocation ?? false);

            var model = new MaterialProductDetailsModel
            {
                UseFactoryLocation = useFactoryLocation
            };

            return View("Material/AddMaterial", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMaterial(MaterialProductDetailsModel newMaterial, CancellationToken ct)
        {
            if (!IsFactoryUser() || !IsVerifiedFactory())
                return DenyToHome();

            var factoryId = GetCurrentUserId();

            await PopulateFactoryLocationViewBagsAsync(factoryId, ct);
            bool factoryHasLocation = (bool)(ViewBag.FactoryHasLocation ?? false);

            // Min order validation (nullable)
            if (newMaterial.MinOrderQuantity.HasValue)
            {
                if (newMaterial.MinOrderQuantity.Value < 0)
                    ModelState.AddModelError(nameof(newMaterial.MinOrderQuantity), "Minimum order quantity cannot be negative.");

                if (newMaterial.MinOrderQuantity.Value > newMaterial.Quantity)
                    ModelState.AddModelError(nameof(newMaterial.MinOrderQuantity), "Minimum order quantity cannot be greater than available quantity.");
            }

            // Location validation
            if (newMaterial.UseFactoryLocation)
            {
                if (!factoryHasLocation)
                    ModelState.AddModelError(nameof(newMaterial.UseFactoryLocation),
                        "Factory location is not set. Please choose custom pickup location and enter it.");
            }
            else
            {
                if (!ModelHasCustomLocation(newMaterial.Address, newMaterial.Latitude, newMaterial.Longitude))
                    ModelState.AddModelError(nameof(newMaterial.Address), "Please provide pickup address or coordinates.");
            }

            if (!ModelState.IsValid)
                return View("Material/AddMaterial", newMaterial);

            if (!await _storeService.CanFactoryAddProductAsync(factoryId))
            {
                TempData["Error"] = "Your factory account is not authorized to add products.";
                return RedirectToAction(nameof(Materials));
            }

            var success = await _storeService.AddMaterialAsync(newMaterial, factoryId);

            if (success)
            {
                TempData["Success"] = "Material added successfully!";
                return RedirectToAction(nameof(Materials));
            }

            TempData["Error"] = "Failed to add material. Please try again.";
            return View("Material/AddMaterial", newMaterial);
        }

        [HttpGet]
        public async Task<IActionResult> EditMaterial(int id, CancellationToken ct)
        {
            if (!IsFactoryUser() || !IsVerifiedFactory())
                return DenyToHome();

            var factoryId = GetCurrentUserId();
            var material = await _storeService.GetMaterialByIdAsync(id, factoryId);

            if (material == null)
            {
                TempData["Error"] = "Material not found or you don't have permission to edit it.";
                return RedirectToAction(nameof(Materials));
            }

            await PopulateFactoryLocationViewBagsAsync(factoryId, ct);

            var hasAnyOrders = await _db.MaterialOrders.AsNoTracking()
                .AnyAsync(o => o.MaterialStoreID == id, ct);

            ViewBag.HasAnyOrders = hasAnyOrders;
            ViewBag.CanModify = !hasAnyOrders;
            ViewBag.CanEditPriceQty = true;
            ViewBag.OldQuantity = material.Quantity;

            return View("Material/EditMaterial", material);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditMaterial(int id, MaterialProductDetailsModel editMaterial, CancellationToken ct)
        {
            if (!IsFactoryUser() || !IsVerifiedFactory())
                return DenyToHome();

            var factoryId = GetCurrentUserId();

            await PopulateFactoryLocationViewBagsAsync(factoryId, ct);
            bool factoryHasLocation = (bool)(ViewBag.FactoryHasLocation ?? false);

            var hasAnyOrders = await _db.MaterialOrders.AsNoTracking()
                .AnyAsync(o => o.MaterialStoreID == id, ct);

            ViewBag.HasAnyOrders = hasAnyOrders;
            ViewBag.CanModify = !hasAnyOrders;
            ViewBag.CanEditPriceQty = true;

            // old qty from DB
            var oldQty = await _db.MaterialStores.AsNoTracking()
                .Where(m => m.MaterialID == id && m.SellerID == factoryId)
                .Select(m => (int?)m.Quantity)
                .FirstOrDefaultAsync(ct);

            if (!oldQty.HasValue)
            {
                TempData["Error"] = "Material not found or you don't have permission to edit it.";
                return RedirectToAction(nameof(Materials));
            }

            ViewBag.OldQuantity = oldQty.Value;

            // Location validation
            if (editMaterial.UseFactoryLocation && !factoryHasLocation)
                ModelState.AddModelError(nameof(editMaterial.UseFactoryLocation), "Factory location is not set. Please choose custom pickup location.");

            if (!editMaterial.UseFactoryLocation)
            {
                if (!ModelHasCustomLocation(editMaterial.Address, editMaterial.Latitude, editMaterial.Longitude))
                    ModelState.AddModelError(nameof(editMaterial.Address), "Please provide pickup address or coordinates.");
            }

            // Rules based on orders
            if (hasAnyOrders)
            {
                // remove locked fields only
                ModelState.Remove(nameof(editMaterial.ProductType));
                ModelState.Remove(nameof(editMaterial.Unit));
                ModelState.Remove(nameof(editMaterial.MinOrderQuantity));
                ModelState.Remove(nameof(editMaterial.Status));
                ModelState.Remove(nameof(editMaterial.CancelWindowDays));
                ModelState.Remove(nameof(editMaterial.DeliveryDays));

                if (editMaterial.Quantity <= oldQty.Value)
                    ModelState.AddModelError(nameof(editMaterial.Quantity),
                        $"With existing orders, quantity must be greater than current quantity ({oldQty.Value}).");

                if (editMaterial.Price <= 0)
                    ModelState.AddModelError(nameof(editMaterial.Price), "Price must be greater than 0.");
            }
            else
            {
                if (editMaterial.MinOrderQuantity.HasValue && editMaterial.MinOrderQuantity.Value > editMaterial.Quantity)
                    ModelState.AddModelError(nameof(editMaterial.MinOrderQuantity), "Minimum order quantity cannot be greater than available quantity.");
            }

            if (!ModelState.IsValid)
                return View("Material/EditMaterial", editMaterial);

            var success = await _storeService.UpdateMaterialAsync(id, editMaterial, factoryId);

            if (success)
            {
                TempData["Success"] = "Material updated successfully!";
                return RedirectToAction(nameof(Materials));
            }

            TempData["Error"] = "Failed to update material. Please try again.";
            return View("Material/EditMaterial", editMaterial);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeMaterialOrderStatus(int orderId, string status, CancellationToken ct)
        {
            if (!IsFactoryUser() || !IsVerifiedFactory())
            {
                TempData["Error"] = "Access denied.";
                return RedirectToAction(nameof(MaterialOrders));
            }

            var factoryId = GetCurrentUserId();
            if (factoryId == 0)
            {
                TempData["Error"] = "Invalid factory.";
                return RedirectToAction(nameof(MaterialOrders));
            }

            if (!Enum.TryParse<EnumsOrderStatus>(status, true, out var newStatus))
            {
                TempData["Error"] = "Invalid status value.";
                return RedirectToAction(nameof(MaterialOrders));
            }

            var res = await _storeService.ChangeMaterialOrderStatusAsync(factoryId, orderId, newStatus, ct);

            TempData[res.Success ? "Success" : "Error"] = res.Message;
            return RedirectToAction(nameof(MaterialOrders));
        }

        [HttpGet]
        public async Task<IActionResult> MaterialOrders(CancellationToken ct)
        {
            if (!IsFactoryUser() || !IsVerifiedFactory())
                return DenyToHome();

            var factoryId = GetCurrentUserId();
            var orders = await _storeService.GetMaterialOrdersForFactoryAsync(factoryId, take: 500);
            ViewBag.UserLoggedIn = await PopulateUserViewBagsAsync(factoryId, ct);

            return View("Material/MaterialOrders", orders);
        }

        [HttpGet]
        public async Task<IActionResult> MaterialOrderDetails(int id, CancellationToken ct)
        {
            if (!IsFactoryUser() || !IsVerifiedFactory())
                return DenyToHome();

            var factoryId = GetCurrentUserId();

            var row = await _db.MaterialOrders.AsNoTracking()
                .Include(o => o.MaterialStore)
                    .ThenInclude(m => m.Seller)
                .FirstOrDefaultAsync(o => o.MaterialOrderID == id, ct);

            if (row == null || row.MaterialStore == null)
                return NotFound();

            if (row.MaterialStore.SellerID != factoryId)
                return Forbid();

            ViewBag.UserLoggedIn = await PopulateUserViewBagsAsync(factoryId, ct);

            var buyer = await _db.Users.AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserId == row.BuyerID, ct);

            var seller = row.MaterialStore.Seller;

            var imgs = new[]
            {
                row.MaterialStore.ProductImgURL1,
                row.MaterialStore.ProductImgURL2,
                row.MaterialStore.ProductImgURL3
            }
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x!.Trim())
            .Distinct()
            .ToList();

            var buyerEmail = buyer != null ? DecryptOrRaw(buyer.Email) : null;
            var buyerPhone = buyer != null ? DecryptOrRaw(buyer.phoneNumber) : null;

            var factoryEmail = seller != null ? DecryptOrRaw(seller.Email) : null;
            var factoryPhone = seller != null ? DecryptOrRaw(seller.phoneNumber) : null;

            var vm = new MaterialOrderDetailsModel
            {
                OrderId = row.MaterialOrderID,
                OrderStatus = row.Status,
                OrderDate = row.OrderDate,
                Quantity = row.Quantity,
                UnitPrice = row.UnitPrice,
                CancelUntil = row.CancelUntil,
                ExpectedArrivalDate = row.ExpectedArrivalDate,
                PickupLocation = row.PickupLocation,

                MaterialId = row.MaterialStore.MaterialID,
                ProductType = row.MaterialStore.ProductType,
                Description = row.MaterialStore.Description,
                MaterialPrice = row.MaterialStore.Price,
                MaterialAvailableQty = row.MaterialStore.Quantity,
                Unit = row.MaterialStore.Unit,
                MaterialStatus = row.MaterialStore.Status.ToString(),
                MaterialImages = imgs,

                BuyerId = buyer?.UserId ?? row.BuyerID,
                BuyerName = buyer?.FullName ?? "Buyer",
                BuyerEmail = buyerEmail,
                BuyerPhone = buyerPhone,
                BuyerAddress = buyer?.Address,
                BuyerProfileImg = buyer?.UserProfileImgURL,
                BuyerVerified = buyer?.Verified ?? false,

                FactoryId = seller?.UserId ?? factoryId,
                FactoryName = seller?.FullName ?? "Factory",
                FactoryAddress = seller?.Address,
                FactoryEmail = factoryEmail,
                FactoryPhone = factoryPhone,
                FactoryProfileImg = seller?.UserProfileImgURL,
                FactoryVerified = seller?.Verified ?? false
            };

            // keep same view path as your code
            return View("Material/MaterialOrderDetails", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMaterial(int id, int? deleteQty, CancellationToken ct)
        {
            if (!IsFactoryUser() || !IsVerifiedFactory())
            {
                TempData["Error"] = "Access denied";
                return RedirectToAction(nameof(Materials));
            }

            var factoryId = GetCurrentUserId();
            var res = await _storeService.DeleteMaterialAsync(factoryId, id, ct);

            TempData[res.Success ? "Success" : "Error"] = res.Message;
            return RedirectToAction(nameof(Materials));
        }

        ///////////////////////// Machines Management ///////////////////////////

        [HttpGet]
        public async Task<IActionResult> Machines(CancellationToken ct)
        {
            if (!IsFactoryUser() || !IsVerifiedFactory())
                return DenyToHome();

            var factoryId = GetCurrentUserId();
            if (factoryId == 0)
            {
                ViewBag.UserLoggedIn = false;
                return DenyToHome();
            }

            await PopulateUserViewBagsAsync(factoryId, ct);

            var machines = await _storeService.GetMachinesAsync(factoryId);
            return View("Machine/Machines", machines);
        }

        [HttpGet]
        public async Task<IActionResult> AddMachine(CancellationToken ct)
        {
            if (!IsFactoryUser() || !IsVerifiedFactory())
                return DenyToHome();

            var factoryId = GetCurrentUserId();
            await PopulateFactoryLocationViewBagsAsync(factoryId, ct);
            ViewBag.UserLoggedIn = await PopulateUserViewBagsAsync(factoryId, ct);

            var useFactoryLocation = (bool)(ViewBag.FactoryHasLocation ?? false);

            var model = new MachineProductDetailsModel
            {
                UseFactoryLocation = useFactoryLocation
            };

            return View("Machine/AddMachine", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMachine(MachineProductDetailsModel newMachine, CancellationToken ct)
        {
            if (!IsFactoryUser() || !IsVerifiedFactory())
                return DenyToHome();

            var factoryId = GetCurrentUserId();

            await PopulateFactoryLocationViewBagsAsync(factoryId, ct);
            bool factoryHasLocation = (bool)(ViewBag.FactoryHasLocation ?? false);

            if (newMachine.MinOrderQuantity.HasValue)
            {
                if (newMachine.MinOrderQuantity.Value < 0)
                    ModelState.AddModelError(nameof(newMachine.MinOrderQuantity), "Minimum order quantity cannot be negative.");

                if (newMachine.MinOrderQuantity.Value > newMachine.Quantity)
                    ModelState.AddModelError(nameof(newMachine.MinOrderQuantity), "Minimum order quantity cannot be greater than available quantity.");
            }

            if (newMachine.UseFactoryLocation)
            {
                if (!factoryHasLocation)
                    ModelState.AddModelError(nameof(newMachine.UseFactoryLocation),
                        "Factory location is not set. Please choose custom pickup location and enter it.");
            }
            else
            {
                if (!ModelHasCustomLocation(newMachine.Address, newMachine.Latitude, newMachine.Longitude))
                    ModelState.AddModelError(nameof(newMachine.Address), "Please provide pickup address or coordinates.");
            }

            if (!ModelState.IsValid)
                return View("Machine/AddMachine", newMachine);

            if (!await _storeService.CanFactoryAddProductAsync(factoryId))
            {
                TempData["Error"] = "Your factory account is not authorized to add products.";
                return RedirectToAction(nameof(Machines));
            }

            var success = await _storeService.AddMachineAsync(newMachine, factoryId);

            if (success)
            {
                TempData["Success"] = "Machine added successfully!";
                return RedirectToAction(nameof(Machines));
            }

            TempData["Error"] = "Failed to add machine. Please try again.";
            return View("Machine/AddMachine", newMachine);
        }

        [HttpGet]
        public async Task<IActionResult> EditMachine(int id, CancellationToken ct)
        {
            if (!IsFactoryUser() || !IsVerifiedFactory())
                return DenyToHome();

            var factoryId = GetCurrentUserId();
            var machine = await _storeService.GetMachineByIdAsync(id, factoryId);

            if (machine == null)
            {
                TempData["Error"] = "Machine not found or you don't have permission to edit it.";
                return RedirectToAction(nameof(Machines));
            }

            await PopulateFactoryLocationViewBagsAsync(factoryId, ct);

            var hasAnyOrders = await _db.MachineOrders.AsNoTracking()
                .AnyAsync(o => o.MachineStoreID == id, ct);

            ViewBag.HasAnyOrders = hasAnyOrders;
            ViewBag.CanModify = !hasAnyOrders;
            ViewBag.CanEditPriceQty = true;
            ViewBag.OldQuantity = machine.Quantity;

            return View("Machine/EditMachine", machine);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditMachine(int id, MachineProductDetailsModel editMachine, CancellationToken ct)
        {
            if (!IsFactoryUser() || !IsVerifiedFactory())
                return DenyToHome();

            var factoryId = GetCurrentUserId();

            await PopulateFactoryLocationViewBagsAsync(factoryId, ct);
            bool factoryHasLocation = (bool)(ViewBag.FactoryHasLocation ?? false);

            var hasAnyOrders = await _db.MachineOrders.AsNoTracking()
                .AnyAsync(o => o.MachineStoreID == id, ct);

            ViewBag.HasAnyOrders = hasAnyOrders;
            ViewBag.CanModify = !hasAnyOrders;
            ViewBag.CanEditPriceQty = true;

            var oldQty = await _db.MachineStores.AsNoTracking()
                .Where(m => m.MachineID == id && m.SellerID == factoryId)
                .Select(m => (int?)m.Quantity)
                .FirstOrDefaultAsync(ct);

            if (!oldQty.HasValue)
            {
                TempData["Error"] = "Machine not found or you don't have permission to edit it.";
                return RedirectToAction(nameof(Machines));
            }

            ViewBag.OldQuantity = oldQty.Value;

            if (editMachine.UseFactoryLocation && !factoryHasLocation)
                ModelState.AddModelError(nameof(editMachine.UseFactoryLocation), "Factory location is not set. Please choose custom pickup location.");

            if (!editMachine.UseFactoryLocation)
            {
                if (!ModelHasCustomLocation(editMachine.Address, editMachine.Latitude, editMachine.Longitude))
                    ModelState.AddModelError(nameof(editMachine.Address), "Please provide pickup address or coordinates.");
            }

            if (hasAnyOrders)
            {
                ModelState.Remove(nameof(editMachine.MachineType));
                ModelState.Remove(nameof(editMachine.MinOrderQuantity));
                ModelState.Remove(nameof(editMachine.Status));
                ModelState.Remove(nameof(editMachine.ManufactureDate));
                ModelState.Remove(nameof(editMachine.Condition));
                ModelState.Remove(nameof(editMachine.Brand));
                ModelState.Remove(nameof(editMachine.Model));
                ModelState.Remove(nameof(editMachine.WarrantyMonths));
                ModelState.Remove(nameof(editMachine.CancelWindowDays));
                ModelState.Remove(nameof(editMachine.DeliveryDays));

                if (editMachine.Quantity <= oldQty.Value)
                    ModelState.AddModelError(nameof(editMachine.Quantity),
                        $"With existing orders, quantity must be greater than current quantity ({oldQty.Value}).");

                if (editMachine.Price <= 0)
                    ModelState.AddModelError(nameof(editMachine.Price), "Price must be greater than 0.");
            }
            else
            {
                if (editMachine.MinOrderQuantity.HasValue && editMachine.MinOrderQuantity.Value > editMachine.Quantity)
                    ModelState.AddModelError(nameof(editMachine.MinOrderQuantity), "Minimum order quantity cannot be greater than available quantity.");
            }

            if (!ModelState.IsValid)
                return View("Machine/EditMachine", editMachine);

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
        public async Task<IActionResult> ChangeMachineOrderStatus(int orderId, string status, CancellationToken ct)
        {
            if (!IsFactoryUser() || !IsVerifiedFactory())
            {
                TempData["Error"] = "Access denied.";
                return RedirectToAction(nameof(MachineOrders));
            }

            var factoryId = GetCurrentUserId();
            if (factoryId == 0)
            {
                TempData["Error"] = "Invalid factory.";
                return RedirectToAction(nameof(MachineOrders));
            }

            if (!Enum.TryParse<EnumsOrderStatus>(status, true, out var newStatus))
            {
                TempData["Error"] = "Invalid status value.";
                return RedirectToAction(nameof(MachineOrders));
            }

            var res = await _storeService.ChangeMachineOrderStatusAsync(factoryId, orderId, newStatus, ct);

            TempData[res.Success ? "Success" : "Error"] = res.Message;
            return RedirectToAction(nameof(MachineOrders));
        }

        [HttpGet]
        public async Task<IActionResult> MachineOrders(CancellationToken ct)
        {
            if (!IsFactoryUser() || !IsVerifiedFactory())
                return DenyToHome();

            var factoryId = GetCurrentUserId();
            var orders = await _storeService.GetMachineOrdersForFactoryAsync(factoryId, take: 500);
            ViewBag.UserLoggedIn = await PopulateUserViewBagsAsync(factoryId, ct);

            return View("Machine/MachineOrders", orders);
        }

        [HttpGet]
        public async Task<IActionResult> MachineOrderDetails(int id, CancellationToken ct)
        {
            if (!IsFactoryUser() || !IsVerifiedFactory())
                return DenyToHome();

            var factoryId = GetCurrentUserId();

            var row = await _db.MachineOrders.AsNoTracking()
                .Include(o => o.MachineStore)
                    .ThenInclude(m => m.Seller)
                .FirstOrDefaultAsync(o => o.MachineOrderID == id, ct);

            if (row == null || row.MachineStore == null)
                return NotFound();

            if (row.MachineStore.SellerID != factoryId)
                return Forbid();

            var buyer = await _db.Users.AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserId == row.BuyerID, ct);

            var seller = row.MachineStore.Seller;

            var imgs = new[]
            {
                row.MachineStore.MachineImgURL1,
                row.MachineStore.MachineImgURL2,
                row.MachineStore.MachineImgURL3
            }
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x!.Trim())
            .Distinct()
            .ToList();

            var buyerEmail = buyer != null ? DecryptOrRaw(buyer.Email) : null;
            var buyerPhone = buyer != null ? DecryptOrRaw(buyer.phoneNumber) : null;

            var factoryEmail = seller != null ? DecryptOrRaw(seller.Email) : null;
            var factoryPhone = seller != null ? DecryptOrRaw(seller.phoneNumber) : null;

            var vm = new MachineOrderDetailsModel
            {
                OrderId = row.MachineOrderID,
                OrderStatus = row.Status.ToString(),
                OrderDate = row.OrderDate,
                Quantity = row.Quantity,
                UnitPrice = row.UnitPrice,
                CancelUntil = row.CancelUntil,
                ExpectedArrivalDate = row.ExpectedArrivalDate,
                PickupLocation = row.PickupLocation,

                TotalPrice = row.TotalPrice,
                DepositPaid = row.DepositPaid,

                MachineId = row.MachineStore.MachineID,
                MachineType = row.MachineStore.MachineType,
                Description = row.MachineStore.Description,
                MachinePrice = row.MachineStore.Price,
                MachineAvailableQty = row.MachineStore.Quantity,
                MachineStatus = row.MachineStore.Status.ToString(),
                MachineImages = imgs,

                BuyerId = buyer?.UserId ?? row.BuyerID,
                BuyerName = buyer?.FullName ?? "Buyer",
                BuyerEmail = buyerEmail,
                BuyerPhone = buyerPhone,
                BuyerAddress = buyer?.Address,
                BuyerProfileImg = buyer?.UserProfileImgURL,
                BuyerVerified = buyer?.Verified ?? false,

                FactoryId = seller?.UserId ?? factoryId,
                FactoryName = seller?.FullName ?? "Factory",
                FactoryAddress = seller?.Address,
                FactoryEmail = factoryEmail,
                FactoryPhone = factoryPhone,
                FactoryProfileImg = seller?.UserProfileImgURL,
                FactoryVerified = seller?.Verified ?? false
            };
            ViewBag.UserLoggedIn = await PopulateUserViewBagsAsync(factoryId, ct);

            // keep same view path as your code
            return View("Machine/MachineOrderDetails", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMachine(int id, CancellationToken ct)
        {
            if (!IsFactoryUser() || !IsVerifiedFactory())
            {
                TempData["Error"] = "Access denied";
                return RedirectToAction(nameof(Machines));
            }

            var factoryId = GetCurrentUserId();
            var res = await _storeService.DeleteMachineAsync(factoryId, id, ct);

            TempData[res.Success ? "Success" : "Error"] = res.Message;
            return RedirectToAction(nameof(Machines));
        }

        /////////////////////////// Rentals Management ///////////////////////////

        [HttpGet]
        public async Task<IActionResult> Rentals(CancellationToken ct)
        {
            if (!IsFactoryUser() || !IsVerifiedFactory())
                return DenyToHome();

            var factoryId = GetCurrentUserId();
            if (factoryId == 0)
            {
                ViewBag.UserLoggedIn = false;
                return DenyToHome();
            }

            await PopulateUserViewBagsAsync(factoryId, ct);

            var rentals = await _storeService.GetRentalsAsync(factoryId);
            return View("Rental/Rentals", rentals);
        }

        [HttpGet]
        public async Task<IActionResult> AddRental(CancellationToken ct)
        {
            if (!IsFactoryUser() || !IsVerifiedFactory())
                return DenyToHome();

            var factoryId = GetCurrentUserId();
            await PopulateFactoryLocationViewBagsAsync(factoryId, ct);
            ViewBag.UserLoggedIn = await PopulateUserViewBagsAsync(factoryId, ct);
            var useFactoryLocation = (bool)(ViewBag.FactoryHasLocation ?? false);

            return View("Rental/AddRental", new RentalProductDetailsModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddRental(RentalProductDetailsModel newRental, CancellationToken ct)
        {
            if (!IsFactoryUser() || !IsVerifiedFactory())
                return DenyToHome();

            if (!ModelState.IsValid)
                return View("Rental/AddRental", newRental);

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
        public async Task<IActionResult> EditRental(int id, CancellationToken ct)
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

            var canModify = !await _db.RentalOrders.AsNoTracking()
                .AnyAsync(o => o.RentalStoreID == id &&
                              (o.Status == EnumsOrderStatus.Pending || o.Status == EnumsOrderStatus.Confirmed), ct);

            ViewBag.CanModify = canModify;

            return View("Rental/EditRental", rental);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRental(int id, RentalProductDetailsModel editRental, CancellationToken ct)
        {
            if (!IsFactoryUser() || !IsVerifiedFactory())
                return RedirectToAction("AccessDenied", "Home");

            var factoryId = GetCurrentUserId();

            var result = await _storeService.UpdateRentalAsync(id, editRental, factoryId, ct);

            var rental = await _storeService.GetRentalByIdAsync(id, factoryId);
            if (rental == null)
            {
                ModelState.AddModelError(string.Empty, "Rental not found.");
                ViewBag.CanModify = true;
                return View("Rental/EditRental", editRental);
            }

            ViewBag.CanModify = await _db.RentalOrders.AsNoTracking()
                .AnyAsync(o => o.RentalOrderID == id && o.Status != EnumsOrderStatus.Cancelled, ct);

            if (result.Success)
                ViewBag.Success = result.Message;
            else
                ViewBag.Warning = result.Message;

            return View("Rental/EditRental", rental);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeRentalOrderStatus(int orderId, CancellationToken ct)
        {
            if (!IsFactoryUser() || !IsVerifiedFactory())
                return DenyToHome();

            var ownerId = GetCurrentUserId();
            var res = await _storeService.ChangeRentalOrderStatusAsync(ownerId, orderId, ct);

            TempData[res.Success ? "Success" : "Error"] = res.Message;
            return RedirectToAction(nameof(RentalOrders));
        }

        [HttpGet]
        public async Task<IActionResult> RentalOrders(CancellationToken ct)
        {
            if (!IsFactoryUser() || !IsVerifiedFactory())
                return DenyToHome();

            var ownerId = GetCurrentUserId();
            var groups = await _storeService.GetRentalOrdersForFactoryAsync(ownerId, take: 400, ct: ct);
            ViewBag.UserLoggedIn = await PopulateUserViewBagsAsync(ownerId, ct);

            return View("Rental/RentalOrders", groups);
        }

        [HttpGet]
        public async Task<IActionResult> RentalOrderDetails(int rentalId, CancellationToken ct)
        {
            if (!IsFactoryUser() || !IsVerifiedFactory())
                return DenyToHome();

            var ownerId = GetCurrentUserId();
            var orders = await _storeService.GetRentalOrderDetailsOwnerAsync(ownerId, rentalId, take: 400, ct: ct);
            ViewBag.UserLoggedIn = await PopulateUserViewBagsAsync(ownerId, ct);

            if (orders == null || orders.Count == 0)
                return NotFound();

            return View("Rental/RentalOrderDetails", orders);
        }

        [HttpGet]
        public async Task<IActionResult> GetRentalOrderBuyerInfo(int rentalId, CancellationToken ct)
        {
            if (!IsFactoryUser() || !IsVerifiedFactory())
                return Json(new { success = false, message = "Access denied." });

            var ownerId = GetCurrentUserId();
            var rows = await _storeService.GetOrdersForRentalForOwnerAsync(ownerId, rentalId, ct);

            return Json(new { success = true, rentalId, rows });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRental(int rentalId, CancellationToken ct)
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
                var result = await _storeService.DeleteRentalAsync(rentalId, factoryId, ct);
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
        public async Task<IActionResult> Auctions(CancellationToken ct)
        {
            if (!IsFactoryUser() || !IsVerifiedFactory())
                return DenyToHome();

            var factoryId = GetCurrentUserId();
            if (factoryId == 0)
            {
                ViewBag.UserLoggedIn = false;
                return DenyToHome();
            }

            await PopulateUserViewBagsAsync(factoryId, ct);

            var auctions = await _storeService.GetAuctionsAsync(factoryId);
            return View("Auction/Auctions", auctions);
        }

        [HttpGet]
        public async Task<IActionResult> AddAuction(CancellationToken ct)
        {
            if (!IsFactoryUser() || !IsVerifiedFactory())
                return DenyToHome();

            var factoryId = GetCurrentUserId();
            await PopulateFactoryLocationViewBagsForAuctionAsync(factoryId, ct);

            ViewBag.UserLoggedIn = await PopulateUserViewBagsAsync(factoryId, ct);

            var useFactoryLocation = (bool)(ViewBag.FactoryHasLocation ?? false);

            var model = new AuctionProductDetailsModel
            {
                UseFactoryLocation = useFactoryLocation
            };

            return View("Auction/AddAuction", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddAuction(AuctionProductDetailsModel newAuction, CancellationToken ct)
        {
            if (!IsFactoryUser() || !IsVerifiedFactory())
                return DenyToHome();

            var factoryId = GetCurrentUserId();
            await PopulateFactoryLocationViewBagsForAuctionAsync(factoryId, ct);

            bool factoryHasLocation = (bool)(ViewBag.FactoryHasLocation ?? false);

            if (newAuction.EndDate.HasValue && newAuction.EndDate.Value <= newAuction.StartDate)
                ModelState.AddModelError(nameof(newAuction.EndDate), "End date/time must be after start date/time.");

            if (newAuction.UseFactoryLocation)
            {
                if (!factoryHasLocation)
                    ModelState.AddModelError(nameof(newAuction.UseFactoryLocation),
                        "Factory location is not set. Please choose custom pickup location and enter it.");
            }
            else
            {
                if (!ModelHasCustomLocation(newAuction.Address, newAuction.Latitude, newAuction.Longitude))
                    ModelState.AddModelError(nameof(newAuction.Address), "Please provide pickup address or coordinates.");
            }

            if (!ModelState.IsValid)
                return View("Auction/AddAuction", newAuction);

            if (!await _storeService.CanFactoryAddProductAsync(factoryId))
            {
                TempData["Error"] = "Your factory account is not authorized to add products.";
                return RedirectToAction(nameof(Auctions));
            }

            var success = await _storeService.AddAuctionAsync(newAuction, factoryId);

            if (success)
            {
                TempData["Success"] = "Auction added successfully!";
                return RedirectToAction(nameof(Auctions));
            }

            TempData["Error"] = "Failed to add auction. Please try again.";
            return View("Auction/AddAuction", newAuction);
        }

        [HttpGet]
        public async Task<IActionResult> EditAuction(int id, CancellationToken ct)
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

            var hasActiveOrders = await _db.AuctionOrders.AsNoTracking()
                .AnyAsync(o => o.AuctionStoreID == id &&
                              (o.Status == EnumsOrderStatus.Pending || o.Status == EnumsOrderStatus.Confirmed), ct);

            if (hasActiveOrders)
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
        public async Task<IActionResult> EditAuction(int id, AuctionProductDetailsModel editAuction, CancellationToken ct)
        {
            if (!IsFactoryUser() || !IsVerifiedFactory())
                return DenyToHome();

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

        [HttpGet]
        public async Task<IActionResult> AuctionOrders(CancellationToken ct)
        {
            if (!IsFactoryUser() || !IsVerifiedFactory())
                return DenyToHome();

            var ownerId = GetCurrentUserId();
            var groups = await _storeService.GetAuctionOrdersForFactoryAsync(ownerId, take: 400, ct: ct);
            ViewBag.UserLoggedIn = await PopulateUserViewBagsAsync(ownerId, ct);
            return View("Auction/AuctionOrders", groups);
        }

        [HttpGet]
        public async Task<IActionResult> AuctionOrderDetails(int auctionId, CancellationToken ct)
        {
            if (!IsFactoryUser() || !IsVerifiedFactory())
                return DenyToHome();

            var ownerId = GetCurrentUserId();
            var orders = await _storeService.GetAuctionOrdersByAuctionIdForFactoryAsync(ownerId, auctionId, ct);
            ViewBag.UserLoggedIn = await PopulateUserViewBagsAsync(ownerId, ct);

            return View("Auction/AuctionOrderDetails", orders);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeAuctionOrderStatus(int auctionId, int? winnerOrderId, CancellationToken ct)
        {
            if (!IsFactoryUser() || !IsVerifiedFactory())
                return DenyToHome();

            var ownerId = GetCurrentUserId();
            var res = await _storeService.ConfirmAuctionWinnerAsync(ownerId, auctionId, winnerOrderId, ct);

            TempData[res.Success ? "Success" : "Error"] = res.Message;
            return RedirectToAction(nameof(AuctionOrderDetails), new { auctionId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAuction(DeleteProductModel deleteAuction, CancellationToken ct)
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
                var result = await _storeService.DeleteAuctionAsync(deleteAuction.Id, factoryId, ct);
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
        public async Task<IActionResult> Jobs(CancellationToken ct)
        {
            if (!IsFactoryUser() || !IsVerifiedFactory())
                return DenyToHome();

            var factoryId = GetCurrentUserId();
            if (factoryId == 0)
            {
                ViewBag.UserLoggedIn = false;
                return DenyToHome();
            }

            await PopulateUserViewBagsAsync(factoryId, ct);

            var jobs = await _storeService.GetJobsAsync(factoryId);
            return View("Job/Jobs", jobs);
        }

        [HttpGet]
        public async Task<IActionResult> AddJob(CancellationToken ct)
        {
            if (!IsFactoryUser() || !IsVerifiedFactory())
                return DenyToHome();

            var factoryId = GetCurrentUserId();
            if (factoryId == 0)
            {
                ViewBag.UserLoggedIn = false;
                return DenyToHome();
            }

            await PopulateUserViewBagsAsync(factoryId, ct);
            ViewBag.UserLoggedIn = await PopulateUserViewBagsAsync(factoryId, ct);

            return View("Job/AddJob", new JobProductDetailsModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddJob(JobProductDetailsModel newJob, CancellationToken ct)
        {
            if (!IsFactoryUser() || !IsVerifiedFactory())
                return DenyToHome();

            if (!ModelState.IsValid)
                return View("Job/AddJob", newJob);

            var factoryId = GetCurrentUserId();

            if (!await _storeService.CanFactoryAddProductAsync(factoryId))
            {
                TempData["Error"] = "Your factory account is not authorized to add products.";
                return RedirectToAction(nameof(Jobs));
            }

            try
            {
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
        public async Task<IActionResult> EditJob(int id, CancellationToken ct)
        {
            if (!IsFactoryUser() || !IsVerifiedFactory())
                return DenyToHome();

            var factoryId = GetCurrentUserId();
            var job = await _storeService.GetJobByIdAsync(id, factoryId);

            if (job == null)
            {
                TempData["Error"] = "Job posting not found or you don't have permission to edit it.";
                return RedirectToAction(nameof(Jobs));
            }

            // keep same behavior: these viewbags were based on session user
            var sessionUserId = HttpContext.Session.GetInt32("UserID");
            if (sessionUserId.HasValue)
                await PopulateUserViewBagsAsync(sessionUserId.Value, ct);
            else
                ViewBag.UserLoggedIn = false;

            var hasOrders = await _db.JobOrders.AsNoTracking().AnyAsync(o => o.JobStoreID == id, ct);
            ViewBag.CanModify = !hasOrders;

            job.Id = id;
            return View("Job/EditJob", job);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditJob(int id, JobProductDetailsModel editJob, CancellationToken ct)
        {
            if (!IsFactoryUser() || !IsVerifiedFactory())
                return DenyToHome();

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
                return DenyToHome();

            var factoryId = GetCurrentUserId();
            if (factoryId == 0) return DenyToHome();

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
        public async Task<IActionResult> JobOrders(CancellationToken ct)
        {
            if (!IsFactoryUser() || !IsVerifiedFactory())
                return DenyToHome();

            var ownerId = GetCurrentUserId();
            //var groups = await _storeService.GetJobOrdersForFactoryAsync(ownerId, ct: ct);
            ViewBag.UserLoggedIn = await PopulateUserViewBagsAsync(ownerId, ct);
            return View("Job/JobOrders");
        }

        [HttpGet]
        public async Task<IActionResult> JobDetails(int id, CancellationToken ct)
        {
            if (!IsFactoryUser() || !IsVerifiedFactory())
                return DenyToHome();

            var factoryId = GetCurrentUserId();
            if (factoryId == 0) return DenyToHome();

            // keep same behavior: viewbags from session
            var sessionUserId = HttpContext.Session.GetInt32("UserID");
            if (sessionUserId.HasValue)
                await PopulateUserViewBagsAsync(sessionUserId.Value, ct);
            else
                ViewBag.UserLoggedIn = false;

            var job = await _storeService.GetJobByIdAsync(id, factoryId);
            if (job == null)
            {
                TempData["Error"] = "Job not found.";
                return RedirectToAction(nameof(Jobs));
            }

            var hasOrders = await _db.JobOrders.AsNoTracking().AnyAsync(o => o.JobStoreID == id, ct);

            var orders = await _storeService.GetJobOrdersForFactoryAsync(id, factoryId, ct);
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

            return View("Job/JobDetails", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteJob(DeleteProductModel deleteJob, CancellationToken ct)
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
    }
}
