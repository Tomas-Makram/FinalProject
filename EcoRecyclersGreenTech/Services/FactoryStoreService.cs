using EcoRecyclersGreenTech.Data.Orders;
using EcoRecyclersGreenTech.Data.Stores;
using EcoRecyclersGreenTech.Data.Users;
using EcoRecyclersGreenTech.Models;
using EcoRecyclersGreenTech.Models.FactoryStore;
using EcoRecyclersGreenTech.Models.FactoryStore.Dashboard;
using EcoRecyclersGreenTech.Models.FactoryStore.Orders;
using EcoRecyclersGreenTech.Models.FactoryStore.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Reflection;
using System.Security.Policy;
using static EcoRecyclersGreenTech.Data.Stores.EnumsProductStatus;
using static EcoRecyclersGreenTech.Services.IFactoryStoreService;

namespace EcoRecyclersGreenTech.Services
{
    public interface IFactoryStoreService
    {
        // Message Request
        public class ServiceResult
        {
            public bool Success { get; set; }
            public string Message { get; set; } = "";
            public int idObj { get; set; } = 0;
            public static ServiceResult Ok(string msg, int idObj = 0) => new() { Success = true, Message = msg, idObj = idObj };
            public static ServiceResult Fail(string msg) => new() { Success = false, Message = msg, idObj = -1 };
        }

        // Dashboard
        Task<DashboardModel> GetDashboardStatsAsync(int factoryId);

        // Validation
        Task<bool> CanFactoryAddProductAsync(int factoryId);

        ///////////////////////////////////////// Material Functions /////////////////////////////////////////
        ////////////////////////////// Factory Function //////////////////////////////

        Task<List<FactoryStoreModel>> GetMaterialsAsync(int factoryId, SearchFilterModel? filter = null);
        Task<MaterialProductDetailsModel?> GetMaterialByIdAsync(int id, int factoryId);
        Task<bool> AddMaterialAsync(MaterialProductDetailsModel model, int factoryId);
        Task<bool> UpdateMaterialAsync(int id, MaterialProductDetailsModel model, int factoryId);
        Task<ServiceResult> ChangeMaterialOrderStatusAsync(int factoryId, int orderId, EnumsOrderStatus newStatus, CancellationToken ct = default);
        Task<List<MaterialOrderDetailsModel>> GetMaterialOrdersForFactoryAsync(int factoryId, int take = 200, CancellationToken ct = default);
        Task<ServiceResult> DeleteMaterialAsync(int factoryId, int materialId, CancellationToken ct = default);

        ///////////////////////////////////////// Material Functions /////////////////////////////////////////
        ////////////////////////////// User Function //////////////////////////////
        Task<List<FactoryStoreModel>> GetPublicMaterialsAsync(SearchFilterModel? filter = null);
        Task<FactoryStoreModel?> GetPublicMaterialDetailsAsync(int id);
        Task<ServiceResult> PlaceMaterialOrderAsync(int buyerId, int materialId, int quantity, decimal depositPaid, decimal walletUsed, string provider, string providerPaymentId, CancellationToken ct = default);
        Task<FactoryStoreModel?> GetMaterialDetailsForBuyerAsync(int materialId, int buyerId);
        Task<(MaterialOrder? order, FactoryStoreModel? details)> GetMaterialOrderDetailsAsync(int buyerId, int orderId);
        Task<ServiceResult> CancelMaterialOrderAsync(int buyerId, int orderId, CancellationToken ct = default);
        Task<ServiceResult> HideMaterialOrderForBuyerAsync(int buyerId, int orderId, CancellationToken ct = default);
        Task<List<MaterialOrder>> GetMaterialOrdersForBuyerAsync(int buyerId);

        ///////////////////////////////////////// Machine Functions /////////////////////////////////////////
        ////////////////////////////// Factory Functions //////////////////////////////
        
        Task<List<FactoryStoreModel>> GetMachinesAsync(int factoryId, SearchFilterModel? filter = null);
        Task<MachineProductDetailsModel?> GetMachineByIdAsync(int id, int factoryId);
        Task<bool> AddMachineAsync(MachineProductDetailsModel model, int factoryId);
        Task<bool> UpdateMachineAsync(int id, MachineProductDetailsModel model, int factoryId);
        Task<ServiceResult> ChangeMachineOrderStatusAsync(int factoryId, int orderId, EnumsOrderStatus newStatus, CancellationToken ct = default);        Task<List<MachineOrderDetailsModel>> GetMachineOrdersForFactoryAsync(int factoryId, int take = 200, CancellationToken ct = default);
        Task<ServiceResult> DeleteMachineAsync(int id, int factoryId, CancellationToken ct = default);

        ///////////////////////////////////////// Machine Functions /////////////////////////////////////////
        ////////////////////////////// Factory Functions //////////////////////////////

        Task<List<FactoryStoreModel>> GetPublicMachinesAsync(SearchFilterModel? filter = null);
        Task<FactoryStoreModel?> GetPublicMachineDetailsAsync(int id);
        Task<ServiceResult> PlaceMachineOrderAsync(int buyerId, int machineId, int quantity, decimal paidAmount, decimal walletUsed, string provider, string providerPaymentId, CancellationToken ct = default);
        Task<FactoryStoreModel?> GetMachineDetailsForBuyerAsync(int machineId, int buyerId);
        Task<(MachineOrder? order, FactoryStoreModel? details)> GetMachineOrderDetailsAsync(int buyerId, int orderId);
        Task<ServiceResult> CancelMachineOrderAsync(int buyerId, int orderId, CancellationToken ct = default);
        Task<ServiceResult> HideMachineOrderForBuyerAsync(int buyerId, int orderId, CancellationToken ct = default);
        Task<List<MachineOrder>> GetMachineOrdersForBuyerAsync(int buyerId);

        ///////////////////////////////////////// Rental Functions /////////////////////////////////////////
        ////////////////////////////// Factory Functions //////////////////////////////
        
        Task<List<FactoryStoreModel>> GetRentalsAsync(int factoryId, SearchFilterModel? filter = null);
        Task<RentalProductDetailsModel?> GetRentalByIdAsync(int id, int factoryId);
        Task<ServiceResult> AddRentalAsync(RentalProductDetailsModel model, int factoryId, CancellationToken ct = default);
        Task<ServiceResult> UpdateRentalAsync(int id, RentalProductDetailsModel model, int factoryId, CancellationToken ct = default);
        Task<List<RentalOrderDetailsModel>> GetRentalOrdersForFactoryAsync(int ownerId, int take = 300, CancellationToken ct = default);
        Task<List<RentalOrderDetailsModel>> GetRentalOrderDetailsOwnerAsync(int ownerId, int rentalId, int take = 400, CancellationToken ct = default);
        Task<ServiceResult> ChangeRentalOrderStatusAsync(int ownerId, int orderId, CancellationToken ct = default);
        Task<List<RentalOrderDetailsModel>> GetOrdersForRentalForOwnerAsync(int ownerId, int rentalId, CancellationToken ct = default);
        Task<ServiceResult> DeleteRentalAsync(int id, int factoryId, CancellationToken ct = default);

        ///////////////////////////////////////// Rental Functions /////////////////////////////////////////
        ////////////////////////////// User Functions //////////////////////////////

        Task<List<FactoryStoreModel>> GetPublicRentalsAsync(SearchFilterModel? filter = null);
        Task<FactoryStoreModel?> GetPublicRentalDetailsAsync(int rentalId, int? viewerUserId);
        Task<List<RentalOrderDetailsModel>> GetRentalOrdersForBuyerAsync(int buyerId, int take = 200, CancellationToken ct = default);
        Task<ServiceResult> PlaceRentalOrderAsync(int buyerId, int rentalId, decimal amountPaid, decimal walletUsed, string provider, string providerPaymentId, CancellationToken ct = default);
        Task<(RentalOrder? order, FactoryStoreModel? details)> GetRentalOrderDetailsAsync(int buyerId, int orderId, CancellationToken ct = default);
        Task<ServiceResult> CancelOrDeleteRentalOrderByBuyerAsync(int buyerId, int rentalOrderId, CancellationToken ct = default);

        ///////////////////////////////////////// Auction Functions /////////////////////////////////////////
        ////////////////////////////// Factory Functions //////////////////////////////

        Task<List<FactoryStoreModel>> GetAuctionsAsync(int factoryId, SearchFilterModel? filter = null);
        Task<AuctionProductDetailsModel?> GetAuctionByIdAsync(int id, int factoryId);
        Task<bool> AddAuctionAsync(AuctionProductDetailsModel model, int factoryId);
        Task<bool> UpdateAuctionAsync(int id, AuctionProductDetailsModel model, int factoryId);
        Task<ServiceResult> ConfirmAuctionWinnerAsync(int ownerId, int auctionId, int? winnerOrderId = null, CancellationToken ct = default);
        Task<(AuctionOrder? order, FactoryStoreModel? details)> GetAuctionOrderDetailsAsync(int winnerId, int orderId, CancellationToken ct = default);
        Task<List<AuctionOrderDetailsModel>> GetAuctionOrdersForFactoryAsync(int ownerId, int take = 300, CancellationToken ct = default);
        Task<List<AuctionOrderDetailsModel>> GetAuctionOrdersByAuctionIdForFactoryAsync(int ownerId, int auctionId, CancellationToken ct = default);
        Task<List<AuctionOrderDetailsModel>> GetAuctionOrderDetailsOwnerAsync(int ownerId, int auctionId, int take = 400, CancellationToken ct = default);
        Task<ServiceResult> DeleteAuctionAsync(int id, int factoryId, CancellationToken ct = default);

        ///////////////////////////////////////// Auction Functions /////////////////////////////////////////
        ////////////////////////////// User Functions //////////////////////////////
        
        Task<List<FactoryStoreModel>> GetPublicAuctionsAsync(SearchFilterModel? filter = null);
        Task<FactoryStoreModel?> GetPublicAuctionDetailsAsync(int id);
        Task<ServiceResult> PlaceAuctionOrderAsync(int winnerId, int auctionId, decimal bidAmount, decimal amountPaid, decimal walletUsed, string provider, string providerPaymentId, CancellationToken ct = default);
        Task<List<AuctionOrderDetailsModel>> GetAuctionOrdersForWinnerAsync(int winnerId, int take = 300, CancellationToken ct = default);
        Task<ServiceResult> CancelOrDeleteAuctionOrderByWinnerAsync(int bidderId, int auctionOrderId, CancellationToken ct = default);

        ////////////////////////////////////////////////////////////////
        Task<List<FactoryStoreModel>> GetJobsAsync(int factoryId, SearchFilterModel? filter = null);
        Task<bool> JobHasOrdersAsync(int jobId, int factoryId, CancellationToken ct = default);
        Task<JobProductDetailsModel?> GetJobByIdAsync(int id, int factoryId);
        Task<ServiceResult> AddJobAsync(JobProductDetailsModel model, int factoryId, CancellationToken ct = default);
        Task<ServiceResult> UpdateJobAsync(int id, JobProductDetailsModel model, int factoryId, CancellationToken ct = default);
        Task<ServiceResult> DeleteJobAsync(int id, int factoryId);
        Task<ServiceResult> PlaceJobOrderAsync(int userId, int jobId, CancellationToken ct = default);
        Task<List<JobOrder>> GetJobOrdersForUserAsync(int userId);
        Task<List<FactoryStoreModel>> GetPublicJobsAsync(SearchFilterModel? filter = null);
        Task<List<JobOrderDetailsModel>> GetJobOrdersForFactoryAsync(int jobId, int factoryId, CancellationToken ct = default);
        Task<FactoryStoreModel?> GetPublicJobDetailsAsync(int jobId);
    }

    public class FactoryStoreService : IFactoryStoreService
    {
        private readonly DBContext _db;
        private readonly IWebHostEnvironment _environment;
        private readonly IImageStorageService _imageStorage;
        private readonly ILocationService _locationService;
        private readonly ILogger<FactoryStoreService> _logger;
        private readonly IOptions<PricingOptions> _pricingOptions;
        private readonly IEmailTemplateService _emailTemplateService;
        private readonly IDataCiphers _dataCiphers;

        public FactoryStoreService(DBContext db, IWebHostEnvironment environment, ILogger<FactoryStoreService> logger, IImageStorageService imageStorage, ILocationService locationService, IOptions<PricingOptions> pricingOptions, IEmailTemplateService emailTemplateService,IDataCiphers dataCiphers)
        {
            _db = db;
            _logger = logger;
            _environment = environment;
            _dataCiphers = dataCiphers;
            _imageStorage = imageStorage;
            _pricingOptions = pricingOptions;
            _locationService = locationService;
            _emailTemplateService = emailTemplateService;
        }

        // Validation Methods

        public async Task<bool> CanFactoryAddProductAsync(int factoryId)
        {
            try
            {
                var factory = await _db.Users.FindAsync(factoryId);
                return factory?.Verified == true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if factory can add product");
                return false;
            }
        }

        private static int MinQtyToInt(decimal? x)
        {
            if (!x.HasValue) return 1;
            var v = (int)Math.Ceiling(x.Value);
            return v <= 0 ? 1 : v;
        }

        // Wallet Methods

        private async Task<Wallet> GetOrCreateWalletAsync(int userId, CancellationToken ct)
        {
            var wallet = await _db.Wallets.FirstOrDefaultAsync(w => w.UserId == userId, ct);
            if (wallet != null) return wallet;

            wallet = new Wallet
            {
                UserId = userId,
                Balance = 0m,
                ReservedBalance = 0m,
                CreatedAt = DateTime.UtcNow
            };

            _db.Wallets.Add(wallet);
            await _db.SaveChangesAsync(ct);
            return wallet;
        }

        private async Task<bool> ExistsIdemAsync(int walletId, string idemKey, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(idemKey)) return false;

            return await _db.WalletTransactions.AsNoTracking()
                .AnyAsync(t => t.WalletId == walletId && t.IdempotencyKey == idemKey, ct);
        }

        private async Task<WalletTransaction> AddWalletTxnAsync(Wallet wallet, WalletTxnType type, decimal amount, string currency, string? note, string? idempotencyKey, CancellationToken ct)
        {
            amount = Math.Round(amount, 2, MidpointRounding.AwayFromZero);

            if (!string.IsNullOrWhiteSpace(idempotencyKey))
            {
                var existing = await _db.WalletTransactions.AsNoTracking()
                    .FirstOrDefaultAsync(t => t.WalletId == wallet.Id && t.IdempotencyKey == idempotencyKey, ct);

                if (existing != null) return existing; // real existing row
            }

            wallet.Balance = Math.Round(wallet.Balance + amount, 2, MidpointRounding.AwayFromZero);

            var txn = new WalletTransaction
            {
                WalletId = wallet.Id,
                Type = type,
                Status = WalletTxnStatus.Succeeded,
                Amount = amount,
                BalanceAfter = wallet.Balance,
                Currency = currency,
                Note = note,
                IdempotencyKey = idempotencyKey,
                CreatedAt = DateTime.UtcNow
            };

            _db.WalletTransactions.Add(txn);
            await _db.SaveChangesAsync(ct);
            return txn;
        }

        // Hold funds inside wallet: increases Balance and ReservedBalance
        private async Task HoldAsync(Wallet wallet, decimal amount, string note, string idemKey, CancellationToken ct)
        {
            amount = Math.Round(amount, 2, MidpointRounding.AwayFromZero);
            if (amount <= 0m) return;

            // guard BEFORE touching reserved/balance
            if (!string.IsNullOrWhiteSpace(idemKey) && await ExistsIdemAsync(wallet.Id, idemKey, ct))
                return;

            wallet.ReservedBalance = Math.Round(wallet.ReservedBalance + amount, 2, MidpointRounding.AwayFromZero);

            await AddWalletTxnAsync(
                wallet: wallet,
                type: WalletTxnType.Hold,
                amount: amount,
                currency: "EGP",
                note: $"{note} | HOLD +{amount:0.00}",
                idempotencyKey: idemKey,
                ct: ct
            );
        }

        // Release hold: decreases ReservedBalance only (Balance unchanged)
        private async Task ReleaseHoldAsync(Wallet wallet, decimal amount, string note, string idemKey, CancellationToken ct)
        {
            amount = Math.Round(amount, 2, MidpointRounding.AwayFromZero);
            if (amount <= 0m) return;

            // guard BEFORE touching reserved
            if (!string.IsNullOrWhiteSpace(idemKey) && await ExistsIdemAsync(wallet.Id, idemKey, ct))
                return;

            if (wallet.ReservedBalance + 0.0001m < amount)
                throw new InvalidOperationException("Reserved balance is insufficient to release.");

            wallet.ReservedBalance = Math.Round(wallet.ReservedBalance - amount, 2, MidpointRounding.AwayFromZero);

            var txn = new WalletTransaction
            {
                WalletId = wallet.Id,
                Type = WalletTxnType.ReleaseHold,
                Status = WalletTxnStatus.Succeeded,
                Amount = amount,
                BalanceAfter = wallet.Balance,
                Currency = "EGP",
                Note = $"{note} | RELEASE -{amount:0.00}",
                IdempotencyKey = idemKey,
                CreatedAt = DateTime.UtcNow
            };

            _db.WalletTransactions.Add(txn);
            await _db.SaveChangesAsync(ct);
        }

        // Transfer money between wallets. If consumeFromReserved=true, decreases sender ReservedBalance too.
        private async Task TransferAsync(Wallet from, Wallet to, decimal amount, bool consumeFromReserved, string note, string idemKey, CancellationToken ct)
        {
            amount = Math.Round(amount, 2, MidpointRounding.AwayFromZero);
            if (amount <= 0m) return;

            var outKey = $"{idemKey}:OUT";
            var inKey = $"{idemKey}:IN";

            // guard BEFORE touching anything
            if (!string.IsNullOrWhiteSpace(outKey) && await ExistsIdemAsync(from.Id, outKey, ct))
                return;
            if (!string.IsNullOrWhiteSpace(inKey) && await ExistsIdemAsync(to.Id, inKey, ct))
                return;

            if (from.Balance + 0.0001m < amount)
                throw new InvalidOperationException("Sender balance is insufficient.");

            if (consumeFromReserved)
            {
                if (from.ReservedBalance + 0.0001m < amount)
                    throw new InvalidOperationException("Sender reserved balance is insufficient.");

                from.ReservedBalance = Math.Round(from.ReservedBalance - amount, 2, MidpointRounding.AwayFromZero);
            }

            await AddWalletTxnAsync(from, WalletTxnType.PaymentDebit, -amount, "EGP", $"{note} | OUT -{amount:0.00}", outKey, ct);
            await AddWalletTxnAsync(to, WalletTxnType.PaymentCredit, amount, "EGP", $"{note} | IN +{amount:0.00}", inKey, ct);
        }

        ////////////////////////////////////////// Dashboard Methods ////////////////////////////////////
        /////////////////////////////// Factory Method ///////////////////////////////

        public async Task<DashboardModel> GetDashboardStatsAsync(int factoryId)
        {
            try
            {
                var stats = new DashboardModel
                {
                    ActiveMaterials = await _db.MaterialStores
                        .CountAsync(m => m.SellerID == factoryId && m.Status == ProductStatus.Available),
                    ActiveMachines = await _db.MachineStores
                        .CountAsync(m => m.SellerID == factoryId && m.Status == ProductStatus.Available),
                    ActiveRentals = await _db.RentalStores
                        .CountAsync(r => r.OwnerID == factoryId && r.Status == ProductStatus.Available),
                    ActiveAuctions = await _db.AuctionStores
                        .CountAsync(a => a.SellerID == factoryId && a.Status == ProductStatus.Available),
                    ActiveJobs = await _db.JobStores
                        .CountAsync(j => j.PostedBy == factoryId && j.Status == ProductStatus.Available)
                };

                stats.TotalProducts = stats.ActiveMaterials + stats.ActiveMachines +
                                     stats.ActiveRentals + stats.ActiveAuctions + stats.ActiveJobs;

                // Get recent orders
                var recentOrders = await _db.MaterialOrders
                    .Where(o => _db.MaterialStores.Any(m => m.MaterialID == o.MaterialStoreID && m.SellerID == factoryId))
                    .Include(o => o.Buyer)
                    .Include(o => o.MaterialStore)
                    .OrderByDescending(o => o.OrderDate)
                    .Take(5)
                    .Select(o => new RecentOrderModel
                    {
                        OrderNumber = $"MAT-{o.MaterialOrderID:00000}",
                        CustomerName = o.Buyer!.FullName,
                        ProductName = o.MaterialStore!.ProductType,
                        Amount = o.MaterialStore.Price,
                        Status = o.Status,
                        OrderDate = o.OrderDate
                    })
                    .ToListAsync();

                stats.RecentOrders = recentOrders;

                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard stats");
                return new DashboardModel();
            }
        }

        ////////////////////////////////////////// Material Methods ////////////////////////////////////
        /////////////////////////////// Factory Method ///////////////////////////////

        // Get All Material Object To Factory
        public async Task<List<FactoryStoreModel>> GetMaterialsAsync(int factoryId, SearchFilterModel? filter = null)
        {
            var activeStatuses = new[]
            {
                EnumsOrderStatus.Pending,
                EnumsOrderStatus.Confirmed,
                EnumsOrderStatus.Processing,
                EnumsOrderStatus.ReadyForPickup,
                EnumsOrderStatus.Shipped
            };

            try
            {
                IQueryable<MaterialStore> query = _db.MaterialStores
                    .AsNoTracking()
                    .Where(m => m.SellerID == factoryId)
                    .Include(m => m.Seller);

                if (filter != null)
                {
                    if (!string.IsNullOrWhiteSpace(filter.Keyword))
                    {
                        var keyword = filter.Keyword.Trim();
                        query = query.Where(m =>
                            (m.ProductType != null && EF.Functions.Like(m.ProductType, $"%{keyword}%")) ||
                            (m.Description != null && EF.Functions.Like(m.Description, $"%{keyword}%")));
                    }

                    if (!string.IsNullOrWhiteSpace(filter.Status))
                    {
                        if (Enum.TryParse<ProductStatus>(filter.Status.Trim(), true, out var statusEnum))
                            query = query.Where(m => m.Status == statusEnum);
                    }

                    if (filter.MinPrice.HasValue)
                        query = query.Where(m => m.Price >= filter.MinPrice.Value);

                    if (filter.MaxPrice.HasValue)
                        query = query.Where(m => m.Price <= filter.MaxPrice.Value);
                }

                var rows = await query
                    .OrderByDescending(m => m.CreatedAt)
                    .Select(m => new
                    {
                        Material = m,
                        OrdersCount = _db.MaterialOrders.Count(o => o.MaterialStoreID == m.MaterialID),
                        ActiveOrdersCount = _db.MaterialOrders.Count(o =>
                            o.MaterialStoreID == m.MaterialID &&
                            activeStatuses.Contains(o.Status))
                    })
                    .ToListAsync();

                var materials = rows.Select(x =>
                {
                    var images = new[] {
                    x.Material.ProductImgURL1,
                    x.Material.ProductImgURL2,
                    x.Material.ProductImgURL3
                }
                        .Where(url => !string.IsNullOrWhiteSpace(url))
                        .Select(url => url!.Trim())
                        .Distinct()
                        .ToList();

                    return new FactoryStoreModel
                    {
                        Id = x.Material.MaterialID,
                        Name = x.Material.ProductType ?? "Material",
                        Type = "Material",

                        ImageUrl = images.FirstOrDefault(),
                        ImageUrls = images,

                        Price = x.Material.Price,
                        AvailableQuantity = x.Material.Quantity,
                        Unit = x.Material.Unit ?? "unit",
                        Status = x.Material.Status.ToString(),

                        SellerName = x.Material.Seller?.FullName,
                        CreatedAt = x.Material.CreatedAt,
                        IsVerifiedSeller = x.Material.Seller?.Verified ?? false,

                        OrdersCount = x.OrdersCount,
                        ActiveOrdersCount = x.ActiveOrdersCount
                    };
                }).ToList();

                return materials;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting materials");
                return new List<FactoryStoreModel>();
            }
        }

        // Get Material Object Data by ID To Factory
        public async Task<MaterialProductDetailsModel?> GetMaterialByIdAsync(int id, int factoryId)
        {
            try
            {
                var material = await _db.MaterialStores
                    .AsNoTracking()
                    .FirstOrDefaultAsync(m => m.MaterialID == id && m.SellerID == factoryId);

                if (material == null) return null;

                return new MaterialProductDetailsModel
                {
                    Id = material.MaterialID,
                    ProductType = material.ProductType,
                    Quantity = material.Quantity,
                    Description = material.Description,
                    Price = material.Price,
                    Unit = material.Unit,
                    MinOrderQuantity = material.MinOrderQuantity,

                    CurrentImageUrl1 = material.ProductImgURL1,
                    CurrentImageUrl2 = material.ProductImgURL2,
                    CurrentImageUrl3 = material.ProductImgURL3,

                    Status = material.Status,

                    Address = material.Address,
                    Latitude = material.Latitude,
                    Longitude = material.Longitude,

                    UseFactoryLocation =
                        string.IsNullOrWhiteSpace(material.Address) &&
                        !(material.Latitude.HasValue && material.Longitude.HasValue),

                    CancelWindowDays = material.CancelWindowDays,
                    DeliveryDays = material.DeliveryDays
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting material by ID");
                return null;
            }
        }

        // Add New Material Object 
        public async Task<bool> AddMaterialAsync(MaterialProductDetailsModel model, int factoryUserId)
        {
            try
            {
                // Bring factory user location for default usage
                var factoryUser = await _db.Users.AsNoTracking()
                    .FirstOrDefaultAsync(u => u.UserId == factoryUserId);

                if (factoryUser == null)
                    return false;

                // Decide location source
                string? address = null;
                decimal? lat = null;
                decimal? lng = null;

                bool factoryHasLocation =
                    !string.IsNullOrWhiteSpace(factoryUser.Address) ||
                    (factoryUser.Latitude.HasValue && factoryUser.Longitude.HasValue);

                if (model.UseFactoryLocation)
                {
                    if (!factoryHasLocation)
                    {
                        // Factory location missing => block save
                        return false;
                    }

                    address = factoryUser.Address;
                    lat = factoryUser.Latitude;
                    lng = factoryUser.Longitude;
                }
                else
                {
                    bool hasAddress = !string.IsNullOrWhiteSpace(model.Address);
                    bool hasCoords = model.Latitude.HasValue && model.Longitude.HasValue;

                    if (!hasAddress && !hasCoords)
                    {
                        // Custom selected but empty => block save
                        return false;
                    }

                    address = model.Address?.Trim();
                    lat = model.Latitude;
                    lng = model.Longitude;
                }

                var entity = new MaterialStore
                {
                    SellerID = factoryUserId,
                    ProductType = model.ProductType?.Trim(),
                    Quantity = model.Quantity,
                    Description = model.Description?.Trim(),
                    Price = model.Price,
                    Unit = string.IsNullOrWhiteSpace(model.Unit) ? "kg" : model.Unit.Trim(),
                    MinOrderQuantity = model.MinOrderQuantity,
                    Status = ProductStatus.Available,
                    CreatedAt = DateTime.UtcNow,

                    // Location fields
                    Address = address,
                    Latitude = lat,
                    Longitude = lng,

                    CancelWindowDays = model.CancelWindowDays,
                    DeliveryDays = model.DeliveryDays
                };

                // Upload images
                if (model.ProductImage1 != null && model.ProductImage1.Length > 0)
                    entity.ProductImgURL1 = await _imageStorage.UploadAsync(model.ProductImage1, "materials");

                if (model.ProductImage2 != null && model.ProductImage2.Length > 0)
                    entity.ProductImgURL2 = await _imageStorage.UploadAsync(model.ProductImage2, "materials");

                if (model.ProductImage3 != null && model.ProductImage3.Length > 0)
                    entity.ProductImgURL3 = await _imageStorage.UploadAsync(model.ProductImage3, "materials");

                _db.MaterialStores.Add(entity);
                await _db.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding material");
                return false;
            }
        }

        // Update Data Material Object by Rules
        public async Task<bool> UpdateMaterialAsync(int id, MaterialProductDetailsModel model, int factoryId)
        {
            try
            {
                var material = await _db.MaterialStores
                    .FirstOrDefaultAsync(m => m.MaterialID == id && m.SellerID == factoryId);

                if (material == null) return false;

                // Get Any Order in this Object 
                var hasAnyOrders = await _db.MaterialOrders
                    .AsNoTracking()
                    .AnyAsync(o => o.MaterialStoreID == id);

                // Allowed Always: Description + Images + Pickup Location + (Price, Quantity with rule when has orders)

                // Description
                material.Description = model.Description?.Trim();

                // Pickup Location (Factory default OR custom)
                string? resolvedAddress = null;
                decimal? resolvedLat = null;
                decimal? resolvedLng = null;

                if (model.UseFactoryLocation)
                {
                    var factoryUser = await _db.Users
                        .AsNoTracking()
                        .FirstOrDefaultAsync(u => u.UserId == factoryId);

                    bool factoryHasLocation =
                        factoryUser != null &&
                        (
                            !string.IsNullOrWhiteSpace(factoryUser.Address) ||
                            (factoryUser.Latitude.HasValue && factoryUser.Longitude.HasValue)
                        );

                    if (!factoryHasLocation)
                        return false;

                    resolvedAddress = factoryUser!.Address?.Trim();
                    resolvedLat = factoryUser.Latitude;
                    resolvedLng = factoryUser.Longitude;
                }
                else
                {
                    bool hasAddress = !string.IsNullOrWhiteSpace(model.Address);
                    bool hasCoords = model.Latitude.HasValue && model.Longitude.HasValue;

                    if (!hasAddress && !hasCoords)
                        return false;

                    resolvedAddress = model.Address?.Trim();
                    resolvedLat = model.Latitude;
                    resolvedLng = model.Longitude;
                }

                material.Address = resolvedAddress;
                material.Latitude = resolvedLat;
                material.Longitude = resolvedLng;

                // mages (replace only if uploaded)
                if (model.ProductImage1 != null && model.ProductImage1.Length > 0)
                    material.ProductImgURL1 = await _imageStorage.ReplaceAsync(model.ProductImage1, "materials", material.ProductImgURL1);

                if (model.ProductImage2 != null && model.ProductImage2.Length > 0)
                    material.ProductImgURL2 = await _imageStorage.ReplaceAsync(model.ProductImage2, "materials", material.ProductImgURL2);

                if (model.ProductImage3 != null && model.ProductImage3.Length > 0)
                    material.ProductImgURL3 = await _imageStorage.ReplaceAsync(model.ProductImage3, "materials", material.ProductImgURL3);

                // If there are orders: allow Price + Quantity (but quantity must increase)
                if (hasAnyOrders)
                {
                    // Price allowed
                    if (model.Price <= 0) return false;
                    material.Price = model.Price;

                    // Quantity allowed but MUST be greater than old quantity
                    if (model.Quantity < material.Quantity)
                        return false;

                    material.Quantity = model.Quantity;

                    await _db.SaveChangesAsync();
                    return true;
                }

                // No orders => Full edit allowed
                material.ProductType = model.ProductType?.Trim();
                material.Quantity = model.Quantity;
                material.Price = model.Price;

                material.Unit = string.IsNullOrWhiteSpace(model.Unit) ? "kg" : model.Unit.Trim();
                material.MinOrderQuantity = model.MinOrderQuantity;

                material.Status = model.Status;

                material.CancelWindowDays = model.CancelWindowDays;
                material.DeliveryDays = model.DeliveryDays;

                await _db.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating material");
                return false;
            }
        }

        // Get All Material Orders Details
        public async Task<List<MaterialOrderDetailsModel>> GetMaterialOrdersForFactoryAsync(int factoryId, int take = 200, CancellationToken ct = default)
        {
            return await (
                from o in _db.MaterialOrders.AsNoTracking()
                join m in _db.MaterialStores.AsNoTracking() on o.MaterialStoreID equals m.MaterialID
                join u in _db.Users.AsNoTracking() on o.BuyerID equals u.UserId
                where m.SellerID == factoryId
                orderby o.OrderDate descending
                select new MaterialOrderDetailsModel
                {
                    OrderId = o.MaterialOrderID,
                    MaterialId = o.MaterialStoreID,
                    ProductType = m.ProductType,

                    BuyerId = u.UserId,
                    BuyerName = u.FullName,

                    Quantity = o.Quantity,
                    UnitPrice = o.UnitPrice,
                    Status = o.Status,
                    OrderDate = o.OrderDate,
                    CancelUntil = o.CancelUntil,
                    ExpectedArrivalDate = o.ExpectedArrivalDate,
                    TotalPrice = o.TotalPrice,
                    DepositPaid = o.DepositPaid

                }
            ).Take(take).ToListAsync(ct);
        }

        // Change Status Order Material Object
        public async Task<ServiceResult> ChangeMaterialOrderStatusAsync(int factoryId, int orderId, EnumsOrderStatus newStatus, CancellationToken ct = default)
        {
            await using var tx = await _db.Database.BeginTransactionAsync(ct);

            try
            {
                var order = await _db.MaterialOrders
                    .Include(o => o.MaterialStore)
                        .ThenInclude(m => m.Seller)
                            .ThenInclude(s => s.FactoryProfile)
                    .FirstOrDefaultAsync(o => o.MaterialOrderID == orderId, ct);

                if (order == null || order.MaterialStore == null)
                    return ServiceResult.Fail("Order not found.");

                if (order.MaterialStore.SellerID != factoryId)
                    return ServiceResult.Fail("No permission.");

                var current = order.Status;

                bool isClosed(EnumsOrderStatus s) =>
                    s == EnumsOrderStatus.Cancelled
                    || s == EnumsOrderStatus.Completed
                    || s == EnumsOrderStatus.Refunded
                    || s == EnumsOrderStatus.Returned
                    || s == EnumsOrderStatus.DeletedByBuyer
                    || s == EnumsOrderStatus.DeletedBySeller;

                if (isClosed(current) && newStatus != current)
                    return ServiceResult.Fail($"Cannot change status because order is closed ({current}).");

                if (newStatus == EnumsOrderStatus.DeletedByBuyer)
                    return ServiceResult.Fail("Factory cannot set DeletedByBuyer.");

                if (newStatus == EnumsOrderStatus.Cancelled
                    || newStatus == EnumsOrderStatus.Refunded
                    || newStatus == EnumsOrderStatus.Returned)
                    return ServiceResult.Fail("This status requires a controlled flow (refund/return).");

                int Rank(EnumsOrderStatus s) => s switch
                {
                    EnumsOrderStatus.Pending => 1,
                    EnumsOrderStatus.Processing => 2,
                    EnumsOrderStatus.Confirmed => 3,
                    EnumsOrderStatus.Shipped => 4,
                    EnumsOrderStatus.ReadyForPickup => 5,
                    EnumsOrderStatus.PickedUp => 6,
                    EnumsOrderStatus.Delivered => 7,
                    EnumsOrderStatus.Completed => 8,
                    _ => 0
                };

                var curRank = Rank(current);
                var newRank = Rank(newStatus);

                if (newStatus == current)
                    return ServiceResult.Ok("No changes.");

                if (newRank == 0)
                    return ServiceResult.Fail("This status is not allowed in the normal flow.");

                if (curRank != 0 && newRank < curRank)
                    return ServiceResult.Fail("You cannot move order status backwards.");

                if (curRank != 0 && newRank != curRank + 1)
                    return ServiceResult.Fail("You must move status step by step.");

                var now = DateTime.UtcNow;

                if (newRank >= Rank(EnumsOrderStatus.Shipped))
                {
                    if (order.CancelUntil > now)
                        return ServiceResult.Fail("Cannot move to Shipped or beyond before CancelUntil ends.");
                }

                if (newStatus == EnumsOrderStatus.Completed)
                {
                    if (order.ExpectedArrivalDate > now)
                        return ServiceResult.Fail("Cannot complete order before ExpectedArrivalDate.");

                    var sellerId = order.MaterialStore.SellerID;
                    var factoryWallet = await GetOrCreateWalletAsync(sellerId, ct);

                    var held = Math.Round(order.DepositPaid, 2, MidpointRounding.AwayFromZero);
                    if (held <= 0m)
                        return ServiceResult.Fail("No held amount to settle for this order.");

                    if (factoryWallet.ReservedBalance + 0.0001m < held)
                        return ServiceResult.Fail("Factory wallet reserved balance is insufficient (hold missing).");

                    var pricing = _pricingOptions.Value;
                    var fee = Math.Round(held * pricing.PlatformFeePercent, 2, MidpointRounding.AwayFromZero);
                    if (fee > held) fee = held;

                    var net = Math.Round(held - fee, 2, MidpointRounding.AwayFromZero);

                    if (net > 0m)
                    {
                        await ReleaseHoldAsync(
                            wallet: factoryWallet,
                            amount: net,
                            note: $"Material completed: release net (orderId={order.MaterialOrderID})",
                            idemKey: $"MAT_REL:{order.MaterialOrderID}",
                            ct: ct
                        );
                    }

                    if (fee > 0m)
                    {
                        if (factoryWallet.ReservedBalance + 0.0001m < fee)
                            return ServiceResult.Fail("Factory reserved balance is insufficient to deduct platform fee.");

                        factoryWallet.ReservedBalance = Math.Round(factoryWallet.ReservedBalance - fee, 2, MidpointRounding.AwayFromZero);

                        await AddWalletTxnAsync(
                            wallet: factoryWallet,
                            type: WalletTxnType.Adjustment,
                            amount: -fee,
                            currency: "EGP",
                            note: $"Material completed: platform fee deducted (fee={fee:0.00}) orderId={order.MaterialOrderID}",
                            idempotencyKey: $"MAT_FEE_DEDUCT:{order.MaterialOrderID}",
                            ct: ct
                        );
                    }

                    var fp = order.MaterialStore.Seller?.FactoryProfile;
                    if (fp != null)
                    {
                        fp.TotalBalancePercentageRequests =
                            Math.Round((fp.TotalBalancePercentageRequests ?? 0m) + fee, 2, MidpointRounding.AwayFromZero);
                    }
                }

                order.Status = newStatus;

                await _db.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);

                // Send Emails
                var shouldSend =
                    newStatus == EnumsOrderStatus.Confirmed
                    || newStatus == EnumsOrderStatus.Shipped
                    || newStatus == EnumsOrderStatus.ReadyForPickup
                    || newStatus == EnumsOrderStatus.Delivered
                    || newStatus == EnumsOrderStatus.Completed;

                if (shouldSend)
                {
                    static string HtmlEncode(string? value)
                    {
                        if (string.IsNullOrEmpty(value)) return "";
                        return System.Net.WebUtility.HtmlEncode(value);
                    }

                    var buyerRow = await _db.Users
                        .AsNoTracking()
                        .Where(u => u.UserId == order.BuyerID)
                        .Select(u => new
                        {
                            EmailEncrypted = u.Email,
                            Name = (u.FullName ?? u.Email)
                        })
                        .FirstOrDefaultAsync(ct);

                    string buyerEmail = "";
                    string buyerName = buyerRow?.Name ?? "";

                    if (buyerRow != null && !string.IsNullOrWhiteSpace(buyerRow.EmailEncrypted))
                    {
                        try { buyerEmail = _dataCiphers.Decrypt(buyerRow.EmailEncrypted); }
                        catch { buyerEmail = ""; }
                    }

                    if (!string.IsNullOrWhiteSpace(buyerEmail))
                    {
                        var orderLink = $"{EmailTemplateConfig.WebsiteUrl}/CraftsMan/MaterialOrderDetails/{order.MaterialOrderID}";

                        var messageHtml = $@"
                            <p>Your order status has been updated ✅</p>

                            <div style='background:#f8f9fa;border:1px solid #eee;padding:16px;border-radius:10px;margin:16px 0;'>
                              <p style='margin:6px 0;'><strong>Order ID:</strong> #{order.MaterialOrderID}</p>
                              <p style='margin:6px 0;'><strong>New Status:</strong> {HtmlEncode(newStatus.ToString())}</p>
                              <p style='margin:6px 0;'><strong>Pickup Location:</strong> {HtmlEncode(order.PickupLocation)}</p>
                              <p style='margin:6px 0;'><strong>Expected Pickup/Arrival:</strong> {order.ExpectedArrivalDate:dddd, MMMM dd, yyyy - hh:mm tt} (UTC)</p>
                            </div>

                            <div style='background:#e3f2fd;border:1px solid #bbdefb;padding:16px;border-radius:10px;margin:16px 0;'>
                              <p style='margin:6px 0;'>Track details here:</p>
                              <p style='margin:6px 0;'><a href='{orderLink}' target='_blank'>{orderLink}</a></p>
                            </div>
                        ";
                        _ = Task.Run(async () =>
                        {
                            try
                            {
                                var mail = _emailTemplateService.CreateNotificationEmail(
                                    email: buyerEmail,
                                    subject: $"Order #{order.MaterialOrderID} Status: {newStatus}",
                                    message: messageHtml,
                                    userName: buyerName
                                );

                                await _emailTemplateService.SendEmailAsync(mail);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Order status email failed");
                            }
                        });
                    }
                }

                return ServiceResult.Ok($"Order #{orderId} status updated to {newStatus}.");
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync(ct);
                _logger.LogError(ex, "ChangeMaterialOrderStatusAsync failed");
                return ServiceResult.Fail("Failed to change order status.");
            }
        }

        // Delete Material Object From Store
        public async Task<ServiceResult> DeleteMaterialAsync(int factoryId, int materialId, CancellationToken ct = default)
        {
            try
            {
                var material = await _db.MaterialStores
                    .FirstOrDefaultAsync(m => m.MaterialID == materialId && m.SellerID == factoryId, ct);

                if (material == null)
                    return ServiceResult.Fail("Material not found or you don't have permission.");

                var activeStatuses = new[]
                {
                    EnumsOrderStatus.Pending,
                    EnumsOrderStatus.Processing,
                    EnumsOrderStatus.Confirmed,
                    EnumsOrderStatus.ReadyForPickup,
                    EnumsOrderStatus.Shipped,
                    EnumsOrderStatus.PickedUp,
                    EnumsOrderStatus.Delivered
                };

                var ordersQ = _db.MaterialOrders
                    .AsNoTracking()
                    .Where(o => o.MaterialStoreID == materialId);

                // If there are absolutely no requests => Delete static + Delete images
                var hasAnyOrders = await ordersQ.AnyAsync(ct);
                if (!hasAnyOrders)
                {
                    var urls = new[] { material.ProductImgURL1, material.ProductImgURL2, material.ProductImgURL3 }
                        .Where(x => !string.IsNullOrWhiteSpace(x))
                        .Select(x => x!.Trim())
                        .Distinct()
                        .ToList();

                    _db.MaterialStores.Remove(material);
                    await _db.SaveChangesAsync(ct);

                    foreach (var url in urls)
                    {
                        try { await _imageStorage.DeleteAsync(url); }
                        catch (Exception ex) { _logger.LogWarning(ex, "Failed to delete image url {Url}", url); }
                    }

                    return ServiceResult.Ok("Material deleted permanently (no orders).");
                }

                // There are Orders — check if there is an Active order?
                var hasActiveOrders = await ordersQ.AnyAsync(o => activeStatuses.Contains(o.Status), ct);

                // Active orders exist => set SOLD OUT and ZERO quantity
                if (hasActiveOrders)
                {
                    var reservedQty = await ordersQ
                        .Where(o => activeStatuses.Contains(o.Status))
                        .SumAsync(o => (int?)o.Quantity, ct) ?? 0;

                    material.Quantity = 0;
                    material.Status = ProductStatus.SoldOut;

                    await _db.SaveChangesAsync(ct);

                    return ServiceResult.Ok(
                        $"Material marked SoldOut and stock set to 0. Reserved in active orders: {reservedQty}."
                    );
                }

                // Orders exist but none active => soft hide
                material.Status = ProductStatus.RemovedByFactory;
                await _db.SaveChangesAsync(ct);

                return ServiceResult.Ok("Material removed from store (hidden). It can be restored if returns happen.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DeleteMaterialAsync (auto) failed");
                return ServiceResult.Fail("Failed to delete material.");
            }
        }

        ////////////////////////////////////////// Material Methods ////////////////////////////////////
        /////////////////////////////// User Method ///////////////////////////////

        // Get All Material objects with state Available from database to store
        public async Task<List<FactoryStoreModel>> GetPublicMaterialsAsync(SearchFilterModel? filter = null)
        {
            try
            {
                // Define which statuses are considered "active"
                var activeStatuses = new[]
                {
                    EnumsOrderStatus.Pending,
                    EnumsOrderStatus.Confirmed,
                    EnumsOrderStatus.Processing,
                    EnumsOrderStatus.ReadyForPickup,
                    EnumsOrderStatus.Shipped
                };

                // Excluded statuses from "OrdersCount"
                var excludedStatuses = new[]
                {
                    EnumsOrderStatus.Cancelled,
                    EnumsOrderStatus.DeletedByBuyer,
                    EnumsOrderStatus.DeletedBySeller
                };

                IQueryable<MaterialStore> q = _db.MaterialStores
                    .AsNoTracking()
                    .Include(x => x.Seller)
                        .ThenInclude(u => u.FactoryProfile)
                    .Where(x => x.Seller != null && x.Seller.Verified)
                    .Where(x => x.Status == ProductStatus.Available && x.Quantity > 0);

                if (filter != null)
                {
                    if (!string.IsNullOrWhiteSpace(filter.Keyword))
                    {
                        var kw = filter.Keyword.Trim();
                        q = q.Where(x =>
                            (x.ProductType != null && EF.Functions.Like(x.ProductType, $"%{kw}%")) ||
                            (x.Description != null && EF.Functions.Like(x.Description, $"%{kw}%")));
                    }

                    if (filter.MinPrice.HasValue)
                        q = q.Where(x => x.Price >= filter.MinPrice.Value);

                    if (filter.MaxPrice.HasValue)
                        q = q.Where(x => x.Price <= filter.MaxPrice.Value);
                }

                // Load material ids first (so we can group orders once)
                var baseRows = await q
                    .OrderByDescending(x => x.CreatedAt)
                    .Select(x => new
                    {
                        x.MaterialID,
                        x.ProductType,
                        x.Description,
                        x.Price,
                        x.Quantity,
                        x.Unit,
                        x.Status,
                        x.CreatedAt,
                        x.MinOrderQuantity,

                        x.ProductImgURL1,
                        x.ProductImgURL2,
                        x.ProductImgURL3,

                        // Listing pickup location stored on MaterialStore
                        ListingAddress = x.Address,
                        ListingLat = x.Latitude,
                        ListingLng = x.Longitude,

                        SellerUserId = x.Seller.UserId,
                        SellerVerified = x.Seller.Verified,
                        SellerName = x.Seller.FullName,
                        SellerAddress = x.Seller.Address,
                        SellerLat = x.Seller.Latitude,
                        SellerLng = x.Seller.Longitude,

                        FactoryName = x.Seller.FactoryProfile != null ? x.Seller.FactoryProfile.FactoryName : null,
                        FactoryImg1 = x.Seller.FactoryProfile != null ? x.Seller.FactoryProfile.FactoryImgURL1 : null,
                        FactoryImg2 = x.Seller.FactoryProfile != null ? x.Seller.FactoryProfile.FactoryImgURL2 : null,
                        FactoryImg3 = x.Seller.FactoryProfile != null ? x.Seller.FactoryProfile.FactoryImgURL3 : null
                    })
                    .ToListAsync();

                if (baseRows.Count == 0)
                    return new List<FactoryStoreModel>();

                var materialIds = baseRows.Select(r => r.MaterialID).Distinct().ToList();

                // Group orders counts in ONE query
                var ordersAgg = await _db.MaterialOrders
                    .AsNoTracking()
                    .Where(o => materialIds.Contains(o.MaterialStoreID))
                    .GroupBy(o => o.MaterialStoreID)
                    .Select(g => new
                    {
                        MaterialID = g.Key,
                        OrdersCount = g.Count(o => !excludedStatuses.Contains(o.Status)),
                        ActiveOrdersCount = g.Count(o => activeStatuses.Contains(o.Status))
                    })
                    .ToDictionaryAsync(x => x.MaterialID, x => new { x.OrdersCount, x.ActiveOrdersCount });

                // Map to Model
                return baseRows.Select(r =>
                {
                    var imgs = new[] { r.ProductImgURL1, r.ProductImgURL2, r.ProductImgURL3 }
                        .Where(z => !string.IsNullOrWhiteSpace(z))
                        .Select(z => z!.Trim())
                        .Distinct()
                        .ToList();

                    var factoryImgs = new[] { r.FactoryImg1, r.FactoryImg2, r.FactoryImg3 }
                        .Where(z => !string.IsNullOrWhiteSpace(z))
                        .Select(z => z!.Trim())
                        .Distinct()
                        .ToList();

                    // Resolve listing pickup location: prefer listing fields, fallback to seller
                    var resolvedAddress = !string.IsNullOrWhiteSpace(r.ListingAddress) ? r.ListingAddress!.Trim() : r.SellerAddress;
                    var resolvedLat = r.ListingLat ?? r.SellerLat;
                    var resolvedLng = r.ListingLng ?? r.SellerLng;

                    var counts = ordersAgg.TryGetValue(r.MaterialID, out var c) ? c : new { OrdersCount = 0, ActiveOrdersCount = 0 };

                    return new FactoryStoreModel
                    {
                        Id = r.MaterialID,
                        Type = "Material",

                        ProductType = r.ProductType,
                        Name = r.ProductType ?? "Material",

                        Description = r.Description,
                        Price = r.Price,
                        AvailableQuantity = r.Quantity,
                        Unit = r.Unit,
                        Status = r.Status.ToString(),
                        CreatedAt = r.CreatedAt,
                        MinOrderQuantity = r.MinOrderQuantity,

                        ImageUrl = imgs.FirstOrDefault(),
                        ImageUrls = imgs,

                        SellerUserId = r.SellerUserId,
                        SellerName = !string.IsNullOrWhiteSpace(r.FactoryName) ? r.FactoryName : r.SellerName,
                        IsVerifiedSeller = r.SellerVerified,

                        // Seller location (profile)
                        SellerAddress = r.SellerAddress,
                        SellerLatitude = r.SellerLat,
                        SellerLongitude = r.SellerLng,

                        // Listing/Factory address for display (pickup)
                        FactoryName = r.FactoryName,
                        FactoryImageUrls = factoryImgs,
                        FactoryAddress = resolvedAddress,
                        FactoryLatitude = resolvedLat,
                        FactoryLongitude = resolvedLng,

                        OrdersCount = counts.OrdersCount,
                        ActiveOrdersCount = counts.ActiveOrdersCount
                    };
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetPublicMaterialsAsync failed");
                return new List<FactoryStoreModel>();
            }
        }

        // Get Details about Material object by id 
        public async Task<FactoryStoreModel?> GetPublicMaterialDetailsAsync(int materialId)
        {
            try
            {
                var row = await _db.MaterialStores
                    .AsNoTracking()
                    .Include(m => m.Seller)
                        .ThenInclude(u => u.FactoryProfile)
                    .Where(m =>
                        m.MaterialID == materialId &&
                        m.Seller != null &&
                        m.Seller.Verified
                    )
                    .Select(m => new
                    {
                        // Material
                        m.MaterialID,
                        m.ProductType,
                        m.Description,
                        m.Price,
                        m.Quantity,
                        m.Unit,
                        m.Status,
                        m.CreatedAt,
                        m.MinOrderQuantity,
                        m.CancelWindowDays,
                        m.DeliveryDays,
                        m.ProductImgURL1,
                        m.ProductImgURL2,
                        m.ProductImgURL3,

                        // ✅ Material Pickup Location (stored on MaterialStore)
                        MaterialPickupAddress = m.Address,
                        MaterialLat = m.Latitude,
                        MaterialLng = m.Longitude,

                        // Seller/Factory User
                        SellerUserId = m.Seller.UserId,
                        SellerOwnerName = m.Seller.FullName,
                        SellerVerified = m.Seller.Verified,
                        SellerProfileImg = m.Seller.UserProfileImgURL,

                        // ✅ Factory location from Seller User profile (Factory default)
                        FactoryAddress = m.Seller.Address,
                        FactoryLat = m.Seller.Latitude,
                        FactoryLng = m.Seller.Longitude,

                        // Factory profile
                        FactoryName = m.Seller.FactoryProfile != null ? m.Seller.FactoryProfile.FactoryName : null,
                        FactoryImg1 = m.Seller.FactoryProfile != null ? m.Seller.FactoryProfile.FactoryImgURL1 : null,
                        FactoryImg2 = m.Seller.FactoryProfile != null ? m.Seller.FactoryProfile.FactoryImgURL2 : null,
                        FactoryImg3 = m.Seller.FactoryProfile != null ? m.Seller.FactoryProfile.FactoryImgURL3 : null,

                        OrdersCount = _db.MaterialOrders.Count(o =>
                            o.MaterialStoreID == m.MaterialID &&
                            o.Status != EnumsOrderStatus.Cancelled &&
                            o.Status != EnumsOrderStatus.DeletedByBuyer &&
                            o.Status != EnumsOrderStatus.DeletedBySeller
                        )
                    })
                    .FirstOrDefaultAsync();

                if (row == null) return null;

                var imgs = new[] { row.ProductImgURL1, row.ProductImgURL2, row.ProductImgURL3 }
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => x!.Trim())
                    .Distinct()
                    .ToList();

                var factoryImgs = new[] { row.FactoryImg1, row.FactoryImg2, row.FactoryImg3 }
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => x!.Trim())
                    .Distinct()
                    .ToList();

                var today = DateTime.UtcNow;
                var previewArrival = today.AddDays(Math.Max(0, row.DeliveryDays));

                return new FactoryStoreModel
                {
                    Id = row.MaterialID,
                    Type = "Material",

                    ProductType = row.ProductType,
                    Name = row.ProductType,
                    Description = row.Description,

                    Price = row.Price,
                    AvailableQuantity = row.Quantity,
                    Unit = row.Unit,
                    Status = row.Status.ToString(),
                    CreatedAt = row.CreatedAt,
                    MinOrderQuantity = row.MinOrderQuantity,

                    ImageUrl = imgs.FirstOrDefault(),
                    ImageUrls = imgs,

                    SellerUserId = row.SellerUserId,

                    // Seller name shown as factory name if exists
                    SellerName = !string.IsNullOrWhiteSpace(row.FactoryName) ? row.FactoryName : row.SellerOwnerName,
                    IsVerifiedSeller = row.SellerVerified,
                    SellerProfileImgUrl = row.SellerProfileImg,

                    // ✅ Factory location (Seller profile)
                    FactoryName = row.FactoryName,
                    FactoryAddress = row.FactoryAddress,
                    FactoryLatitude = row.FactoryLat,
                    FactoryLongitude = row.FactoryLng,
                    FactoryImageUrls = factoryImgs,

                    // ✅ Material Pickup location
                    MaterialPickupAddress = row.MaterialPickupAddress,
                    MaterialLatitude = row.MaterialLat,
                    MaterialLongitude = row.MaterialLng,

                    OrdersCount = row.OrdersCount,
                    ExpectedArrivalDate = previewArrival
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetPublicMaterialDetailsAsync failed");
                return null;
            }
        }

        // Book Order Material
        public async Task<ServiceResult> PlaceMaterialOrderAsync(int buyerId, int materialId, int quantity, decimal paidAmount, decimal walletUsed, string provider, string providerPaymentId, CancellationToken ct = default)
        {
            if (quantity <= 0) return ServiceResult.Fail("Invalid quantity.");

            paidAmount = Math.Round(paidAmount, 2, MidpointRounding.AwayFromZero);
            walletUsed = Math.Round(walletUsed, 2, MidpointRounding.AwayFromZero);

            if (paidAmount <= 0m) return ServiceResult.Fail("Paid amount is invalid.");
            if (walletUsed > paidAmount) return ServiceResult.Fail("WalletUsed cannot exceed PaidAmount.");

            try
            {
                // Payment idempotency
                if (!string.IsNullOrWhiteSpace(providerPaymentId))
                {
                    var exists = await _db.PaymentTransactions
                        .AsNoTracking()
                        .AnyAsync(p => p.Provider == provider && p.ProviderPaymentId == providerPaymentId, ct);

                    if (exists) return ServiceResult.Fail("Payment already processed.");
                }

                var material = await _db.MaterialStores
                    .Include(m => m.Seller)
                        .ThenInclude(u => u.FactoryProfile)
                    .FirstOrDefaultAsync(m =>
                        m.MaterialID == materialId &&
                        m.Status == ProductStatus.Available &&
                        m.Quantity > 0 &&
                        m.Seller != null &&
                        m.Seller.Verified, ct);

                if (material == null) return ServiceResult.Fail("Material not available.");
                if (material.SellerID == buyerId) return ServiceResult.Fail("You can't order your own listing.");

                var min = MinQtyToInt(material.MinOrderQuantity);
                if (quantity < min) return ServiceResult.Fail($"Minimum order quantity is {min}.");

                await using var tx = await _db.Database.BeginTransactionAsync(ct);

                // reload for concurrency
                await _db.Entry(material).ReloadAsync(ct);
                if (material.Quantity < quantity) return ServiceResult.Fail("Not enough quantity available.");

                var unitPrice = Math.Round(material.Price, 2, MidpointRounding.AwayFromZero);
                var totalPrice = Math.Round(unitPrice * quantity, 2, MidpointRounding.AwayFromZero);

                if (paidAmount > totalPrice + 0.0001m)
                    return ServiceResult.Fail("Paid amount exceeds total price.");

                var pricing = _pricingOptions.Value;

                var depositRequired = Math.Round(totalPrice * pricing.DepositPercent, 2, MidpointRounding.AwayFromZero);
                if (paidAmount + 0.0001m < depositRequired)
                    return ServiceResult.Fail($"Minimum deposit is {depositRequired:N2} EGP.");

                // Calculate platform fees internally
                var platformFeeCalc = Math.Round(paidAmount * pricing.PlatformFeePercent, 2, MidpointRounding.AwayFromZero);
                if (platformFeeCalc > paidAmount) platformFeeCalc = paidAmount;

                // Buyer wallet debit (if used)
                long? buyerWalletTxnId = null;
                if (walletUsed > 0m)
                {
                    var buyerWallet = await GetOrCreateWalletAsync(buyerId, ct);

                    var availableBuyer = Math.Round(buyerWallet.Balance - buyerWallet.ReservedBalance, 2, MidpointRounding.AwayFromZero);
                    if (availableBuyer + 0.0001m < walletUsed)
                        return ServiceResult.Fail("Insufficient available wallet balance.");

                    var wtxn = await AddWalletTxnAsync(
                        wallet: buyerWallet,
                        type: WalletTxnType.PaymentDebit,
                        amount: -walletUsed,
                        currency: "EGP",
                        note: $"Material order wallet payment (materialId={materialId})",
                        idempotencyKey: $"BUYER_PAY:{provider}:{providerPaymentId}",
                        ct: ct
                    );

                    buyerWalletTxnId = wtxn.Id;
                }

                // Reduce stock
                material.Quantity -= quantity;
                if (material.Quantity == 0) material.Status = ProductStatus.SoldOut;

                var now = DateTime.UtcNow;

                // Create Order (Pending)
                var order = new MaterialOrder
                {
                    MaterialStoreID = material.MaterialID,
                    BuyerID = buyerId,
                    Status = EnumsOrderStatus.Pending,
                    OrderDate = now,

                    Quantity = quantity,
                    UnitPrice = unitPrice,
                    TotalPrice = totalPrice,

                    DepositPaid = paidAmount,

                    CancelUntil = now.AddDays(Math.Max(0, material.CancelWindowDays)),
                    ExpectedArrivalDate = now.AddDays(Math.Max(0, material.DeliveryDays)),
                    Latitude = material.Latitude,
                    Longitude = material.Longitude,
                    PickupLocation = material.Address
                };

                _db.MaterialOrders.Add(order);
                await _db.SaveChangesAsync(ct);

                // Factory Wallet HOLD
                var sellerId = material.SellerID;
                var factoryWallet = await GetOrCreateWalletAsync(sellerId, ct);

                await HoldAsync(
                    wallet: factoryWallet,
                    amount: paidAmount,
                    note: $"Order HOLD (orderId={order.MaterialOrderID})",
                    idemKey: $"HOLD:{provider}:{providerPaymentId}",
                    ct: ct
                );

                // Update FactoryProfile totals
                var factoryProfile = material.Seller?.FactoryProfile;
                if (factoryProfile != null)
                {
                    factoryProfile.TotalBalanceOrderWaiting =
                        Math.Round((factoryProfile.TotalBalanceOrderWaiting ?? 0m) + paidAmount, 2, MidpointRounding.AwayFromZero);

                    factoryProfile.TotalBalancePercentageRequests =
                        Math.Round((factoryProfile.TotalBalancePercentageRequests ?? 0m) + platformFeeCalc, 2, MidpointRounding.AwayFromZero);
                }

                // PaymentTransaction record
                _db.PaymentTransactions.Add(new PaymentTransaction
                {
                    UserId = buyerId,
                    Provider = provider,
                    ProviderPaymentId = providerPaymentId,
                    Amount = paidAmount,
                    Currency = "EGP",
                    Status = PaymentStatus.Succeeded,
                    WalletTransactionId = buyerWalletTxnId,
                    CreatedAt = now
                });

                await _db.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);

                // Send Email
                var buyerRow = await _db.Users
                    .AsNoTracking()
                    .Where(u => u.UserId == buyerId)
                    .Select(u => new
                    {
                        EmailEncrypted = u.Email,
                        Name = (u.FullName ?? u.Email)
                    })
                    .FirstOrDefaultAsync(ct);

                string buyerEmail = "";
                string buyerName = buyerRow?.Name ?? "";

                if (buyerRow != null && !string.IsNullOrWhiteSpace(buyerRow.EmailEncrypted))
                {
                    try { buyerEmail = _dataCiphers.Decrypt(buyerRow.EmailEncrypted); }
                    catch { buyerEmail = ""; }
                }

                var imgRow = await _db.MaterialStores
                    .AsNoTracking()
                    .Where(m => m.MaterialID == material.MaterialID)
                    .Select(m => new { m.ProductImgURL1, m.ProductImgURL2, m.ProductImgURL3 })
                    .FirstOrDefaultAsync(ct);

                var imageUrls = new List<string?> { imgRow?.ProductImgURL1, imgRow?.ProductImgURL2, imgRow?.ProductImgURL3 }
                    .Where(u => !string.IsNullOrWhiteSpace(u))
                    .Select(u => u!)
                    .Distinct()
                    .ToList();

                var imagesHtml = imageUrls.Any()
                    ? "<div style='margin:10px 0;'>" + string.Join("",imageUrls.Select(u =>
                        $@"<a href='{u}' target='_blank' style='text-decoration:none;'>
               <img src='{EcoRecyclersGreenTech.Services.EmailTemplateConfig.WebsiteUrl}/{u}' alt='Material Image'
                    style='width:110px;height:110px;object-fit:cover;border-radius:10px;border:1px solid #eee;margin:6px;' />
                    </a>"
                      )) + "</div>"
                    : "<p style='color:#6c757d;margin:10px 0;'>No images available for this material.</p>";

                static string HtmlEncode(string? value)
                {
                    if (string.IsNullOrEmpty(value)) return "";
                    return System.Net.WebUtility.HtmlEncode(value);
                }

                var remaining = Math.Round(totalPrice - paidAmount, 2, MidpointRounding.AwayFromZero);
                if (remaining < 0m) remaining = 0m;

                var factoryName =
                    material.Seller?.FactoryProfile?.FactoryName
                    ?? material.Seller?.FullName
                    ?? EmailTemplateConfig.CompanyName;

                var factoryLocation = EmailTemplateConfig.SupportLocation;

                var orderLink = $"{EmailTemplateConfig.WebsiteUrl}/CraftsMan/MaterialOrderDetails/{order.MaterialOrderID}";
                var materialLink = $"{EmailTemplateConfig.WebsiteUrl}/CraftsMan/MaterialDetails/{material.MaterialID}";

                var messageHtml = $@"
                    <p>Your order has been created successfully ✅</p>

                    <div style='background:#f8f9fa;border:1px solid #eee;padding:16px;border-radius:10px;margin:16px 0;'>
                      <h3 style='margin:0 0 10px 0;'>🧾 Order Summary</h3>
                      <p style='margin:6px 0;'><strong>Order ID:</strong> #{order.MaterialOrderID}</p>
                      <p style='margin:6px 0;'><strong>Status:</strong> {HtmlEncode(order.Status.ToString())}</p>
                      <p style='margin:6px 0;'><strong>Material Type:</strong> {HtmlEncode(material.ProductType)}</p>
                      <p style='margin:6px 0;'><strong>Quantity:</strong> {order.Quantity}</p>
                      <p style='margin:6px 0;'><strong>Unit Price:</strong> {unitPrice:N2} EGP</p>
                      <p style='margin:6px 0;'><strong>Total Price:</strong> {totalPrice:N2} EGP</p>
                      <p style='margin:6px 0;'><strong>Paid Now:</strong> {paidAmount:N2} EGP</p>
                      <p style='margin:6px 0;'><strong>Remaining:</strong> {remaining:N2} EGP</p>
                    </div>

                    <div style='background:#fff;border:1px solid #eee;padding:16px;border-radius:10px;margin:16px 0;'>
                      <h3 style='margin:0 0 10px 0;'>📍 Pickup Details</h3>
                      <p style='margin:6px 0;'><strong>Pickup Location:</strong> {HtmlEncode(order.PickupLocation)}</p>
                      <p style='margin:6px 0;'><strong>Factory:</strong> {HtmlEncode(factoryName)}</p>
                      <p style='margin:6px 0;'><strong>Factory Location:</strong> {HtmlEncode(factoryLocation)}</p>
                      <p style='margin:6px 0;'><strong>Expected Pickup/Arrival:</strong> {order.ExpectedArrivalDate:dddd, MMMM dd, yyyy - hh:mm tt} (UTC)</p>
                      <p style='margin:6px 0;'><strong>Cancel Window Ends:</strong> {order.CancelUntil:dddd, MMMM dd, yyyy - hh:mm tt} (UTC)</p>
                    </div>

                    <div style='background:#fff;border:1px solid #eee;padding:16px;border-radius:10px;margin:16px 0;'>
                      <h3 style='margin:0 0 10px 0;'>🖼️ Material Images</h3>
                      {imagesHtml}
                      <p style='margin:10px 0 0 0;'><a href='{materialLink}' target='_blank'>Open material page</a></p>
                    </div>

                    <div style='background:#e8f5e9;border:1px solid #c8e6c9;padding:16px;border-radius:10px;margin:16px 0;'>
                      <h3 style='margin:0 0 10px 0;'>🔎 Track your order</h3>
                      <p style='margin:6px 0;'>Follow updates from here:</p>
                      <p style='margin:6px 0;'><a href='{orderLink}' target='_blank'>{orderLink}</a></p>
                    </div>
                ";

                if (!string.IsNullOrWhiteSpace(buyerEmail))
                {
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            var mail = _emailTemplateService.CreateNotificationEmail(
                                email: buyerEmail,
                                subject: $"Material Order #{order.MaterialOrderID} Created",
                                message: messageHtml,
                                userName: buyerName
                            );

                            await _emailTemplateService.SendEmailAsync(mail);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Order created email failed");
                        }
                    });
                }
                return ServiceResult.Ok(
                    $"Order created. Paid {paidAmount:N2} of {totalPrice:N2} EGP.",
                    order.MaterialOrderID
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PlaceMaterialOrderAsync failed");
                return ServiceResult.Fail("Failed to create material order.");
            }
        }

        // Get Material Details before Book
        public async Task<FactoryStoreModel?> GetMaterialDetailsForBuyerAsync(int materialId, int buyerId)
        {
            try
            {
                var row = await (
                    from m in _db.MaterialStores.AsNoTracking()
                    join u in _db.Users.AsNoTracking().Include(x => x.FactoryProfile) on m.SellerID equals u.UserId
                    where m.MaterialID == materialId
                    where u.Verified
                    where (m.Status == ProductStatus.Available)
                        || _db.MaterialOrders.Any(o => o.MaterialStoreID == materialId && o.BuyerID == buyerId)
                    select new
                    {
                        m.MaterialID,
                        m.ProductType,
                        m.Description,
                        m.Price,
                        m.Quantity,
                        m.Unit,
                        m.Status,
                        m.CreatedAt,
                        m.MinOrderQuantity,
                        m.ProductImgURL1,
                        m.ProductImgURL2,
                        m.ProductImgURL3,

                        SellerUserId = u.UserId,
                        SellerName = u.FullName,
                        SellerVerified = u.Verified,
                        SellerProfileImg = u.UserProfileImgURL,
                        SellerLat = u.Latitude,
                        SellerLng = u.Longitude,
                        SellerAddress = u.Address,

                        FactoryName = u.FactoryProfile != null ? u.FactoryProfile.FactoryName : null,
                        FactoryImg1 = u.FactoryProfile != null ? u.FactoryProfile.FactoryImgURL1 : null,
                        FactoryImg2 = u.FactoryProfile != null ? u.FactoryProfile.FactoryImgURL2 : null,
                        FactoryImg3 = u.FactoryProfile != null ? u.FactoryProfile.FactoryImgURL3 : null,

                        OrdersCount = _db.MaterialOrders.Count(o => o.MaterialStoreID == m.MaterialID),

                        MyOrder = _db.MaterialOrders
                            .Where(o => o.MaterialStoreID == materialId && o.BuyerID == buyerId)
                            .OrderByDescending(o => o.OrderDate)
                            .Select(o => new
                            {
                                o.MaterialOrderID,
                                o.Status,
                                o.OrderDate,
                                o.Quantity,
                                o.CancelUntil,
                                o.ExpectedArrivalDate,
                                o.PickupLocation,
                                o.TotalPrice,
                                o.DepositPaid
                            })
                            .FirstOrDefault()
                    }
                ).FirstOrDefaultAsync();

                if (row == null) return null;

                var imgs = new[] { row.ProductImgURL1, row.ProductImgURL2, row.ProductImgURL3 }
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => x!.Trim())
                    .Distinct()
                    .ToList();

                var factoryImgs = new[] { row.FactoryImg1, row.FactoryImg2, row.FactoryImg3 }
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => x!.Trim())
                    .Distinct()
                    .ToList();

                bool canCancel = false;
                if (row.MyOrder != null)
                {
                    var st = row.MyOrder.Status;
                    canCancel = (st == EnumsOrderStatus.Pending ||
                                 st == EnumsOrderStatus.Confirmed ||
                                 st == EnumsOrderStatus.Processing)
                        && DateTime.UtcNow <= row.MyOrder.CancelUntil;
                }

                return new FactoryStoreModel
                {
                    Id = row.MaterialID,
                    Type = "Material",
                    ProductType = row.ProductType,
                    Name = row.ProductType,
                    Description = row.Description,

                    Price = row.Price,
                    AvailableQuantity = row.Quantity,
                    Unit = row.Unit,
                    Status = row.Status.ToString(),
                    CreatedAt = row.CreatedAt,
                    MinOrderQuantity = row.MinOrderQuantity,

                    ImageUrl = imgs.FirstOrDefault(),
                    ImageUrls = imgs,

                    SellerUserId = row.SellerUserId,
                    SellerName = !string.IsNullOrWhiteSpace(row.FactoryName) ? row.FactoryName : row.SellerName,
                    IsVerifiedSeller = row.SellerVerified,
                    SellerProfileImgUrl = row.SellerProfileImg,
                    SellerLatitude = row.SellerLat,
                    SellerLongitude = row.SellerLng,
                    SellerAddress = row.SellerAddress,

                    FactoryName = row.FactoryName,
                    FactoryImageUrls = factoryImgs,

                    FactoryAddress = row.MyOrder?.PickupLocation ?? row.SellerAddress,
                    OrdersCount = row.OrdersCount,

                    MyOrderId = row.MyOrder?.MaterialOrderID,
                    MyOrderStatus = row.MyOrder?.Status.ToString(),
                    MyOrderDate = row.MyOrder?.OrderDate,
                    MyOrderQuantity = row.MyOrder?.Quantity,
                    CancelUntil = row.MyOrder?.CancelUntil,
                    ExpectedArrivalDate = row.MyOrder?.ExpectedArrivalDate,
                    CanCancel = canCancel
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetMaterialDetailsForBuyerAsync failed");
                return null;
            }
        }

        // Get Details about order after Book
        public async Task<(MaterialOrder? order, FactoryStoreModel? details)> GetMaterialOrderDetailsAsync(int buyerId, int orderId)
        {
            var order = await _db.MaterialOrders
                .AsNoTracking()
                .Include(o => o.MaterialStore).ThenInclude(m => m.Seller).ThenInclude(u => u.FactoryProfile)
                .FirstOrDefaultAsync(o => o.MaterialOrderID == orderId && o.BuyerID == buyerId);

            if (order == null) return (null, null);

            var details = await GetMaterialDetailsForBuyerAsync(order.MaterialStoreID, buyerId);
            return (order, details);
        }

        // Cancel Order
        public async Task<ServiceResult> CancelMaterialOrderAsync(int buyerId, int orderId, CancellationToken ct = default)
        {
            try
            {
                var order = await _db.MaterialOrders
                    .Include(o => o.MaterialStore)
                        .ThenInclude(m => m.Seller)
                            .ThenInclude(u => u.FactoryProfile)
                    .FirstOrDefaultAsync(o => o.MaterialOrderID == orderId && o.BuyerID == buyerId, ct);

                if (order == null) return ServiceResult.Fail("Order not found.");
                if (order.Status == EnumsOrderStatus.Cancelled) return ServiceResult.Fail("Order already cancelled.");
                if (!(order.Status == EnumsOrderStatus.Pending || order.Status == EnumsOrderStatus.Confirmed || order.Status == EnumsOrderStatus.Processing)) return ServiceResult.Fail("You can’t cancel this order at its current status.");
                if (DateTime.UtcNow > order.CancelUntil) return ServiceResult.Fail("Cancel window has expired.");
                if (order.MaterialStore == null || order.MaterialStore.Seller == null) return ServiceResult.Fail("Listing/Seller not found.");

                var refundAmount = Math.Round(order.DepositPaid, 2, MidpointRounding.AwayFromZero);

                // Compute fee for pending tracking removal
                var pricing = _pricingOptions.Value;
                var platformFee = Math.Round(refundAmount * pricing.PlatformFeePercent, 2, MidpointRounding.AwayFromZero);
                if (platformFee > refundAmount) platformFee = refundAmount;

                await using var tx = await _db.Database.BeginTransactionAsync(ct);

                // Restore inventory
                order.MaterialStore.Quantity += order.Quantity;
                if (order.MaterialStore.Quantity > 0 && order.MaterialStore.Status != ProductStatus.Available)
                    order.MaterialStore.Status = ProductStatus.Available;

                // Refund: factory RESERVED -> buyer wallet
                var sellerId = order.MaterialStore.SellerID;
                var factoryWallet = await GetOrCreateWalletAsync(sellerId, ct);
                var buyerWallet = await GetOrCreateWalletAsync(buyerId, ct);

                if (factoryWallet.ReservedBalance + 0.0001m < refundAmount)
                    return ServiceResult.Fail("Factory reserved balance is insufficient to refund (hold missing).");

                await TransferAsync(
                    from: factoryWallet,
                    to: buyerWallet,
                    amount: refundAmount,
                    consumeFromReserved: true,
                    note: $"Refund cancelled orderId={orderId}",
                    idemKey: $"REF:{orderId}",
                    ct: ct
                );

                // Update factory totals (remove pending + cancel fee)
                var factoryProfile = order.MaterialStore.Seller.FactoryProfile;
                if (factoryProfile != null)
                {
                    factoryProfile.TotalBalanceOrderWaiting =
                        Math.Round((factoryProfile.TotalBalanceOrderWaiting ?? 0m) - refundAmount, 2, MidpointRounding.AwayFromZero);

                    factoryProfile.TotalBalancePercentageRequests =
                        Math.Round((factoryProfile.TotalBalancePercentageRequests ?? 0m) - platformFee, 2, MidpointRounding.AwayFromZero);

                    if (factoryProfile.TotalBalanceOrderWaiting < 0m) factoryProfile.TotalBalanceOrderWaiting = 0m;
                    if (factoryProfile.TotalBalancePercentageRequests < 0m) factoryProfile.TotalBalancePercentageRequests = 0m;
                }

                // Cancel order
                order.Status = EnumsOrderStatus.Cancelled;

                await _db.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);

                return ServiceResult.Ok("Order cancelled successfully. Funds refunded to your wallet and platform fee cancelled.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CancelMaterialOrderAsync failed");
                return ServiceResult.Fail("Failed to cancel order.");
            }
        }

        // Hide Order after Canceled from my list
        public async Task<ServiceResult> HideMaterialOrderForBuyerAsync(int buyerId, int orderId, CancellationToken ct = default)
        {
            try
            {
                var order = await _db.MaterialOrders
                    .FirstOrDefaultAsync(o => o.MaterialOrderID == orderId && o.BuyerID == buyerId, ct);

                if (order == null) return ServiceResult.Fail("Order not found.");

                // Only allow hide if status is Cancelled OR DeletedBySeller
                if (order.Status != EnumsOrderStatus.Cancelled && order.Status != EnumsOrderStatus.DeletedBySeller)
                    return ServiceResult.Fail("You can remove this order only if it is Cancelled or Deleted by Seller.");

                // If seller already deleted, remove permanently
                if (order.Status == EnumsOrderStatus.DeletedBySeller)
                {
                    _db.MaterialOrders.Remove(order);
                    await _db.SaveChangesAsync(ct);
                    return ServiceResult.Ok("Order deleted permanently.");
                }

                // If cancelled -> hide for buyer (soft delete)
                order.Status = EnumsOrderStatus.DeletedByBuyer;
                await _db.SaveChangesAsync(ct);

                return ServiceResult.Ok("Order removed from your list.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "HideMaterialOrderForBuyerAsync failed");
                return ServiceResult.Fail("Failed to delete order.");
            }
        }

        // Get All order Material by user
        public async Task<List<MaterialOrder>> GetMaterialOrdersForBuyerAsync(int buyerId)
        {
            return await _db.MaterialOrders
                .AsNoTracking()
                .Include(o => o.MaterialStore).ThenInclude(m => m.Seller)
                .Where(o => o.BuyerID == buyerId
                    && o.Status != EnumsOrderStatus.DeletedByBuyer) // VisibleForBuyer
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        ////////////////////////////////////////// Machine Methods ////////////////////////////////////
        /////////////////////////////// Factory Method ///////////////////////////////

        // Get All Machine Object To Factory
        public async Task<List<FactoryStoreModel>> GetMachinesAsync(int factoryId, SearchFilterModel? filter = null)
        {
            var activeStatuses = new[]
            {
                EnumsOrderStatus.Pending,
                EnumsOrderStatus.Confirmed,
                EnumsOrderStatus.Processing,
                EnumsOrderStatus.ReadyForPickup,
                EnumsOrderStatus.Shipped
            };

            try
            {
                IQueryable<MachineStore> query = _db.MachineStores
                    .AsNoTracking()
                    .Where(m => m.SellerID == factoryId)
                    .Include(m => m.Seller);

                if (filter != null)
                {
                    if (!string.IsNullOrWhiteSpace(filter.Keyword))
                    {
                        var keyword = filter.Keyword.Trim();
                        query = query.Where(m =>
                            (m.MachineType != null && EF.Functions.Like(m.MachineType, $"%{keyword}%")) ||
                            (m.Description != null && EF.Functions.Like(m.Description, $"%{keyword}%")) ||
                            (m.Brand != null && EF.Functions.Like(m.Brand, $"%{keyword}%")) ||
                            (m.Model != null && EF.Functions.Like(m.Model, $"%{keyword}%"))
                        );
                    }

                    if (!string.IsNullOrWhiteSpace(filter.Status))
                    {
                        if (Enum.TryParse<ProductStatus>(filter.Status.Trim(), true, out var statusEnum))
                            query = query.Where(m => m.Status == statusEnum);
                    }

                    if (filter.MinPrice.HasValue)
                        query = query.Where(m => m.Price >= filter.MinPrice.Value);

                    if (filter.MaxPrice.HasValue)
                        query = query.Where(m => m.Price <= filter.MaxPrice.Value);
                }

                var rows = await query
                    .OrderByDescending(m => m.CreatedAt)
                    .Select(m => new
                    {
                        Machine = m,
                        OrdersCount = _db.MachineOrders.Count(o => o.MachineStoreID == m.MachineID),
                        ActiveOrdersCount = _db.MachineOrders.Count(o =>
                            o.MachineStoreID == m.MachineID &&
                            activeStatuses.Contains(o.Status))
                    })
                    .ToListAsync();

                var machines = rows.Select(x =>
                {
                    var images = new[] { x.Machine.MachineImgURL1, x.Machine.MachineImgURL2, x.Machine.MachineImgURL3 }
                        .Where(url => !string.IsNullOrWhiteSpace(url))
                        .Select(url => url!.Trim())
                        .Distinct()
                        .ToList();

                    return new FactoryStoreModel
                    {
                        Id = x.Machine.MachineID,
                        Name = x.Machine.MachineType ?? "Machine",
                        Type = "Machine",

                        ImageUrl = images.FirstOrDefault(),
                        ImageUrls = images,

                        Price = x.Machine.Price,
                        AvailableQuantity = x.Machine.Quantity,
                        Unit = null,
                        Status = x.Machine.Status.ToString(),

                        SellerName = x.Machine.Seller?.FullName,
                        CreatedAt = x.Machine.CreatedAt,
                        IsVerifiedSeller = x.Machine.Seller?.Verified ?? false,

                        MachineCondition = x.Machine.Condition?.ToString(),
                        Brand = x.Machine.Brand,
                        Model = x.Machine.Model,
                        WarrantyMonths = x.Machine.WarrantyMonths,

                        OrdersCount = x.OrdersCount,
                        ActiveOrdersCount = x.ActiveOrdersCount
                    };
                }).ToList();

                return machines;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting machines");
                return new List<FactoryStoreModel>();
            }
        }

        // Get Machine Object Data by ID To Factory
        public async Task<MachineProductDetailsModel?> GetMachineByIdAsync(int id, int factoryId)
        {
            try
            {
                var machine = await _db.MachineStores
                    .AsNoTracking()
                    .FirstOrDefaultAsync(m => m.MachineID == id && m.SellerID == factoryId);

                if (machine == null) return null;

                return new MachineProductDetailsModel
                {
                    Id = machine.MachineID,
                    MachineType = machine.MachineType,
                    Quantity = machine.Quantity,
                    Description = machine.Description,
                    Price = machine.Price,
                    Status = machine.Status,
                    MinOrderQuantity = machine.MinOrderQuantity,
                    ManufactureDate = machine.ManufactureDate,

                    Condition = machine.Condition,
                    Brand = machine.Brand,
                    Model = machine.Model,
                    WarrantyMonths = machine.WarrantyMonths,

                    CurrentImageUrl1 = machine.MachineImgURL1,
                    CurrentImageUrl2 = machine.MachineImgURL2,
                    CurrentImageUrl3 = machine.MachineImgURL3,

                    // New fields (same as materials)
                    Address = machine.Address,
                    Latitude = machine.Latitude,
                    Longitude = machine.Longitude,

                    UseFactoryLocation =
                        string.IsNullOrWhiteSpace(machine.Address) &&
                        !(machine.Latitude.HasValue && machine.Longitude.HasValue),

                    CancelWindowDays = machine.CancelWindowDays,
                    DeliveryDays = machine.DeliveryDays
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting machine by ID");
                return null;
            }
        }

        // Add New Machine Object 
        public async Task<bool> AddMachineAsync(MachineProductDetailsModel model, int factoryUserId)
        {
            try
            {
                // Bring factory user location for default usage
                var factoryUser = await _db.Users.AsNoTracking()
                    .FirstOrDefaultAsync(u => u.UserId == factoryUserId);

                if (factoryUser == null)
                    return false;

                // Decide location source
                string? address = null;
                decimal? lat = null;
                decimal? lng = null;

                bool factoryHasLocation =
                    !string.IsNullOrWhiteSpace(factoryUser.Address) ||
                    (factoryUser.Latitude.HasValue && factoryUser.Longitude.HasValue);

                if (model.UseFactoryLocation)
                {
                    if (!factoryHasLocation) return false;

                    address = factoryUser.Address;
                    lat = factoryUser.Latitude;
                    lng = factoryUser.Longitude;
                }
                else
                {
                    bool hasAddress = !string.IsNullOrWhiteSpace(model.Address);
                    bool hasCoords = model.Latitude.HasValue && model.Longitude.HasValue;

                    if (!hasAddress && !hasCoords) return false;

                    address = model.Address?.Trim();
                    lat = model.Latitude;
                    lng = model.Longitude;
                }

                var entity = new MachineStore
                {
                    SellerID = factoryUserId,

                    MachineType = model.MachineType?.Trim(),
                    Quantity = model.Quantity,
                    Description = model.Description?.Trim(),
                    Price = model.Price,

                    Status = ProductStatus.Available,
                    MinOrderQuantity = model.MinOrderQuantity,
                    ManufactureDate = model.ManufactureDate,

                    Condition = model.Condition,
                    Brand = model.Brand?.Trim(),
                    Model = model.Model?.Trim(),
                    WarrantyMonths = model.WarrantyMonths,

                    CreatedAt = DateTime.UtcNow,

                    // Location fields
                    Address = address,
                    Latitude = lat,
                    Longitude = lng,

                    CancelWindowDays = model.CancelWindowDays,
                    DeliveryDays = model.DeliveryDays
                };

                // Upload images
                if (model.MachineImage1 != null && model.MachineImage1.Length > 0)
                    entity.MachineImgURL1 = await _imageStorage.UploadAsync(model.MachineImage1, "machines");

                if (model.MachineImage2 != null && model.MachineImage2.Length > 0)
                    entity.MachineImgURL2 = await _imageStorage.UploadAsync(model.MachineImage2, "machines");

                if (model.MachineImage3 != null && model.MachineImage3.Length > 0)
                    entity.MachineImgURL3 = await _imageStorage.UploadAsync(model.MachineImage3, "machines");

                _db.MachineStores.Add(entity);
                await _db.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding machine");
                return false;
            }
        }

        // Update Data Machine Object by Rules
        public async Task<bool> UpdateMachineAsync(int id, MachineProductDetailsModel model, int factoryId)
        {
            try
            {
                var machine = await _db.MachineStores
                    .FirstOrDefaultAsync(m => m.MachineID == id && m.SellerID == factoryId);

                if (machine == null) return false;

                // Get All Order about This Machine Object
                var hasAnyOrders = await _db.MachineOrders
                    .AsNoTracking()
                    .AnyAsync(o => o.MachineStoreID == id);

                // Allowed Always: Description + Images + Pickup Location + (Price, Quantity with rule when has orders)

                // Description
                machine.Description = model.Description?.Trim();

                // Pickup Location (Factory default OR custom)
                string? resolvedAddress = null;
                decimal? resolvedLat = null;
                decimal? resolvedLng = null;

                if (model.UseFactoryLocation)
                {
                    var factoryUser = await _db.Users
                        .AsNoTracking()
                        .FirstOrDefaultAsync(u => u.UserId == factoryId);

                    bool factoryHasLocation =
                        factoryUser != null &&
                        (
                            !string.IsNullOrWhiteSpace(factoryUser.Address) ||
                            (factoryUser.Latitude.HasValue && factoryUser.Longitude.HasValue)
                        );

                    if (!factoryHasLocation) return false;

                    resolvedAddress = factoryUser!.Address?.Trim();
                    resolvedLat = factoryUser.Latitude;
                    resolvedLng = factoryUser.Longitude;
                }
                else
                {
                    bool hasAddress = !string.IsNullOrWhiteSpace(model.Address);
                    bool hasCoords = model.Latitude.HasValue && model.Longitude.HasValue;

                    if (!hasAddress && !hasCoords) return false;

                    resolvedAddress = model.Address?.Trim();
                    resolvedLat = model.Latitude;
                    resolvedLng = model.Longitude;
                }

                machine.Address = resolvedAddress;
                machine.Latitude = resolvedLat;
                machine.Longitude = resolvedLng;

                // Images (replace only if uploaded)
                if (model.MachineImage1 != null && model.MachineImage1.Length > 0)
                    machine.MachineImgURL1 = await _imageStorage.ReplaceAsync(model.MachineImage1, "machines", machine.MachineImgURL1);

                if (model.MachineImage2 != null && model.MachineImage2.Length > 0)
                    machine.MachineImgURL2 = await _imageStorage.ReplaceAsync(model.MachineImage2, "machines", machine.MachineImgURL2);

                if (model.MachineImage3 != null && model.MachineImage3.Length > 0)
                    machine.MachineImgURL3 = await _imageStorage.ReplaceAsync(model.MachineImage3, "machines", machine.MachineImgURL3);

                // If there are orders: allow Price + Quantity (but quantity must increase)
                if (hasAnyOrders)
                {
                    // Price allowed
                    if (model.Price <= 0) return false;
                    machine.Price = model.Price;

                    // Quantity allowed but MUST be greater than old quantity
                    if (model.Quantity < machine.Quantity)
                        return false;

                    machine.Quantity = model.Quantity;

                    // keep these editable too if you want
                    machine.CancelWindowDays = model.CancelWindowDays;
                    machine.DeliveryDays = model.DeliveryDays;

                    await _db.SaveChangesAsync();
                    return true;
                }

                // No orders => Full edit allowed
                machine.MachineType = model.MachineType?.Trim();
                machine.Quantity = model.Quantity;
                machine.Price = model.Price;

                machine.MinOrderQuantity = model.MinOrderQuantity;
                machine.ManufactureDate = model.ManufactureDate;

                machine.Condition = model.Condition;
                machine.Brand = model.Brand?.Trim();
                machine.Model = model.Model?.Trim();
                machine.WarrantyMonths = model.WarrantyMonths;

                machine.Status = model.Status;

                machine.CancelWindowDays = model.CancelWindowDays;
                machine.DeliveryDays = model.DeliveryDays;

                await _db.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating machine");
                return false;
            }
        }

        // Get All Machine Orders Details
        public async Task<List<MachineOrderDetailsModel>> GetMachineOrdersForFactoryAsync(int factoryId, int take = 200, CancellationToken ct = default)
        {
            return await (
                from o in _db.MachineOrders.AsNoTracking()
                join m in _db.MachineStores.AsNoTracking() on o.MachineStoreID equals m.MachineID
                join u in _db.Users.AsNoTracking() on o.BuyerID equals u.UserId
                where m.SellerID == factoryId
                orderby o.OrderDate descending
                select new MachineOrderDetailsModel
                {
                    OrderId = o.MachineOrderID,
                    MachineId = o.MachineStoreID,

                    MachineType = m.MachineType,

                    BuyerId = u.UserId,
                    BuyerName = u.FullName!,

                    Quantity = o.Quantity,
                    UnitPrice = o.UnitPrice,
                    Status = o.Status,
                    OrderDate = o.OrderDate,
                    CancelUntil = o.CancelUntil,
                    ExpectedArrivalDate = o.ExpectedArrivalDate,

                    TotalPrice = o.TotalPrice,
                    DepositPaid = o.DepositPaid
                }
            ).Take(take).ToListAsync(ct);
        }

        // Change Status Order Machine Object
        public async Task<ServiceResult> ChangeMachineOrderStatusAsync(int factoryId, int orderId, EnumsOrderStatus newStatus, CancellationToken ct = default)
        {
            await using var tx = await _db.Database.BeginTransactionAsync(ct);

            try
            {
                var order = await _db.MachineOrders
                    .Include(o => o.MachineStore)
                        .ThenInclude(m => m.Seller)
                            .ThenInclude(s => s.FactoryProfile)
                    .FirstOrDefaultAsync(o => o.MachineOrderID == orderId, ct);

                if (order == null || order.MachineStore == null)
                    return ServiceResult.Fail("Order not found.");

                if (order.MachineStore.SellerID != factoryId)
                    return ServiceResult.Fail("No permission.");

                var current = order.Status;

                bool isClosed(EnumsOrderStatus s) =>
                    s == EnumsOrderStatus.Cancelled
                    || s == EnumsOrderStatus.Completed
                    || s == EnumsOrderStatus.Refunded
                    || s == EnumsOrderStatus.Returned
                    || s == EnumsOrderStatus.DeletedByBuyer
                    || s == EnumsOrderStatus.DeletedBySeller;

                if (isClosed(current) && newStatus != current)
                    return ServiceResult.Fail($"Cannot change status because order is closed ({current}).");

                if (newStatus == EnumsOrderStatus.DeletedByBuyer)
                    return ServiceResult.Fail("Factory cannot set DeletedByBuyer.");

                if (newStatus == EnumsOrderStatus.Cancelled
                    || newStatus == EnumsOrderStatus.Refunded
                    || newStatus == EnumsOrderStatus.Returned)
                    return ServiceResult.Fail("This status requires a controlled flow (refund/return).");

                // Status order rule
                // Pending,Processing,Confirmed,Shipped,ReadyForPickup,PickedUp,Delivered,Completed
                int Rank(EnumsOrderStatus s) => s switch
                {
                    EnumsOrderStatus.Pending => 1,
                    EnumsOrderStatus.Processing => 2,
                    EnumsOrderStatus.Confirmed => 3,
                    EnumsOrderStatus.Shipped => 4,
                    EnumsOrderStatus.ReadyForPickup => 5,
                    EnumsOrderStatus.PickedUp => 6,
                    EnumsOrderStatus.Delivered => 7,
                    EnumsOrderStatus.Completed => 8,
                    _ => 0
                };

                var curRank = Rank(current);
                var newRank = Rank(newStatus);

                if (newStatus == current)
                    return ServiceResult.Ok("No changes.");

                if (newRank == 0)
                    return ServiceResult.Fail("This status is not allowed in the normal flow.");

                if (curRank != 0 && newRank < curRank)
                    return ServiceResult.Fail("You cannot move order status backwards.");

                // strict step-by-step
                if (curRank != 0 && newRank != curRank + 1)
                    return ServiceResult.Fail("You must move status step by step.");

                // From Shipped onward: only if CancelUntil Ending
                var now = DateTime.UtcNow;

                if (newRank >= Rank(EnumsOrderStatus.Shipped))
                {
                    if (order.CancelUntil > now)
                        return ServiceResult.Fail("Cannot move to Shipped or beyond before CancelUntil ends.");
                }

                // Completed only if ExpectedArrivalDate Ending + settlement
                if (newStatus == EnumsOrderStatus.Completed)
                {
                    if (order.ExpectedArrivalDate > now)
                        return ServiceResult.Fail("Cannot complete order before ExpectedArrivalDate.");

                    var sellerId = order.MachineStore.SellerID;
                    var factoryWallet = await GetOrCreateWalletAsync(sellerId, ct);

                    var held = Math.Round(order.DepositPaid, 2, MidpointRounding.AwayFromZero);
                    if (held <= 0m)
                        return ServiceResult.Fail("No held amount to settle for this order.");

                    if (factoryWallet.ReservedBalance + 0.0001m < held)
                        return ServiceResult.Fail("Factory wallet reserved balance is insufficient (hold missing).");

                    var pricing = _pricingOptions.Value;
                    var fee = Math.Round(held * pricing.PlatformFeePercent, 2, MidpointRounding.AwayFromZero);
                    if (fee > held) fee = held;

                    var net = Math.Round(held - fee, 2, MidpointRounding.AwayFromZero);

                    if (net > 0m)
                    {
                        await ReleaseHoldAsync(
                            wallet: factoryWallet,
                            amount: net,
                            note: $"Machine completed: release net (orderId={order.MachineOrderID})",
                            idemKey: $"MAC_REL:{order.MachineOrderID}",
                            ct: ct
                        );
                    }

                    if (fee > 0m)
                    {
                        if (factoryWallet.ReservedBalance + 0.0001m < fee)
                            return ServiceResult.Fail("Factory reserved balance is insufficient to deduct platform fee.");

                        factoryWallet.ReservedBalance = Math.Round(factoryWallet.ReservedBalance - fee, 2, MidpointRounding.AwayFromZero);

                        await AddWalletTxnAsync(
                            wallet: factoryWallet,
                            type: WalletTxnType.Adjustment,
                            amount: -fee,
                            currency: "EGP",
                            note: $"Machine completed: platform fee deducted (fee={fee:0.00}) orderId={order.MachineOrderID}",
                            idempotencyKey: $"MAC_FEE_DEDUCT:{order.MachineOrderID}",
                            ct: ct
                        );
                    }

                    var fp = order.MachineStore.Seller?.FactoryProfile;
                    if (fp != null)
                    {
                        fp.TotalBalancePercentageRequests =
                            Math.Round((fp.TotalBalancePercentageRequests ?? 0m) + fee, 2, MidpointRounding.AwayFromZero);
                    }
                }

                // Update status
                order.Status = newStatus;

                await _db.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);

                // Send Email
                var shouldSend =
                    newStatus == EnumsOrderStatus.Confirmed
                    || newStatus == EnumsOrderStatus.Shipped
                    || newStatus == EnumsOrderStatus.ReadyForPickup
                    || newStatus == EnumsOrderStatus.Delivered
                    || newStatus == EnumsOrderStatus.Completed;

                if (shouldSend)
                {
                    static string HtmlEncode(string? value)
                    {
                        if (string.IsNullOrEmpty(value)) return "";
                        return System.Net.WebUtility.HtmlEncode(value);
                    }

                    var buyerRow = await _db.Users
                        .AsNoTracking()
                        .Where(u => u.UserId == order.BuyerID)
                        .Select(u => new
                        {
                            EmailEncrypted = u.Email,
                            Name = (u.FullName ?? u.Email)
                        })
                        .FirstOrDefaultAsync(ct);

                    string buyerEmail = "";
                    string buyerName = buyerRow?.Name ?? "";

                    if (buyerRow != null && !string.IsNullOrWhiteSpace(buyerRow.EmailEncrypted))
                    {
                        try { buyerEmail = _dataCiphers.Decrypt(buyerRow.EmailEncrypted); }
                        catch { buyerEmail = ""; }
                    }

                    if (!string.IsNullOrWhiteSpace(buyerEmail))
                    {
                        var orderLink = $"{EmailTemplateConfig.WebsiteUrl}/CraftsMan/MachineOrderDetails/{order.MachineOrderID}";

                        var messageHtml = $@"
                            <p>Your machine order status has been updated ✅</p>

                            <div style='background:#f8f9fa;border:1px solid #eee;padding:16px;border-radius:10px;margin:16px 0;'>
                              <p style='margin:6px 0;'><strong>Order ID:</strong> #{order.MachineOrderID}</p>
                              <p style='margin:6px 0;'><strong>New Status:</strong> {HtmlEncode(newStatus.ToString())}</p>
                              <p style='margin:6px 0;'><strong>Pickup Location:</strong> {HtmlEncode(order.PickupLocation)}</p>
                              <p style='margin:6px 0;'><strong>Expected Pickup/Arrival:</strong> {order.ExpectedArrivalDate:dddd, MMMM dd, yyyy - hh:mm tt} (UTC)</p>
                            </div>

                            <div style='background:#e3f2fd;border:1px solid #bbdefb;padding:16px;border-radius:10px;margin:16px 0;'>
                              <p style='margin:6px 0;'>Track details here:</p>
                              <p style='margin:6px 0;'><a href='{orderLink}' target='_blank'>{orderLink}</a></p>
                            </div>
                        ";

                        _ = Task.Run(async () =>
                        {
                            try
                            {
                                var mail = _emailTemplateService.CreateNotificationEmail(
                                    email: buyerEmail,
                                    subject: $"Order #{order.MachineOrderID} Status: {newStatus}",
                                    message: messageHtml,
                                    userName: buyerName
                                );

                                await _emailTemplateService.SendEmailAsync(mail);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Machine order status email failed");
                            }
                        });
                    }
                }

                return ServiceResult.Ok($"Order #{orderId} status updated to {newStatus}.");
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync(ct);
                _logger.LogError(ex, "ChangeMachineOrderStatusAsync failed");
                return ServiceResult.Fail("Failed to change order status.");
            }
        }

        // Delete Machine Object From Store
        public async Task<ServiceResult> DeleteMachineAsync(int factoryId, int machineId, CancellationToken ct = default)
        {
            try
            {
                var machine = await _db.MachineStores
                    .FirstOrDefaultAsync(m => m.MachineID == machineId && m.SellerID == factoryId, ct);

                if (machine == null)
                    return ServiceResult.Fail("Machine not found or you don't have permission.");

                var activeStatuses = new[]
                {
                    EnumsOrderStatus.Pending,
                    EnumsOrderStatus.Processing,
                    EnumsOrderStatus.Confirmed,
                    EnumsOrderStatus.ReadyForPickup,
                    EnumsOrderStatus.Shipped,
                    EnumsOrderStatus.PickedUp,
                    EnumsOrderStatus.Delivered
                };

                var ordersQ = _db.MachineOrders
                    .AsNoTracking()
                    .Where(o => o.MachineStoreID == machineId);

                // no orders => hard delete + delete images
                var hasAnyOrders = await ordersQ.AnyAsync(ct);
                if (!hasAnyOrders)
                {
                    var urls = new[] { machine.MachineImgURL1, machine.MachineImgURL2, machine.MachineImgURL3 }
                        .Where(x => !string.IsNullOrWhiteSpace(x))
                        .Select(x => x!.Trim())
                        .Distinct()
                        .ToList();

                    _db.MachineStores.Remove(machine);
                    await _db.SaveChangesAsync(ct);

                    foreach (var url in urls)
                    {
                        try { await _imageStorage.DeleteAsync(url); }
                        catch (Exception ex) { _logger.LogWarning(ex, "Failed to delete image url {Url}", url); }
                    }

                    return ServiceResult.Ok("Machine deleted permanently (no orders).");
                }

                // orders exist => active?
                var hasActiveOrders = await ordersQ.AnyAsync(o => activeStatuses.Contains(o.Status), ct);

                // active orders => SOLD OUT + quantity 0
                if (hasActiveOrders)
                {
                    var reservedQty = await ordersQ
                        .Where(o => activeStatuses.Contains(o.Status))
                        .SumAsync(o => (int?)o.Quantity, ct) ?? 0;

                    machine.Quantity = 0;
                    machine.Status = ProductStatus.SoldOut;

                    await _db.SaveChangesAsync(ct);

                    return ServiceResult.Ok($"Machine marked SoldOut and stock set to 0. Reserved in active orders: {reservedQty}.");
                }

                // orders exist but none active => soft hide
                machine.Status = ProductStatus.RemovedByFactory;
                await _db.SaveChangesAsync(ct);

                return ServiceResult.Ok("Machine removed from store (hidden). It can be restored if returns happen.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DeleteMachineAsync failed");
                return ServiceResult.Fail("Failed to delete machine.");
            }
        }

        ////////////////////////////////////////// Machine Methods ////////////////////////////////////
        /////////////////////////////// User Method ///////////////////////////////

        // Get All Machine objects with state Available from database to store
        public async Task<List<FactoryStoreModel>> GetPublicMachinesAsync(SearchFilterModel? filter = null)
        {
            try
            {
                var activeStatuses = new[]
                {
                    EnumsOrderStatus.Pending,
                    EnumsOrderStatus.Confirmed,
                    EnumsOrderStatus.Processing,
                    EnumsOrderStatus.ReadyForPickup,
                    EnumsOrderStatus.Shipped
                };

                var excludedStatuses = new[]
                {
                    EnumsOrderStatus.Cancelled,
                    EnumsOrderStatus.DeletedByBuyer,
                    EnumsOrderStatus.DeletedBySeller
                };

                IQueryable<MachineStore> q = _db.MachineStores
                    .AsNoTracking()
                    .Include(x => x.Seller)
                        .ThenInclude(u => u.FactoryProfile)
                    .Where(x => x.Seller != null && x.Seller.Verified)
                    .Where(x => x.Status == ProductStatus.Available && x.Quantity > 0);

                if (filter != null)
                {
                    if (!string.IsNullOrWhiteSpace(filter.Keyword))
                    {
                        var kw = filter.Keyword.Trim();
                        q = q.Where(x =>
                            (x.MachineType != null && EF.Functions.Like(x.MachineType, $"%{kw}%")) ||
                            (x.Description != null && EF.Functions.Like(x.Description, $"%{kw}%")) ||
                            (x.Brand != null && EF.Functions.Like(x.Brand, $"%{kw}%")) ||
                            (x.Model != null && EF.Functions.Like(x.Model, $"%{kw}%"))
                        );
                    }

                    if (filter.MinPrice.HasValue) q = q.Where(x => x.Price >= filter.MinPrice.Value);
                    if (filter.MaxPrice.HasValue) q = q.Where(x => x.Price <= filter.MaxPrice.Value);
                }

                var baseRows = await q
                    .OrderByDescending(x => x.CreatedAt)
                    .Select(x => new
                    {
                        x.MachineID,
                        x.MachineType,
                        x.Description,
                        x.Price,
                        x.Quantity,
                        x.Status,
                        x.CreatedAt,
                        x.MinOrderQuantity,

                        x.MachineImgURL1,
                        x.MachineImgURL2,
                        x.MachineImgURL3,

                        x.Condition,
                        x.Brand,
                        x.Model,
                        x.WarrantyMonths,

                        // Listing pickup (stored on MachineStore)
                        ListingAddress = x.Address,
                        ListingLat = x.Latitude,
                        ListingLng = x.Longitude,

                        SellerUserId = x.Seller.UserId,
                        SellerVerified = x.Seller.Verified,
                        SellerName = x.Seller.FullName,
                        SellerAddress = x.Seller.Address,
                        SellerLat = x.Seller.Latitude,
                        SellerLng = x.Seller.Longitude,

                        FactoryName = x.Seller.FactoryProfile != null ? x.Seller.FactoryProfile.FactoryName : null,
                        FactoryImg1 = x.Seller.FactoryProfile != null ? x.Seller.FactoryProfile.FactoryImgURL1 : null,
                        FactoryImg2 = x.Seller.FactoryProfile != null ? x.Seller.FactoryProfile.FactoryImgURL2 : null,
                        FactoryImg3 = x.Seller.FactoryProfile != null ? x.Seller.FactoryProfile.FactoryImgURL3 : null
                    })
                    .ToListAsync();

                if (baseRows.Count == 0)
                    return new List<FactoryStoreModel>();

                var machineIds = baseRows.Select(r => r.MachineID).Distinct().ToList();

                var ordersAgg = await _db.MachineOrders
                    .AsNoTracking()
                    .Where(o => machineIds.Contains(o.MachineStoreID))
                    .GroupBy(o => o.MachineStoreID)
                    .Select(g => new
                    {
                        MachineID = g.Key,
                        OrdersCount = g.Count(o => !excludedStatuses.Contains(o.Status)),
                        ActiveOrdersCount = g.Count(o => activeStatuses.Contains(o.Status))
                    })
                    .ToDictionaryAsync(x => x.MachineID, x => new { x.OrdersCount, x.ActiveOrdersCount });

                return baseRows.Select(r =>
                {
                    var imgs = new[] { r.MachineImgURL1, r.MachineImgURL2, r.MachineImgURL3 }
                        .Where(z => !string.IsNullOrWhiteSpace(z))
                        .Select(z => z!.Trim())
                        .Distinct()
                        .ToList();

                    var factoryImgs = new[] { r.FactoryImg1, r.FactoryImg2, r.FactoryImg3 }
                        .Where(z => !string.IsNullOrWhiteSpace(z))
                        .Select(z => z!.Trim())
                        .Distinct()
                        .ToList();

                    var resolvedAddress = !string.IsNullOrWhiteSpace(r.ListingAddress) ? r.ListingAddress!.Trim() : r.SellerAddress;
                    var resolvedLat = r.ListingLat ?? r.SellerLat;
                    var resolvedLng = r.ListingLng ?? r.SellerLng;

                    var counts = ordersAgg.TryGetValue(r.MachineID, out var c) ? c : new { OrdersCount = 0, ActiveOrdersCount = 0 };

                    return new FactoryStoreModel
                    {
                        Id = r.MachineID,
                        Type = "Machine",

                        Name = r.MachineType ?? "Machine",
                        MachineType = r.MachineType,
                        Description = r.Description,

                        Price = r.Price,
                        AvailableQuantity = r.Quantity,
                        Status = r.Status.ToString(),
                        CreatedAt = r.CreatedAt,
                        MinOrderQuantity = r.MinOrderQuantity,

                        ImageUrl = imgs.FirstOrDefault(),
                        ImageUrls = imgs,

                        MachineCondition = r.Condition?.ToString(),
                        Brand = r.Brand,
                        Model = r.Model,
                        WarrantyMonths = r.WarrantyMonths,

                        SellerUserId = r.SellerUserId,
                        SellerName = !string.IsNullOrWhiteSpace(r.FactoryName) ? r.FactoryName : r.SellerName,
                        IsVerifiedSeller = r.SellerVerified,

                        SellerAddress = r.SellerAddress,
                        SellerLatitude = r.SellerLat,
                        SellerLongitude = r.SellerLng,

                        FactoryName = r.FactoryName,
                        FactoryImageUrls = factoryImgs,
                        FactoryAddress = resolvedAddress,
                        FactoryLatitude = resolvedLat,
                        FactoryLongitude = resolvedLng,

                        OrdersCount = counts.OrdersCount,
                        ActiveOrdersCount = counts.ActiveOrdersCount
                    };
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetPublicMachinesAsync failed");
                return new List<FactoryStoreModel>();
            }
        }

        // Get Details about Machine object by id 
        public async Task<FactoryStoreModel?> GetPublicMachineDetailsAsync(int machineId)
        {
            try
            {
                var row = await _db.MachineStores
                    .AsNoTracking()
                    .Include(m => m.Seller).ThenInclude(u => u.FactoryProfile)
                    .Where(m =>
                        m.MachineID == machineId &&
                        m.Seller != null &&
                        m.Seller.Verified
                    )
                    .Select(m => new
                    {
                        m.MachineID,
                        m.MachineType,
                        m.Description,
                        m.Price,
                        m.Quantity,
                        m.Status,
                        m.CreatedAt,
                        m.MinOrderQuantity,
                        m.CancelWindowDays,
                        m.DeliveryDays,

                        m.Condition,
                        m.Brand,
                        m.Model,
                        m.WarrantyMonths,

                        m.MachineImgURL1,
                        m.MachineImgURL2,
                        m.MachineImgURL3,

                        // Listing pickup stored on machine
                        MachinePickupAddress = m.Address,
                        MachineLat = m.Latitude,
                        MachineLng = m.Longitude,

                        // Seller
                        SellerUserId = m.Seller.UserId,
                        SellerOwnerName = m.Seller.FullName,
                        SellerVerified = m.Seller.Verified,
                        SellerProfileImg = m.Seller.UserProfileImgURL,

                        // Factory default location (seller profile)
                        FactoryAddress = m.Seller.Address,
                        FactoryLat = m.Seller.Latitude,
                        FactoryLng = m.Seller.Longitude,

                        // Factory profile
                        FactoryName = m.Seller.FactoryProfile != null ? m.Seller.FactoryProfile.FactoryName : null,
                        FactoryImg1 = m.Seller.FactoryProfile != null ? m.Seller.FactoryProfile.FactoryImgURL1 : null,
                        FactoryImg2 = m.Seller.FactoryProfile != null ? m.Seller.FactoryProfile.FactoryImgURL2 : null,
                        FactoryImg3 = m.Seller.FactoryProfile != null ? m.Seller.FactoryProfile.FactoryImgURL3 : null,

                        OrdersCount = _db.MachineOrders.Count(o =>
                            o.MachineStoreID == m.MachineID &&
                            o.Status != EnumsOrderStatus.Cancelled &&
                            o.Status != EnumsOrderStatus.DeletedByBuyer &&
                            o.Status != EnumsOrderStatus.DeletedBySeller
                        )
                    })
                    .FirstOrDefaultAsync();

                if (row == null) return null;

                var imgs = new[] { row.MachineImgURL1, row.MachineImgURL2, row.MachineImgURL3 }
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => x!.Trim())
                    .Distinct()
                    .ToList();

                var factoryImgs = new[] { row.FactoryImg1, row.FactoryImg2, row.FactoryImg3 }
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => x!.Trim())
                    .Distinct()
                    .ToList();

                var today = DateTime.UtcNow;
                var previewArrival = today.AddDays(Math.Max(0, row.DeliveryDays));

                return new FactoryStoreModel
                {
                    Id = row.MachineID,
                    Type = "Machine",

                    MachineType = row.MachineType,
                    Name = row.MachineType ?? "Machine",
                    Description = row.Description,

                    Price = row.Price,
                    AvailableQuantity = row.Quantity,
                    Status = row.Status.ToString(),
                    CreatedAt = row.CreatedAt,
                    MinOrderQuantity = row.MinOrderQuantity,

                    ImageUrl = imgs.FirstOrDefault(),
                    ImageUrls = imgs,

                    MachineCondition = row.Condition?.ToString(),
                    Brand = row.Brand,
                    Model = row.Model,
                    WarrantyMonths = row.WarrantyMonths,

                    SellerUserId = row.SellerUserId,
                    SellerName = !string.IsNullOrWhiteSpace(row.FactoryName) ? row.FactoryName : row.SellerOwnerName,
                    IsVerifiedSeller = row.SellerVerified,
                    SellerProfileImgUrl = row.SellerProfileImg,

                    FactoryName = row.FactoryName,
                    FactoryImageUrls = factoryImgs,

                    // Display pickup location: prefer listing, fallback seller profile
                    FactoryAddress = !string.IsNullOrWhiteSpace(row.MachinePickupAddress) ? row.MachinePickupAddress : row.FactoryAddress,
                    FactoryLatitude = row.MachineLat ?? row.FactoryLat,
                    FactoryLongitude = row.MachineLng ?? row.FactoryLng,

                    OrdersCount = row.OrdersCount,
                    ExpectedArrivalDate = previewArrival
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetPublicMachineDetailsAsync failed");
                return null;
            }
        }

        // Book Machine Order
        public async Task<ServiceResult> PlaceMachineOrderAsync(int buyerId, int machineId, int quantity, decimal paidAmount, decimal walletUsed, string provider, string providerPaymentId, CancellationToken ct = default)
        {
            if (quantity <= 0) return ServiceResult.Fail("Invalid quantity.");

            paidAmount = Math.Round(paidAmount, 2, MidpointRounding.AwayFromZero);
            walletUsed = Math.Round(walletUsed, 2, MidpointRounding.AwayFromZero);

            if (paidAmount <= 0m) return ServiceResult.Fail("Paid amount is invalid.");
            if (walletUsed > paidAmount) return ServiceResult.Fail("WalletUsed cannot exceed PaidAmount.");

            try
            {
                // Payment idempotency
                if (!string.IsNullOrWhiteSpace(providerPaymentId))
                {
                    var exists = await _db.PaymentTransactions
                        .AsNoTracking()
                        .AnyAsync(p => p.Provider == provider && p.ProviderPaymentId == providerPaymentId, ct);

                    if (exists) return ServiceResult.Fail("Payment already processed.");
                }

                var machine = await _db.MachineStores
                    .Include(m => m.Seller)
                        .ThenInclude(u => u.FactoryProfile)
                    .FirstOrDefaultAsync(m =>
                        m.MachineID == machineId &&
                        m.Status == ProductStatus.Available &&
                        m.Quantity > 0 &&
                        m.Seller != null &&
                        m.Seller.Verified, ct);

                if (machine == null) return ServiceResult.Fail("Machine not available.");
                if (machine.SellerID == buyerId) return ServiceResult.Fail("You can't order your own listing.");

                var min = MinQtyToInt(machine.MinOrderQuantity);
                if (quantity < min) return ServiceResult.Fail($"Minimum order quantity is {min}.");

                await using var tx = await _db.Database.BeginTransactionAsync(ct);

                // reload for concurrency
                await _db.Entry(machine).ReloadAsync(ct);
                if (machine.Quantity < quantity) return ServiceResult.Fail("Not enough quantity available.");

                var unitPrice = Math.Round(machine.Price, 2, MidpointRounding.AwayFromZero);
                var totalPrice = Math.Round(unitPrice * quantity, 2, MidpointRounding.AwayFromZero);

                if (paidAmount > totalPrice + 0.0001m)
                    return ServiceResult.Fail("Paid amount exceeds total price.");

                // Use PricingOptions
                var pricing = _pricingOptions.Value;

                var depositRequired = Math.Round(totalPrice * pricing.DepositPercent, 2, MidpointRounding.AwayFromZero);
                if (paidAmount + 0.0001m < depositRequired)
                    return ServiceResult.Fail($"Minimum deposit is {depositRequired:N2} EGP.");

                // Compute platform fee internally
                var platformFeeCalc = Math.Round(paidAmount * pricing.PlatformFeePercent, 2, MidpointRounding.AwayFromZero);
                if (platformFeeCalc > paidAmount) platformFeeCalc = paidAmount;

                // Buyer wallet debit
                long? buyerWalletTxnId = null;
                if (walletUsed > 0m)
                {
                    var buyerWallet = await GetOrCreateWalletAsync(buyerId, ct);

                    var availableBuyer = Math.Round(buyerWallet.Balance - buyerWallet.ReservedBalance, 2, MidpointRounding.AwayFromZero);
                    if (availableBuyer + 0.0001m < walletUsed)
                        return ServiceResult.Fail("Insufficient available wallet balance.");

                    var wtxn = await AddWalletTxnAsync(
                        wallet: buyerWallet,
                        type: WalletTxnType.PaymentDebit,
                        amount: -walletUsed,
                        currency: "EGP",
                        note: $"Machine order wallet payment (machineId={machineId})",
                        idempotencyKey: $"BUYER_PAY:{provider}:{providerPaymentId}",
                        ct: ct
                    );

                    buyerWalletTxnId = wtxn.Id;
                }

                // Reduce stock
                machine.Quantity -= quantity;
                if (machine.Quantity == 0) machine.Status = ProductStatus.SoldOut;

                var now = DateTime.UtcNow;

                // Create Order (Pending)
                var order = new MachineOrder
                {
                    MachineStoreID = machine.MachineID,
                    BuyerID = buyerId,
                    Status = EnumsOrderStatus.Pending,
                    OrderDate = now,

                    Quantity = quantity,
                    UnitPrice = unitPrice,
                    TotalPrice = totalPrice,

                    DepositPaid = paidAmount,

                    CancelUntil = now.AddDays(Math.Max(0, machine.CancelWindowDays)),
                    ExpectedArrivalDate = now.AddDays(Math.Max(0, machine.DeliveryDays)),

                    Latitude = machine.Latitude,
                    Longitude = machine.Longitude,
                    PickupLocation = machine.Address
                };

                _db.MachineOrders.Add(order);
                await _db.SaveChangesAsync(ct);

                // Factory Wallet HOLD paidAmount
                var sellerId = machine.SellerID;
                var factoryWallet = await GetOrCreateWalletAsync(sellerId, ct);

                await HoldAsync(
                    wallet: factoryWallet,
                    amount: paidAmount,
                    note: $"Order HOLD (machine orderId={order.MachineOrderID})",
                    idemKey: $"HOLD:{provider}:{providerPaymentId}",
                    ct: ct
                );

                // Update FactoryProfile totals
                var factoryProfile = machine.Seller?.FactoryProfile;
                if (factoryProfile != null)
                {
                    factoryProfile.TotalBalanceOrderWaiting =
                        Math.Round((factoryProfile.TotalBalanceOrderWaiting ?? 0m) + paidAmount, 2, MidpointRounding.AwayFromZero);

                    factoryProfile.TotalBalancePercentageRequests =
                        Math.Round((factoryProfile.TotalBalancePercentageRequests ?? 0m) + platformFeeCalc, 2, MidpointRounding.AwayFromZero);
                }

                // PaymentTransaction record
                _db.PaymentTransactions.Add(new PaymentTransaction
                {
                    UserId = buyerId,
                    Provider = provider,
                    ProviderPaymentId = providerPaymentId,
                    Amount = paidAmount,
                    Currency = "EGP",
                    Status = PaymentStatus.Succeeded,
                    WalletTransactionId = buyerWalletTxnId,
                    CreatedAt = now
                });

                await _db.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);

                // Send Email
                var buyerRow = await _db.Users
                    .AsNoTracking()
                    .Where(u => u.UserId == buyerId)
                    .Select(u => new
                    {
                        EmailEncrypted = u.Email,
                        Name = (u.FullName ?? u.Email)
                    })
                    .FirstOrDefaultAsync(ct);

                string buyerEmail = "";
                string buyerName = buyerRow?.Name ?? "";

                if (buyerRow != null && !string.IsNullOrWhiteSpace(buyerRow.EmailEncrypted))
                {
                    try { buyerEmail = _dataCiphers.Decrypt(buyerRow.EmailEncrypted); }
                    catch { buyerEmail = ""; }
                }

                var imgRow = await _db.MachineStores
                    .AsNoTracking()
                    .Where(m => m.MachineID == machine.MachineID)
                    .Select(m => new { m.MachineImgURL1, m.MachineImgURL2, m.MachineImgURL3 })
                    .FirstOrDefaultAsync(ct);

                var imageUrls = new List<string?> { imgRow?.MachineImgURL1, imgRow?.MachineImgURL2, imgRow?.MachineImgURL3 }
                    .Where(u => !string.IsNullOrWhiteSpace(u))
                    .Select(u => u!)
                    .Distinct()
                    .ToList();

                var imagesHtml = imageUrls.Any()
                    ? "<div style='margin:10px 0;'>" + string.Join("", imageUrls.Select(u =>
                        $@"<a href='{u}' target='_blank' style='text-decoration:none;'>
                       <img src='{EmailTemplateConfig.WebsiteUrl}/{u}' alt='Machine Image'
                            style='width:110px;height:110px;object-fit:cover;border-radius:10px;border:1px solid #eee;margin:6px;' />
                   </a>"
                      )) + "</div>"
                    : "<p style='color:#6c757d;margin:10px 0;'>No images available for this machine.</p>";

                static string HtmlEncode(string? value)
                {
                    if (string.IsNullOrEmpty(value)) return "";
                    return System.Net.WebUtility.HtmlEncode(value);
                }

                var remaining = Math.Round(totalPrice - paidAmount, 2, MidpointRounding.AwayFromZero);
                if (remaining < 0m) remaining = 0m;

                var factoryName =
                    machine.Seller?.FactoryProfile?.FactoryName
                    ?? machine.Seller?.FullName
                    ?? EmailTemplateConfig.CompanyName;

                var factoryLocation = EmailTemplateConfig.SupportLocation;

                var orderLink = $"{EmailTemplateConfig.WebsiteUrl}/CraftsMan/MachineOrderDetails/{order.MachineOrderID}";
                var machineLink = $"{EmailTemplateConfig.WebsiteUrl}/CraftsMan/MachineDetails/{machine.MachineID}";

                var messageHtml = $@"
                    <p>Your machine order has been created successfully ✅</p>

                    <div style='background:#f8f9fa;border:1px solid #eee;padding:16px;border-radius:10px;margin:16px 0;'>
                      <h3 style='margin:0 0 10px 0;'>🧾 Order Summary</h3>
                      <p style='margin:6px 0;'><strong>Order ID:</strong> #{order.MachineOrderID}</p>
                      <p style='margin:6px 0;'><strong>Status:</strong> {HtmlEncode(order.Status.ToString())}</p>
                      <p style='margin:6px 0;'><strong>Machine Type:</strong> {HtmlEncode(machine.MachineType)}</p>
                      <p style='margin:6px 0;'><strong>Brand:</strong> {HtmlEncode(machine.Brand)}</p>
                      <p style='margin:6px 0;'><strong>Model:</strong> {HtmlEncode(machine.Model)}</p>
                      <p style='margin:6px 0;'><strong>Condition:</strong> {HtmlEncode(machine.Condition?.ToString())}</p>

                      <p style='margin:6px 0;'><strong>Quantity:</strong> {order.Quantity}</p>
                      <p style='margin:6px 0;'><strong>Unit Price:</strong> {unitPrice:N2} EGP</p>
                      <p style='margin:6px 0;'><strong>Total Price:</strong> {totalPrice:N2} EGP</p>
                      <p style='margin:6px 0;'><strong>Paid Now:</strong> {paidAmount:N2} EGP</p>
                      <p style='margin:6px 0;'><strong>Remaining:</strong> {remaining:N2} EGP</p>
                    </div>

                    <div style='background:#fff;border:1px solid #eee;padding:16px;border-radius:10px;margin:16px 0;'>
                      <h3 style='margin:0 0 10px 0;'>📍 Pickup Details</h3>
                      <p style='margin:6px 0;'><strong>Pickup Location:</strong> {HtmlEncode(order.PickupLocation)}</p>
                      <p style='margin:6px 0;'><strong>Factory:</strong> {HtmlEncode(factoryName)}</p>
                      <p style='margin:6px 0;'><strong>Factory Location:</strong> {HtmlEncode(factoryLocation)}</p>
                      <p style='margin:6px 0;'><strong>Expected Pickup/Arrival:</strong> {order.ExpectedArrivalDate:dddd, MMMM dd, yyyy - hh:mm tt} (UTC)</p>
                      <p style='margin:6px 0;'><strong>Cancel Window Ends:</strong> {order.CancelUntil:dddd, MMMM dd, yyyy - hh:mm tt} (UTC)</p>
                    </div>

                    <div style='background:#fff;border:1px solid #eee;padding:16px;border-radius:10px;margin:16px 0;'>
                      <h3 style='margin:0 0 10px 0;'>🖼️ Machine Images</h3>
                      {imagesHtml}
                      <p style='margin:10px 0 0 0;'><a href='{machineLink}' target='_blank'>Open machine page</a></p>
                    </div>

                    <div style='background:#e8f5e9;border:1px solid #c8e6c9;padding:16px;border-radius:10px;margin:16px 0;'>
                      <h3 style='margin:0 0 10px 0;'>🔎 Track your order</h3>
                      <p style='margin:6px 0;'>Follow updates from here:</p>
                      <p style='margin:6px 0;'><a href='{orderLink}' target='_blank'>{orderLink}</a></p>
                    </div>
                ";

                if (!string.IsNullOrWhiteSpace(buyerEmail))
                {
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            var mail = _emailTemplateService.CreateNotificationEmail(
                                email: buyerEmail,
                                subject: $"Machine Order #{order.MachineOrderID} Created",
                                message: messageHtml,
                                userName: buyerName
                            );

                            await _emailTemplateService.SendEmailAsync(mail);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Machine order created email failed");
                        }
                    });
                }

                return ServiceResult.Ok(
                    $"Order created. Paid {paidAmount:N2} of {totalPrice:N2} EGP.",
                    order.MachineOrderID
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PlaceMachineOrderAsync failed");
                return ServiceResult.Fail("Failed to create machine order.");
            }
        }

        // Get Machine Details before Book
        public async Task<FactoryStoreModel?> GetMachineDetailsForBuyerAsync(int machineId, int buyerId)
        {
            try
            {
                var row = await (
                    from m in _db.MachineStores.AsNoTracking()
                    join u in _db.Users.AsNoTracking().Include(x => x.FactoryProfile) on m.SellerID equals u.UserId
                    where m.MachineID == machineId
                    where u.Verified
                    where (m.Status == ProductStatus.Available)
                        || _db.MachineOrders.Any(o => o.MachineStoreID == machineId && o.BuyerID == buyerId)
                    select new
                    {
                        m.MachineID,
                        m.MachineType,
                        m.Description,
                        m.Price,
                        m.Quantity,
                        m.Status,
                        m.CreatedAt,
                        m.MinOrderQuantity,

                        m.MachineImgURL1,
                        m.MachineImgURL2,
                        m.MachineImgURL3,

                        // Listing pickup
                        ListingAddress = m.Address,
                        ListingLat = m.Latitude,
                        ListingLng = m.Longitude,

                        SellerUserId = u.UserId,
                        SellerName = u.FullName,
                        SellerVerified = u.Verified,
                        SellerProfileImg = u.UserProfileImgURL,
                        SellerLat = u.Latitude,
                        SellerLng = u.Longitude,
                        SellerAddress = u.Address,

                        FactoryName = u.FactoryProfile != null ? u.FactoryProfile.FactoryName : null,
                        FactoryImg1 = u.FactoryProfile != null ? u.FactoryProfile.FactoryImgURL1 : null,
                        FactoryImg2 = u.FactoryProfile != null ? u.FactoryProfile.FactoryImgURL2 : null,
                        FactoryImg3 = u.FactoryProfile != null ? u.FactoryProfile.FactoryImgURL3 : null,

                        OrdersCount = _db.MachineOrders.Count(o => o.MachineStoreID == m.MachineID),

                        MyOrder = _db.MachineOrders
                            .Where(o => o.MachineStoreID == machineId && o.BuyerID == buyerId)
                            .OrderByDescending(o => o.OrderDate)
                            .Select(o => new
                            {
                                o.MachineOrderID,
                                o.Status,
                                o.OrderDate,
                                o.Quantity,
                                o.CancelUntil,
                                o.ExpectedArrivalDate,
                                o.PickupLocation,
                                o.TotalPrice,
                                o.DepositPaid
                            })
                            .FirstOrDefault()
                    }
                ).FirstOrDefaultAsync();

                if (row == null) return null;

                var imgs = new[] { row.MachineImgURL1, row.MachineImgURL2, row.MachineImgURL3 }
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => x!.Trim())
                    .Distinct()
                    .ToList();

                var factoryImgs = new[] { row.FactoryImg1, row.FactoryImg2, row.FactoryImg3 }
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => x!.Trim())
                    .Distinct()
                    .ToList();

                bool canCancel = false;
                if (row.MyOrder != null)
                {
                    canCancel = (row.MyOrder.Status == EnumsOrderStatus.Pending ||
                                 row.MyOrder.Status == EnumsOrderStatus.Confirmed ||
                                 row.MyOrder.Status == EnumsOrderStatus.Processing)
                        && DateTime.UtcNow <= row.MyOrder.CancelUntil;
                }

                // Resolve pickup location: prefer listing fields, fallback seller profile
                var resolvedAddress = !string.IsNullOrWhiteSpace(row.ListingAddress) ? row.ListingAddress : row.SellerAddress;

                return new FactoryStoreModel
                {
                    Id = row.MachineID,
                    Type = "Machine",
                    MachineType = row.MachineType,
                    Name = row.MachineType ?? "Machine",
                    Description = row.Description,

                    Price = row.Price,
                    AvailableQuantity = row.Quantity,
                    Status = row.Status.ToString(),
                    CreatedAt = row.CreatedAt,
                    MinOrderQuantity = row.MinOrderQuantity,

                    ImageUrl = imgs.FirstOrDefault(),
                    ImageUrls = imgs,

                    SellerUserId = row.SellerUserId,
                    SellerName = !string.IsNullOrWhiteSpace(row.FactoryName) ? row.FactoryName : row.SellerName,
                    IsVerifiedSeller = row.SellerVerified,
                    SellerProfileImgUrl = row.SellerProfileImg,
                    SellerLatitude = row.SellerLat,
                    SellerLongitude = row.SellerLng,
                    SellerAddress = row.SellerAddress,

                    FactoryName = row.FactoryName,
                    FactoryImageUrls = factoryImgs,

                    FactoryAddress = row.MyOrder?.PickupLocation ?? resolvedAddress,
                    OrdersCount = row.OrdersCount,

                    MyOrderId = row.MyOrder?.MachineOrderID,
                    MyOrderStatus = row.MyOrder?.Status.ToString(),
                    MyOrderDate = row.MyOrder?.OrderDate,
                    MyOrderQuantity = row.MyOrder?.Quantity,
                    CancelUntil = row.MyOrder?.CancelUntil,
                    ExpectedArrivalDate = row.MyOrder?.ExpectedArrivalDate,
                    CanCancel = canCancel
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetMachineDetailsForBuyerAsync failed");
                return null;
            }
        }

        // Get Details about order after Book
        public async Task<(MachineOrder? order, FactoryStoreModel? details)> GetMachineOrderDetailsAsync(int buyerId, int orderId)
        {
            var order = await _db.MachineOrders
                .AsNoTracking()
                .Include(o => o.MachineStore).ThenInclude(m => m.Seller).ThenInclude(u => u.FactoryProfile)
                .FirstOrDefaultAsync(o => o.MachineOrderID == orderId && o.BuyerID == buyerId);

            if (order == null) return (null, null);

            var details = await GetMachineDetailsForBuyerAsync(order.MachineStoreID, buyerId);
            return (order, details);
        }

        // Cancel Order
        public async Task<ServiceResult> CancelMachineOrderAsync(int buyerId, int orderId, CancellationToken ct = default)
        {
            try
            {
                var order = await _db.MachineOrders
                    .Include(o => o.MachineStore)
                        .ThenInclude(m => m.Seller)
                            .ThenInclude(u => u.FactoryProfile)
                    .FirstOrDefaultAsync(o => o.MachineOrderID == orderId && o.BuyerID == buyerId, ct);

                if (order == null) return ServiceResult.Fail("Order not found.");
                if (order.Status == EnumsOrderStatus.Cancelled) return ServiceResult.Fail("Order already cancelled.");
                if (!(order.Status == EnumsOrderStatus.Pending || order.Status == EnumsOrderStatus.Confirmed || order.Status == EnumsOrderStatus.Processing)) return ServiceResult.Fail("You can’t cancel this order at its current status.");
                if (DateTime.UtcNow > order.CancelUntil) return ServiceResult.Fail("Cancel window has expired.");
                if (order.MachineStore == null || order.MachineStore.Seller == null) return ServiceResult.Fail("Listing/Seller not found.");

                var refundAmount = Math.Round(order.DepositPaid, 2, MidpointRounding.AwayFromZero);

                var pricing = _pricingOptions.Value;
                var platformFee = Math.Round(refundAmount * pricing.PlatformFeePercent, 2, MidpointRounding.AwayFromZero);
                if (platformFee > refundAmount) platformFee = refundAmount;

                await using var tx = await _db.Database.BeginTransactionAsync(ct);

                // Restore inventory
                order.MachineStore.Quantity += order.Quantity;
                if (order.MachineStore.Quantity > 0 && order.MachineStore.Status != ProductStatus.Available)
                    order.MachineStore.Status = ProductStatus.Available;

                // Refund: factory RESERVED -> buyer wallet
                var sellerId = order.MachineStore.SellerID;
                var factoryWallet = await GetOrCreateWalletAsync(sellerId, ct);
                var buyerWallet = await GetOrCreateWalletAsync(buyerId, ct);

                if (factoryWallet.ReservedBalance + 0.0001m < refundAmount)
                    return ServiceResult.Fail("Factory reserved balance is insufficient to refund (hold missing).");

                await TransferAsync(
                    from: factoryWallet,
                    to: buyerWallet,
                    amount: refundAmount,
                    consumeFromReserved: true,
                    note: $"Refund cancelled machine orderId={orderId}",
                    idemKey: $"MREF:{orderId}",
                    ct: ct
                );

                // Update factory totals
                var factoryProfile = order.MachineStore.Seller.FactoryProfile;
                if (factoryProfile != null)
                {
                    factoryProfile.TotalBalanceOrderWaiting =
                        Math.Round((factoryProfile.TotalBalanceOrderWaiting ?? 0m) - refundAmount, 2, MidpointRounding.AwayFromZero);

                    factoryProfile.TotalBalancePercentageRequests =
                        Math.Round((factoryProfile.TotalBalancePercentageRequests ?? 0m) - platformFee, 2, MidpointRounding.AwayFromZero);

                    if (factoryProfile.TotalBalanceOrderWaiting < 0m) factoryProfile.TotalBalanceOrderWaiting = 0m;
                    if (factoryProfile.TotalBalancePercentageRequests < 0m) factoryProfile.TotalBalancePercentageRequests = 0m;
                }

                // Cancel order
                order.Status = EnumsOrderStatus.Cancelled;

                await _db.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);

                return ServiceResult.Ok("Order cancelled successfully. Funds refunded to your wallet and platform fee cancelled.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CancelMachineOrderAsync failed");
                return ServiceResult.Fail("Failed to cancel order.");
            }
        }

        // Hide Order after Canceled from my list
        public async Task<ServiceResult> HideMachineOrderForBuyerAsync(int buyerId, int orderId, CancellationToken ct = default)
        {
            try
            {
                var order = await _db.MachineOrders
                    .FirstOrDefaultAsync(o => o.MachineOrderID == orderId && o.BuyerID == buyerId, ct);

                if (order == null) return ServiceResult.Fail("Order not found.");

                // Only allow hide if status is Cancelled OR DeletedBySeller
                if (order.Status != EnumsOrderStatus.Cancelled && order.Status != EnumsOrderStatus.DeletedBySeller)
                    return ServiceResult.Fail("You can remove this order only if it is Cancelled or Deleted by Seller.");

                // If seller already deleted, remove permanently
                if (order.Status == EnumsOrderStatus.DeletedBySeller)
                {
                    _db.MachineOrders.Remove(order);
                    await _db.SaveChangesAsync(ct);
                    return ServiceResult.Ok("Order deleted permanently.");
                }

                // If cancelled -> hide for buyer
                order.Status = EnumsOrderStatus.DeletedByBuyer;
                await _db.SaveChangesAsync(ct);

                return ServiceResult.Ok("Order removed from your list.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "HideMachineOrderForBuyerAsync failed");
                return ServiceResult.Fail("Failed to delete order.");
            }
        }

        // Get All order Machine by user 
        public async Task<List<MachineOrder>> GetMachineOrdersForBuyerAsync(int buyerId)
        {
            return await _db.MachineOrders
                .AsNoTracking()
                .Include(o => o.MachineStore).ThenInclude(m => m.Seller)
                .Where(o => o.BuyerID == buyerId
                    && o.Status != EnumsOrderStatus.DeletedByBuyer)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        ////////////////////////////////////////// Rental Methods ////////////////////////////////////
        /////////////////////////////// Factory Method ///////////////////////////////

        // Get Location And Searching Filter
        private async Task<(bool ok, string? error, decimal? lat, decimal? lng, string? normalizedAddress)> ResolveRentalLocationAsync(RentalProductDetailsModel model, CancellationToken ct = default)
        {
            var loc = _locationService.ExtractAndValidateFromForm(
                model.Latitude?.ToString(CultureInfo.InvariantCulture),
                model.Longitude?.ToString(CultureInfo.InvariantCulture),
                model.Location
            );

            if (!loc.IsValid)
                return (false, loc.Error, null, null, null);

            var gpsOrMapProvided = loc.Latitude.HasValue && loc.Longitude.HasValue;

            if (gpsOrMapProvided)
            {
                // user picked from map/GPS
                var lat = loc.Latitude!.Value;
                var lng = loc.Longitude!.Value;

                // If the user doesn't enter an address → we perform ReverseGeocode
                string? address = loc.Address;
                if (string.IsNullOrWhiteSpace(address))
                {
                    var rev = await _locationService.ReverseGeocodeAsync(lat, lng, ct);
                    address = rev;
                }

                if (string.IsNullOrWhiteSpace(address))
                    address = model.Location;

                return (true, null, lat, lng, address);
            }

            // No lat/lng provided -> must have address (Location required anyway)
            if (!string.IsNullOrWhiteSpace(loc.Address))
            {
                var geo = await _locationService.GetLocationFromAddressAsync(loc.Address!, ct);
                if (geo != null)
                {
                    var normalized = string.IsNullOrWhiteSpace(geo.NormalizedAddress)
                        ? loc.Address
                        : geo.NormalizedAddress;

                    return (true, null, geo.Latitude, geo.Longitude, normalized);
                }

                // Geocode execution failed: We only store the address (lat/lng null)
                return (true, null, null, null, loc.Address);
            }

            // No coordinates or address
            return (false, "Please select location on map or type an address.", null, null, null);
        }

        // Get All Rental Object To Factory
        public async Task<List<FactoryStoreModel>> GetRentalsAsync(int factoryId, SearchFilterModel? filter = null)
        {
            var activeStatuses = new[]
            {
                EnumsOrderStatus.Pending,
            };

            try
            {
                IQueryable<RentalStore> query = _db.RentalStores
                    .AsNoTracking()
                    .Where(r => r.OwnerID == factoryId)
                    .Include(r => r.Owner);

                // Filters
                if (filter != null)
                {
                    if (!string.IsNullOrWhiteSpace(filter.Keyword))
                    {
                        var keyword = filter.Keyword.Trim();
                        query = query.Where(r =>
                            (r.Address != null && EF.Functions.Like(r.Address, $"%{keyword}%")) ||
                            (r.Description != null && EF.Functions.Like(r.Description, $"%{keyword}%")));
                    }

                    if (!string.IsNullOrWhiteSpace(filter.Status))
                    {
                        if (Enum.TryParse<ProductStatus>(filter.Status.Trim(), true, out var statusEnum))
                            query = query.Where(r => r.Status == statusEnum);
                    }

                    if (!string.IsNullOrWhiteSpace(filter.Location))
                    {
                        var loc = filter.Location.Trim();
                        query = query.Where(r => r.Address != null && EF.Functions.Like(r.Address, $"%{loc}%"));
                    }

                    if (filter.DateFrom.HasValue)
                    {
                        var from = filter.DateFrom.Value.Date;
                        query = query.Where(r => r.AvailableFrom.Date >= from);
                    }

                    if (filter.DateTo.HasValue)
                    {
                        var to = filter.DateTo.Value.Date;
                        query = query.Where(r => r.AvailableFrom.Date <= to);
                    }

                    if (filter.MinPrice.HasValue)
                        query = query.Where(r => r.PricePerMonth >= filter.MinPrice.Value);

                    if (filter.MaxPrice.HasValue)
                        query = query.Where(r => r.PricePerMonth <= filter.MaxPrice.Value);
                }

                var totalOrdersForFactoryRentals = await _db.RentalOrders
                    .AsNoTracking()
                    .CountAsync(o => _db.RentalStores.Any(r => r.OwnerID == factoryId && r.RentalID == o.RentalStoreID));

                // Main Query (Counts + Data)
                var rows = await query
                    .OrderByDescending(r => r.AvailableFrom)
                    .Select(r => new
                    {
                        Rental = r,

                        OrdersCount = _db.RentalOrders.Count(o => o.RentalStoreID == r.RentalID),
                        ActiveOrdersCount = _db.RentalOrders.Count(o =>
                            o.RentalStoreID == r.RentalID && activeStatuses.Contains(o.Status))
                    })
                    .ToListAsync();

                var rentals = rows.Select(x =>
                {
                    var r = x.Rental;

                    var images = new[] { r.RentalImgURL1, r.RentalImgURL2, r.RentalImgURL3 }
                        .Where(u => !string.IsNullOrWhiteSpace(u))
                        .Select(u => u!.Trim())
                        .Distinct()
                        .ToList();

                    return new FactoryStoreModel
                    {
                        Id = r.RentalID,
                        Type = "Rental",

                        Name = !string.IsNullOrWhiteSpace(r.Address) ? r.Address.Trim() : "Rental",

                        Price = r.PricePerMonth,
                        AvailableQuantity = 1,
                        Unit = "month",

                        Status = r.Status.ToString(),
                        SellerName = r.Owner?.FullName,
                        CreatedAt = r.AvailableFrom,
                        IsVerifiedSeller = r.Owner?.Verified ?? false,

                        OrdersCount = x.OrdersCount,
                        ActiveOrdersCount = x.ActiveOrdersCount,

                        RentalAddress = r.Address,
                        RentalArea = r.Area,
                        AvailableFrom = r.AvailableFrom,
                        AvailableUntil = r.AvailableUntil,
                        RentalCondition = r.Condition.HasValue ? r.Condition.Value.ToString() : null,
                        IsFurnished = r.IsFurnished,
                        HasElectricity = r.HasElectricity,
                        HasWater = r.HasWater,

                        RentalLatitude = r.Latitude,
                        RentalLongitude = r.Longitude,

                        ImageUrl = images.FirstOrDefault(),
                        ImageUrls = images
                    };
                }).ToList();

                return rentals;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting rentals");
                return new List<FactoryStoreModel>();
            }
        }

        // Get Rental Object Data by ID To Factory
        public async Task<RentalProductDetailsModel?> GetRentalByIdAsync(int id, int factoryId)
        {
            try
            {
                var rental = await _db.RentalStores
                    .AsNoTracking()
                    .FirstOrDefaultAsync(r => r.RentalID == id && r.OwnerID == factoryId);

                if (rental == null) return null;

                return new RentalProductDetailsModel
                {
                    Id = rental.RentalID,
                    Location = rental.Address,
                    Area = rental.Area,
                    PricePerMonth = rental.PricePerMonth,
                    Description = rental.Description,

                    Status = rental.Status,
                    AvailableFrom = rental.AvailableFrom,
                    AvailableUntil = rental.AvailableUntil,

                    Condition = rental.Condition,
                    IsFurnished = rental.IsFurnished,
                    HasElectricity = rental.HasElectricity,
                    HasWater = rental.HasWater,

                    // location
                    Latitude = rental.Latitude,
                    Longitude = rental.Longitude,

                    // current images
                    CurrentImageUrl1 = rental.RentalImgURL1,
                    CurrentImageUrl2 = rental.RentalImgURL2,
                    CurrentImageUrl3 = rental.RentalImgURL3
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting rental by ID");
                return null;
            }
        }

        // Add New Rental Object 
        public async Task<ServiceResult> AddRentalAsync(RentalProductDetailsModel model, int factoryId, CancellationToken ct = default)
        {
            try
            {
                var resolved = await ResolveRentalLocationAsync(model, ct);
                if (!resolved.ok)
                    return ServiceResult.Fail(resolved.error ?? "Invalid location.");

                var entity = new RentalStore
                {
                    OwnerID = factoryId,

                    // ✅ Address + Lat/Lng
                    Address = resolved.normalizedAddress?.Trim(),
                    Latitude = resolved.lat,
                    Longitude = resolved.lng,

                    Area = model.Area,
                    PricePerMonth = model.PricePerMonth,
                    Description = model.Description?.Trim(),

                    AvailableFrom = model.AvailableFrom,
                    AvailableUntil = model.AvailableUntil,

                    Condition = model.Condition,
                    Status = ProductStatus.Available,

                    IsFurnished = model.IsFurnished,
                    HasElectricity = model.HasElectricity,
                    HasWater = model.HasWater
                };

                // ✅ Upload images (same style as materials)
                if (model.RentalImage1 != null && model.RentalImage1.Length > 0)
                    entity.RentalImgURL1 = await _imageStorage.UploadAsync(model.RentalImage1, "rentals");

                if (model.RentalImage2 != null && model.RentalImage2.Length > 0)
                    entity.RentalImgURL2 = await _imageStorage.UploadAsync(model.RentalImage2, "rentals");

                if (model.RentalImage3 != null && model.RentalImage3.Length > 0)
                    entity.RentalImgURL3 = await _imageStorage.UploadAsync(model.RentalImage3, "rentals");

                _db.RentalStores.Add(entity);
                await _db.SaveChangesAsync(ct);

                return ServiceResult.Ok("Rental property added successfully!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding rental");
                return ServiceResult.Fail("Failed to add rental property. Please try again.");
            }
        }

        // Update Data Rental Object by Rules
        public async Task<ServiceResult> UpdateRentalAsync(int id, RentalProductDetailsModel model, int factoryId, CancellationToken ct = default)
        {
            try
            {
                var rental = await _db.RentalStores
                    .FirstOrDefaultAsync(r => r.RentalID == id && r.OwnerID == factoryId, ct);

                if (rental == null)
                    return ServiceResult.Fail("Rental not found or you don't have permission.");

                if(rental.Status == ProductStatus.RemovedByFactory)
                    return ServiceResult.Fail("Rental was Deleted by Owner Can not update data");

                var hasNonCanceledOrders = await _db.RentalOrders
                    .AsNoTracking()
                    .AnyAsync(o => o.RentalOrderID == id && o.Status != EnumsOrderStatus.Cancelled, ct);

                // If has orders => Description only
                if (hasNonCanceledOrders)
                {
                    // normalize incoming description
                    var incomingDesc = (model.Description ?? "").Trim();
                    var oldDesc = (rental.Description ?? "").Trim();

                    bool descriptionChanged = !string.Equals(oldDesc, incomingDesc, StringComparison.Ordinal);

                    // detect if user attempted to change any locked fields
                    bool attemptedLockedChanges =
                        rental.Status != model.Status ||
                        !string.Equals((rental.Address ?? "").Trim(), (model.Location ?? model.Location ?? "").Trim(), StringComparison.Ordinal) ||
                        rental.Latitude != model.Latitude ||
                        rental.Longitude != model.Longitude ||
                        rental.Area != model.Area ||
                        rental.PricePerMonth != model.PricePerMonth ||
                        rental.AvailableFrom != model.AvailableFrom ||
                        rental.AvailableUntil != model.AvailableUntil ||
                        rental.Condition != model.Condition ||
                        rental.IsFurnished != model.IsFurnished ||
                        rental.HasElectricity != model.HasElectricity ||
                        rental.HasWater != model.HasWater ||
                        (model.RentalImage1 != null && model.RentalImage1.Length > 0) ||
                        (model.RentalImage2 != null && model.RentalImage2.Length > 0) ||
                        (model.RentalImage3 != null && model.RentalImage3.Length > 0);

                    // Save Description only (if changed)
                    if (descriptionChanged)
                    {
                        rental.Description = incomingDesc;
                        await _db.SaveChangesAsync(ct);

                        // message rule:
                        // - Desc only => Success
                        // - Desc + locked attempts => Warning
                        if (attemptedLockedChanges)
                        {
                            return ServiceResult.Fail("Description saved. Other changes were ignored because this rental has active orders.");
                        }

                        return ServiceResult.Ok("Rental description updated successfully!");
                    }

                    // No description change:
                    if (attemptedLockedChanges)
                    {
                        return ServiceResult.Fail("This rental has active orders. Only description can be edited. No description change detected, so nothing was saved.");
                    }

                    return ServiceResult.Ok("No changes detected.");
                }

                // No orders => Full edit allowed
                rental.Status = model.Status;

                var resolved = await ResolveRentalLocationAsync(model, ct);
                if (!resolved.ok)
                    return ServiceResult.Fail(resolved.error ?? "Invalid location.");

                rental.Address = resolved.normalizedAddress?.Trim();
                rental.Latitude = resolved.lat;
                rental.Longitude = resolved.lng;

                rental.Area = model.Area;
                rental.PricePerMonth = model.PricePerMonth;
                rental.Description = model.Description?.Trim();

                rental.AvailableFrom = model.AvailableFrom;
                rental.AvailableUntil = model.AvailableUntil;

                rental.Condition = model.Condition;
                rental.IsFurnished = model.IsFurnished;
                rental.HasElectricity = model.HasElectricity;
                rental.HasWater = model.HasWater;

                if (model.RentalImage1 != null && model.RentalImage1.Length > 0)
                    rental.RentalImgURL1 = await _imageStorage.ReplaceAsync(model.RentalImage1, "rentals", rental.RentalImgURL1);

                if (model.RentalImage2 != null && model.RentalImage2.Length > 0)
                    rental.RentalImgURL2 = await _imageStorage.ReplaceAsync(model.RentalImage2, "rentals", rental.RentalImgURL2);

                if (model.RentalImage3 != null && model.RentalImage3.Length > 0)
                    rental.RentalImgURL3 = await _imageStorage.ReplaceAsync(model.RentalImage3, "rentals", rental.RentalImgURL3);

                await _db.SaveChangesAsync(ct);
                return ServiceResult.Ok("Rental property updated successfully!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating rental");
                return ServiceResult.Fail("Failed to update rental property. Please try again.");
            }
        }

        // Get All Rental Orders about all Rentals
        public async Task<List<RentalOrderDetailsModel>> GetRentalOrdersForFactoryAsync(int ownerId, int take = 300, CancellationToken ct = default)
        {
            var rows = await (
                from r in _db.RentalStores.AsNoTracking()
                where r.OwnerID == ownerId
                join o in _db.RentalOrders.AsNoTracking() on r.RentalID equals o.RentalStoreID
                group o by new
                {
                    r.RentalID,
                    r.Address,
                    r.PricePerMonth,
                    r.AvailableFrom,
                    r.AvailableUntil,
                    r.RentalImgURL1,
                    r.RentalImgURL2,
                    r.RentalImgURL3
                } into g
                orderby g.Max(x => x.OrderDate) descending
                select new RentalOrderDetailsModel
                {
                    RentalId = g.Key.RentalID,
                    RentalAddress = g.Key.Address,
                    PricePerMonth = g.Key.PricePerMonth,
                    AvailableFrom = g.Key.AvailableFrom,
                    AvailableUntil = g.Key.AvailableUntil,

                    ImageUrl = (g.Key.RentalImgURL1 ?? g.Key.RentalImgURL2 ?? g.Key.RentalImgURL3),

                    OrdersCount = g.Count(),
                    PendingCount = g.Count(x => x.Status == EnumsOrderStatus.Pending),
                    ConfirmedCount = g.Count(x => x.Status == EnumsOrderStatus.Confirmed)
                }
            ).Take(take).ToListAsync(ct);

            return rows;
        }

        // Get All Rental Orders Details
        public async Task<List<RentalOrderDetailsModel>> GetRentalOrderDetailsOwnerAsync(int ownerId, int rentalId, int take = 400, CancellationToken ct = default)
        {

            try
            {
                var rental = await _db.RentalStores
                    .AsNoTracking()
                    .Include(r => r.Owner)
                    .FirstOrDefaultAsync(r => r.RentalID == rentalId && r.OwnerID == ownerId, ct);

                if (rental == null)
                    return new List<RentalOrderDetailsModel>();

                var imgs = new[] { rental.RentalImgURL1, rental.RentalImgURL2, rental.RentalImgURL3 }
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => x!.Trim())
                    .Distinct()
                    .ToList();

                var rows = await _db.RentalOrders
                    .AsNoTracking()
                    .Where(o => o.RentalStoreID == rentalId)
                    .OrderByDescending(o => o.OrderDate)
                    .Take(take)
                    .Join(_db.Users.AsNoTracking(),
                        o => o.BuyerID,
                        u => u.UserId,
                        (o, u) => new { o, u })
                    .ToListAsync(ct);

                var result = rows.Select(x =>
                {
                    var o = x.o;
                    var u = x.u;

                    return new RentalOrderDetailsModel
                    {
                        RentalId = rental.RentalID,
                        RentalAddress = rental.Address,
                        PricePerMonth = rental.PricePerMonth,
                        AvailableFrom = rental.AvailableFrom,
                        AvailableUntil = rental.AvailableUntil,
                        ImageUrl = imgs.FirstOrDefault(),
                        ImageUrls = imgs,

                        // Order info
                        RentalOrderId = o.RentalOrderID,
                        Status = o.Status.ToString(),
                        OrderDate = o.OrderDate,

                        // Payments
                        TotalPaid = o.AmountPaid,
                        HeldAmount = o.HeldAmount,

                        // Buyer info
                        BuyerId = u.UserId,
                        BuyerName = u.FullName ?? "Buyer",
                        BuyerProfileImgUrl = u.UserProfileImgURL,

                        BuyerVerified = u.Verified,
                        BuyerEmail = u.Email != null ? _dataCiphers.Decrypt(u.Email!) : "",
                        BuyerPhone = u.phoneNumber != null ? _dataCiphers.Decrypt(u.phoneNumber!) : "",
                        BuyerAddress = u.Address
                    };
                }).ToList();

                var ordersCount = result.Count;
                var pendingCount = result.Count(x => x.Status.Equals("Pending", StringComparison.OrdinalIgnoreCase));
                var confirmedCount = result.Count(x => x.Status.Equals("Confirmed", StringComparison.OrdinalIgnoreCase));

                foreach (var vm in result)
                {
                    vm.OrdersCount = ordersCount;
                    vm.PendingCount = pendingCount;
                    vm.ConfirmedCount = confirmedCount;
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetRentalOrderDetailsAsync failed (ownerId={ownerId}, rentalId={rentalId})", ownerId, rentalId);
                return new List<RentalOrderDetailsModel>();
            }
        }

        // Change Status Order Rental Object
        public async Task<ServiceResult> ChangeRentalOrderStatusAsync(int ownerId, int orderId, CancellationToken ct = default)
        {
            await using var tx = await _db.Database.BeginTransactionAsync(ct);

            try
            {
                // Load order + rental + owner profile
                var order = await _db.RentalOrders
                    .Include(o => o.RentalStore)
                        .ThenInclude(r => r.Owner)
                            .ThenInclude(u => u.FactoryProfile)
                    .FirstOrDefaultAsync(o => o.RentalOrderID == orderId, ct);

                if (order == null || order.RentalStore == null)
                    return ServiceResult.Fail("Order not found.");

                var rental = order.RentalStore;

                if (rental.OwnerID != ownerId)
                    return ServiceResult.Fail("No permission.");

                var current = order.Status;

                bool isClosed(EnumsOrderStatus s) =>
                    s == EnumsOrderStatus.Cancelled
                    || s == EnumsOrderStatus.Completed
                    || s == EnumsOrderStatus.Refunded
                    || s == EnumsOrderStatus.Returned
                    || s == EnumsOrderStatus.DeletedByBuyer
                    || s == EnumsOrderStatus.DeletedBySeller;

                if (isClosed(current))
                    return ServiceResult.Fail($"Cannot change status because order is closed ({current}).");

                // Decide next status automatically
                // Pending -> Confirmed
                // Confirmed -> Refunded
                EnumsOrderStatus targetStatus;

                if (current == EnumsOrderStatus.Pending)
                    targetStatus = EnumsOrderStatus.Confirmed;
                else if (current == EnumsOrderStatus.Confirmed)
                    targetStatus = EnumsOrderStatus.Refunded;
                else
                    return ServiceResult.Fail($"This action is allowed only for Pending/Confirmed. Current: {current}");

                // Confirm Settings
                if (targetStatus == EnumsOrderStatus.Confirmed)
                {
                    if (rental.Status != ProductStatus.Available)
                        return ServiceResult.Fail("Rental is no longer available.");

                    var alreadyConfirmed = await _db.RentalOrders
                        .AsNoTracking()
                        .AnyAsync(o => o.RentalStoreID == rental.RentalID && o.Status == EnumsOrderStatus.Confirmed, ct);

                    if (alreadyConfirmed)
                        return ServiceResult.Fail("Another request already approved.");

                    var ownerWallet = await GetOrCreateWalletAsync(ownerId, ct);

                    // confirm this order + reserve rental
                    order.Status = EnumsOrderStatus.Confirmed;

                    rental.Status = ProductStatus.Reserved;
                    rental.ReservedForOrderId = order.RentalOrderID;
                    rental.ReservedAt = DateTime.UtcNow;

                    // cancel other pending + refund
                    var otherPending = await _db.RentalOrders
                        .Where(o => o.RentalStoreID == rental.RentalID
                                    && o.RentalOrderID != orderId
                                    && o.Status == EnumsOrderStatus.Pending)
                        .ToListAsync(ct);

                    foreach (var p in otherPending)
                    {
                        var refund = Math.Round(p.HeldAmount, 2, MidpointRounding.AwayFromZero);

                        if (refund > 0m)
                        {
                            var buyerWallet = await GetOrCreateWalletAsync(p.BuyerID, ct);

                            await TransferAsync(
                                from: ownerWallet,
                                to: buyerWallet,
                                amount: refund,
                                consumeFromReserved: true,
                                note: $"Refund: rentalId={rental.RentalID} reserved for another buyer (orderId={p.RentalOrderID})",
                                idemKey: $"RENT_REFUND:{p.RentalOrderID}",
                                ct: ct
                            );
                        }

                        p.Status = EnumsOrderStatus.Cancelled;
                        p.CancelledAt = DateTime.UtcNow;
                    }

                    // settlement on confirm
                    var amount = Math.Round(order.HeldAmount, 2, MidpointRounding.AwayFromZero);
                    if (amount <= 0m)
                        return ServiceResult.Fail("Invalid held amount.");

                    if (ownerWallet.ReservedBalance + 0.0001m < amount)
                        return ServiceResult.Fail("Owner wallet reserved balance is insufficient (hold missing).");

                    var pricing = _pricingOptions.Value;
                    var fee = Math.Round(amount * pricing.PlatformFeePercent, 2, MidpointRounding.AwayFromZero);
                    if (fee > amount) fee = amount;

                    var net = Math.Round(amount - fee, 2, MidpointRounding.AwayFromZero);

                    if (net > 0m)
                    {
                        await ReleaseHoldAsync(
                            wallet: ownerWallet,
                            amount: net,
                            note: $"Rental confirm: release net (rentalId={rental.RentalID}, orderId={order.RentalOrderID})",
                            idemKey: $"RENT_REL:{order.RentalOrderID}",
                            ct: ct
                        );
                    }

                    if (fee > 0m)
                    {
                        if (ownerWallet.ReservedBalance + 0.0001m < fee)
                            return ServiceResult.Fail("Owner reserved balance is insufficient to deduct platform fee.");

                        ownerWallet.ReservedBalance = Math.Round(ownerWallet.ReservedBalance - fee, 2, MidpointRounding.AwayFromZero);

                        await AddWalletTxnAsync(
                            wallet: ownerWallet,
                            type: WalletTxnType.Adjustment,
                            amount: -fee,
                            currency: "EGP",
                            note: $"Rental confirm: platform fee deducted (fee={fee:0.00}) rentalId={rental.RentalID}, orderId={order.RentalOrderID}",
                            idempotencyKey: $"RENT_FEE_DEDUCT:{order.RentalOrderID}",
                            ct: ct
                        );
                    }

                    var fp = rental.Owner?.FactoryProfile;
                    if (fp != null)
                    {
                        fp.TotalBalancePercentageRequests =
                            Math.Round((fp.TotalBalancePercentageRequests ?? 0m) + fee, 2, MidpointRounding.AwayFromZero);
                    }

                    await _db.SaveChangesAsync(ct);
                    await tx.CommitAsync(ct);

                    // Send Email
                    var buyerRow = await _db.Users
                        .AsNoTracking()
                        .Where(u => u.UserId == order.BuyerID)
                        .Select(u => new
                        {
                            EmailEncrypted = u.Email,
                            Name = (u.FullName ?? u.Email)
                        })
                        .FirstOrDefaultAsync(ct);

                    string buyerEmail = "";
                    string buyerName = buyerRow?.Name ?? "";

                    if (buyerRow != null && !string.IsNullOrWhiteSpace(buyerRow.EmailEncrypted))
                    {
                        try { buyerEmail = _dataCiphers.Decrypt(buyerRow.EmailEncrypted); }
                        catch { buyerEmail = ""; }
                    }

                    static string HtmlEncode(string? value)
                    {
                        if (string.IsNullOrEmpty(value)) return "";
                        return System.Net.WebUtility.HtmlEncode(value);
                    }

                    var factoryName =
                        rental.Owner?.FactoryProfile?.FactoryName
                        ?? rental.Owner?.FullName
                        ?? EmailTemplateConfig.CompanyName;

                    var signingLocation = EmailTemplateConfig.SupportLocation;

                    var orderLink = $"{EmailTemplateConfig.WebsiteUrl}/CraftsMan/RentalOrderDetails/{order.RentalOrderID}";
                    var rentalLink = $"{EmailTemplateConfig.WebsiteUrl}/CraftsMan/RentalDetails/{rental.RentalID}";

                    var messageHtml = $@"
                        <p>🎉 Congratulations! Your rental request has been <strong>CONFIRMED</strong> ✅</p>

                        <div style='background:#e8f5e9;border:1px solid #c8e6c9;padding:16px;border-radius:10px;margin:16px 0;'>
                          <h3 style='margin:0 0 10px 0;'>✅ Approved</h3>
                          <p style='margin:6px 0;'>Your request is approved by the owner. Please proceed to sign the rental paperwork.</p>
                        </div>

                        <div style='background:#fff;border:1px solid #eee;padding:16px;border-radius:10px;margin:16px 0;'>
                          <h3 style='margin:0 0 10px 0;'>📌 Details</h3>
                          <p style='margin:6px 0;'><strong>Request ID:</strong> #{order.RentalOrderID}</p>
                          <p style='margin:6px 0;'><strong>Status:</strong> {HtmlEncode(order.Status.ToString())}</p>
                          <p style='margin:6px 0;'><strong>Owner/Factory:</strong> {HtmlEncode(factoryName)}</p>
                          <p style='margin:6px 0;'><strong>Rental Address:</strong> {HtmlEncode(rental.Address)}</p>
                        </div>

                        <div style='background:#fff3cd;border-left:4px solid #ffc107;padding:14px;border-radius:8px;margin:16px 0;'>
                          <p style='margin:0;color:#856404;'>
                            <strong>✍️ Contract Signing Location:</strong><br/>
                            Please visit our office/factory to sign the rental papers:<br/>
                            <strong>{HtmlEncode(signingLocation)}</strong>
                          </p>
                        </div>

                        <div style='background:#e3f2fd;border:1px solid #bbdefb;padding:16px;border-radius:10px;margin:16px 0;'>
                          <h3 style='margin:0 0 10px 0;'>🔎 Track</h3>
                          <p style='margin:6px 0;'><a href='{orderLink}' target='_blank'>Open request details</a></p>
                          <p style='margin:6px 0;'><a href='{rentalLink}' target='_blank'>Open rental details</a></p>
                        </div>
                    ";

                    if (!string.IsNullOrWhiteSpace(buyerEmail))
                    {
                        _ = Task.Run(async () =>
                        {
                            try
                            {
                                var mail = _emailTemplateService.CreateNotificationEmail(
                                    email: buyerEmail,
                                    subject: $"Rental Request #{order.RentalOrderID} Confirmed",
                                    message: messageHtml,
                                    userName: buyerName
                                );

                                await _emailTemplateService.SendEmailAsync(mail);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Rental confirmed email failed");
                            }
                        });
                    }
                    return ServiceResult.Ok("Order confirmed successfully.");
                }

                // Cancel/Reject after Confirm (NO REFUND) + Delete all related orders
                if (targetStatus == EnumsOrderStatus.Refunded)
                {
                    // Refund
                    order.Status = EnumsOrderStatus.Refunded;
                    order.CancelledAt = DateTime.UtcNow;

                    //The rental service will be available again if booked for the same request
                    if (rental.ReservedForOrderId == order.RentalOrderID)
                    {
                        rental.Status = ProductStatus.Available;
                        rental.ReservedForOrderId = null;
                        rental.ReservedAt = null;
                    }

                    // Delete all orders on the same Rental account
                    var ordersToDelete = await _db.RentalOrders
                        .Where(o => o.RentalStoreID == rental.RentalID
                                    && o.RentalOrderID != order.RentalOrderID)
                        .ToListAsync(ct);

                    _db.RentalOrders.RemoveRange(ordersToDelete);

                    await _db.SaveChangesAsync(ct);
                    await tx.CommitAsync(ct);

                    // Send Email
                    var buyerRow = await _db.Users
                        .AsNoTracking()
                        .Where(u => u.UserId == order.BuyerID)
                        .Select(u => new
                        {
                            EmailEncrypted = u.Email,
                            Name = (u.FullName ?? u.Email)
                        })
                        .FirstOrDefaultAsync(ct);

                    string buyerEmail = "";
                    string buyerName = buyerRow?.Name ?? "";

                    if (buyerRow != null && !string.IsNullOrWhiteSpace(buyerRow.EmailEncrypted))
                    {
                        try { buyerEmail = _dataCiphers.Decrypt(buyerRow.EmailEncrypted); }
                        catch { buyerEmail = ""; }
                    }

                    static string HtmlEncode(string? value)
                    {
                        if (string.IsNullOrEmpty(value)) return "";
                        return System.Net.WebUtility.HtmlEncode(value);
                    }

                    var orderLink = $"{EmailTemplateConfig.WebsiteUrl}/CraftsMan/RentalOrderDetails/{order.RentalOrderID}";

                    var messageHtml = $@"
                        <p>⚠️ Your rental request status has been updated.</p>

                        <div style='background:#f8d7da;border:1px solid #f5c6cb;padding:16px;border-radius:10px;margin:16px 0;'>
                          <h3 style='margin:0 0 10px 0;'>Request Cancelled</h3>
                          <p style='margin:6px 0;'>Your request has been marked as <strong>REFUNDED</strong> (as per current flow: <strong>No refund</strong>).</p>
                          <p style='margin:6px 0;'>If you think this is a mistake, please contact support.</p>
                        </div>

                        <div style='background:#fff;border:1px solid #eee;padding:16px;border-radius:10px;margin:16px 0;'>
                          <p style='margin:6px 0;'><strong>Request ID:</strong> #{order.RentalOrderID}</p>
                          <p style='margin:6px 0;'><strong>Status:</strong> {HtmlEncode(order.Status.ToString())}</p>
                        </div>

                        <div style='background:#e3f2fd;border:1px solid #bbdefb;padding:16px;border-radius:10px;margin:16px 0;'>
                          <p style='margin:6px 0;'><a href='{orderLink}' target='_blank'>Open request details</a></p>
                        </div>
                    ";

                    if (!string.IsNullOrWhiteSpace(buyerEmail))
                    {
                        _ = Task.Run(async () =>
                        {
                            try
                            {
                                var mail = _emailTemplateService.CreateNotificationEmail(
                                    email: buyerEmail,
                                    subject: $"Rental Request #{order.RentalOrderID} Update: {order.Status}",
                                    message: messageHtml,
                                    userName: buyerName
                                );

                                await _emailTemplateService.SendEmailAsync(mail);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Rental status email failed");
                            }
                        });
                    }

                    return ServiceResult.Ok("Order cancelled (no refund) and all related orders deleted.");
                }

                return ServiceResult.Fail("Unexpected flow.");
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync(ct);
                _logger.LogError(ex, "ChangeRentalOrderStatusAsync failed");
                return ServiceResult.Fail("Failed to change rental order status.");
            }
        }

        // Get Some Details about Owner and Rental Order Details
        public async Task<List<RentalOrderDetailsModel>> GetOrdersForRentalForOwnerAsync(int ownerId, int rentalId, CancellationToken ct = default)
        {
            var isMine = await _db.RentalStores.AsNoTracking()
                .AnyAsync(r => r.RentalID == rentalId && r.OwnerID == ownerId, ct);

            if (!isMine) return new List<RentalOrderDetailsModel>();

            return await (
                from o in _db.RentalOrders.AsNoTracking()
                join r in _db.RentalStores.AsNoTracking() on o.RentalStoreID equals r.RentalID
                join u in _db.Users.AsNoTracking() on o.BuyerID equals u.UserId
                where o.RentalStoreID == rentalId && r.OwnerID == ownerId
                orderby o.OrderDate descending
                select new RentalOrderDetailsModel
                {
                    RentalOrderId = o.RentalOrderID,
                    RentalId = r.RentalID,

                    RentalAddress = r.Address,
                    PricePerMonth = r.PricePerMonth,
                    AvailableFrom = r.AvailableFrom,
                    AvailableUntil = r.AvailableUntil,

                    BuyerId = u.UserId,
                    BuyerName = u.FullName!,
                    BuyerProfileImgUrl = u.UserProfileImgURL,

                    Status = o.Status.ToString(),
                    OrderDate = o.OrderDate
                }
            ).ToListAsync(ct);
        }

        // Delete Rental Object From Store
        public async Task<ServiceResult> DeleteRentalAsync(int id, int factoryId, CancellationToken ct = default)
        {
            await using var tx = await _db.Database.BeginTransactionAsync(ct);

            try
            {
                var rental = await _db.RentalStores
                    .FirstOrDefaultAsync(r => r.RentalID == id && r.OwnerID == factoryId, ct);

                if (rental == null)
                    return ServiceResult.Fail("Rental not found or you don't have permission.");

                // Get all orders for this rental (tracked)
                var orders = await _db.RentalOrders
                    .Where(o => o.RentalStoreID == id)
                    .ToListAsync(ct);

                // No orders => Hard delete (your existing behavior)
                if (orders.Count == 0)
                {
                    var urls = new[] { rental.RentalImgURL1, rental.RentalImgURL2, rental.RentalImgURL3 }
                        .Where(x => !string.IsNullOrWhiteSpace(x))
                        .Select(x => x!.Trim())
                        .Distinct()
                        .ToList();

                    _db.RentalStores.Remove(rental);
                    await _db.SaveChangesAsync(ct);

                    // delete from storage AFTER db commit is ok, but we are in tx => do it after commit
                    await tx.CommitAsync(ct);

                    foreach (var url in urls)
                        await _imageStorage.DeleteAsync(url);

                    return ServiceResult.Ok("Rental deleted successfully!");
                }

                // Has orders => Strict rules Allow only if ALL orders are Pending or Cancelled
                bool isAllowedToHideOnly(EnumsOrderStatus s) =>
                    s == EnumsOrderStatus.Pending ||
                    s == EnumsOrderStatus.Refunded ||
                    s == EnumsOrderStatus.Cancelled;

                var hasBlockingOrders = orders.Any(o => !isAllowedToHideOnly(o.Status));
                if (hasBlockingOrders)
                {
                    // Strict protection: do NOT hide/delete if any Confirmed/Completed/... exists
                    var badStatuses = string.Join(", ", orders
                        .Where(o => !isAllowedToHideOnly(o.Status))
                        .Select(o => o.Status)
                        .Distinct());

                    return ServiceResult.Fail($"Cannot delete/hide this rental because it has orders in these statuses: {badStatuses}");
                }

                // Cancel all Pending orders + Refund their held amounts
                var pendingOrders = orders.Where(o => o.Status == EnumsOrderStatus.Pending).ToList();

                if (pendingOrders.Count > 0)
                {
                    // owner wallet (factory)
                    var ownerWallet = await GetOrCreateWalletAsync(factoryId, ct);

                    foreach (var o in pendingOrders)
                    {
                        var refund = Math.Round(o.HeldAmount, 2, MidpointRounding.AwayFromZero);

                        if (refund > 0m)
                        {
                            var buyerWallet = await GetOrCreateWalletAsync(o.BuyerID, ct);

                            // Refund from owner's RESERVED balance (because Pending holds are reserved)
                            await TransferAsync(
                                from: ownerWallet,
                                to: buyerWallet,
                                amount: refund,
                                consumeFromReserved: true,
                                note: $"Refund: rental listing removed by owner (rentalId={id}, orderId={o.RentalOrderID})",
                                idemKey: $"RENT_DEL_REFUND:{o.RentalOrderID}",
                                ct: ct
                            );
                        }

                        // cancel it
                        o.Status = EnumsOrderStatus.Cancelled;
                        o.CancelledAt = DateTime.UtcNow;
                    }
                }

                // Mark orders as deleted-for-buyer (use existing enum)
                    // - Buyers won't see it (your buyer query can exclude DeletedBySeller)
                        // - Factory still can see it
                foreach (var o in orders)
                {
                    o.Status = EnumsOrderStatus.DeletedBySeller;
                }

                // Hide rental (soft delete)
                // If you can't add fields now, at least:
                rental.Status = ProductStatus.RemovedByFactory;

                // clear reservation
                rental.ReservedForOrderId = null;
                rental.ReservedAt = null;

                await _db.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);

                return ServiceResult.Ok("Rental removed successfully. Pending orders were cancelled and refunded.");
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync(ct);
                _logger.LogError(ex, "DeleteRentalAsync failed");
                return ServiceResult.Fail("Failed to delete/hide rental. Please try again.");
            }
        }

        ////////////////////////////////////////// Rental Methods ////////////////////////////////////
        /////////////////////////////// User Method ///////////////////////////////

        // Get All Rental objects with state Available from database to store
        public async Task<List<FactoryStoreModel>> GetPublicRentalsAsync(SearchFilterModel? filter = null)
        {
            try
            {
                IQueryable<RentalStore> q = _db.RentalStores
                    .AsNoTracking()
                    .Include(x => x.Owner)
                    .Where(x => x.Status == ProductStatus.Available && x.Owner.Verified);

                if (filter != null)
                {
                    if (!string.IsNullOrWhiteSpace(filter.Keyword))
                    {
                        var kw = filter.Keyword.Trim();
                        q = q.Where(x =>
                            (x.Address != null && EF.Functions.Like(x.Address, $"%{kw}%")) ||
                            (x.Description != null && EF.Functions.Like(x.Description, $"%{kw}%")));
                    }

                    if (!string.IsNullOrWhiteSpace(filter.Location))
                    {
                        var loc = filter.Location.Trim();
                        q = q.Where(x => x.Address != null && EF.Functions.Like(x.Address, $"%{loc}%"));
                    }

                    if (filter.MinPrice.HasValue) q = q.Where(x => x.PricePerMonth >= filter.MinPrice.Value);
                    if (filter.MaxPrice.HasValue) q = q.Where(x => x.PricePerMonth <= filter.MaxPrice.Value);

                    if (filter.DateFrom.HasValue)
                    {
                        var from = filter.DateFrom.Value.Date;
                        q = q.Where(x => x.AvailableFrom.Date <= from);
                    }

                    if (filter.DateTo.HasValue)
                    {
                        var to = filter.DateTo.Value.Date;
                        q = q.Where(x => !x.AvailableUntil.HasValue || x.AvailableUntil.Value.Date >= to);
                    }
                }

                var rows = await q.OrderByDescending(x => x.AvailableFrom)
                    .Select(x => new
                    {
                        x.RentalID,
                        x.Address,
                        x.Area,
                        x.PricePerMonth,
                        x.Description,
                        x.AvailableFrom,
                        x.AvailableUntil,
                        x.Condition,
                        x.IsFurnished,
                        x.HasElectricity,
                        x.HasWater,
                        x.Latitude,
                        x.Longitude,
                        x.RentalImgURL1,
                        x.RentalImgURL2,
                        x.RentalImgURL3,
                        OwnerId = x.OwnerID,
                        OwnerName = x.Owner.FullName,
                        OwnerVerified = x.Owner.Verified,
                        OwnerProfile = x.Owner.UserProfileImgURL
                    })
                    .ToListAsync();

                return rows.Select(r =>
                {
                    var imgs = new[] { r.RentalImgURL1, r.RentalImgURL2, r.RentalImgURL3 }
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => x!.Trim())
                    .Distinct()
                    .ToList();

                    return new FactoryStoreModel
                    {
                        Id = r.RentalID,
                        Type = "Rental",
                        Name = string.IsNullOrWhiteSpace(r.Address) ? "Rental" : r.Address.Trim(),
                        Description = r.Description,
                        Price = r.PricePerMonth,
                        CreatedAt = r.AvailableFrom,
                        Status = ProductStatus.Available.ToString(),
                        Unit = "month",
                        AvailableQuantity = 1,

                        RentalAddress = r.Address,
                        RentalArea = r.Area,
                        AvailableFrom = r.AvailableFrom,
                        AvailableUntil = r.AvailableUntil,
                        RentalCondition = r.Condition?.ToString(),
                        IsFurnished = r.IsFurnished,
                        HasElectricity = r.HasElectricity,
                        HasWater = r.HasWater,
                        RentalLatitude = r.Latitude,
                        RentalLongitude = r.Longitude,

                        ImageUrl = imgs.FirstOrDefault(),
                        ImageUrls = imgs,

                        SellerUserId = r.OwnerId,
                        SellerName = r.OwnerName,
                        IsVerifiedSeller = r.OwnerVerified,
                        SellerProfileImgUrl = r.OwnerProfile
                    };
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetPublicRentalsAsync failed");
                return new();
            }
        }

        // Get Details about Rental object by id 
        public async Task<FactoryStoreModel?> GetPublicRentalDetailsAsync(int rentalId, int? viewerUserId = null)
        {
            try
            {
                var row = await _db.RentalStores
                    .AsNoTracking()
                    .Where(r =>
                        r.RentalID == rentalId &&
                        r.Owner != null &&
                        r.Owner.Verified
                    )
                    .Select(r => new
                    {
                        // Rental
                        r.RentalID,
                        r.OwnerID,
                        r.Status,
                        r.ReservedForOrderId,

                        r.Address,
                        r.Latitude,
                        r.Longitude,
                        r.Area,
                        r.PricePerMonth,
                        r.Description,
                        r.AvailableFrom,
                        r.AvailableUntil,
                        r.Condition,
                        r.IsFurnished,
                        r.HasElectricity,
                        r.HasWater,

                        r.RentalImgURL1,
                        r.RentalImgURL2,
                        r.RentalImgURL3,

                        // Owner
                        OwnerName = r.Owner.FullName,
                        OwnerVerified = r.Owner.Verified,
                        OwnerProfile = r.Owner.UserProfileImgURL,

                        // Factory default location
                        FactoryAddress = r.Owner.Address,
                        FactoryLat = r.Owner.Latitude,
                        FactoryLng = r.Owner.Longitude,

                        OwnerEmail = r.Owner.Email,
                        OwnerPhone = r.Owner.phoneNumber,

                        // Factory profile
                        FactoryName = r.Owner.FactoryProfile != null ? r.Owner.FactoryProfile.FactoryName : null,
                        FactoryImg1 = r.Owner.FactoryProfile != null ? r.Owner.FactoryProfile.FactoryImgURL1 : null,
                        FactoryImg2 = r.Owner.FactoryProfile != null ? r.Owner.FactoryProfile.FactoryImgURL2 : null,
                        FactoryImg3 = r.Owner.FactoryProfile != null ? r.Owner.FactoryProfile.FactoryImgURL3 : null,

                        // OrdersCount
                        OrdersCount = _db.RentalOrders.Count(o =>
                            o.RentalStoreID == r.RentalID &&
                            o.Status != EnumsOrderStatus.Cancelled &&
                            o.Status != EnumsOrderStatus.DeletedByBuyer &&
                            o.Status != EnumsOrderStatus.DeletedBySeller
                        )
                    })
                    .FirstOrDefaultAsync();

                if (row == null) return null;

                // Images
                var rentalImgs = new[] { row.RentalImgURL1, row.RentalImgURL2, row.RentalImgURL3 }
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => x!.Trim())
                    .Distinct()
                    .ToList();

                var factoryImgs = new[] { row.FactoryImg1, row.FactoryImg2, row.FactoryImg3 }
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => x!.Trim())
                    .Distinct()
                    .ToList();

                bool isPrivate = row.Status == ProductStatus.Reserved;

                if (viewerUserId.HasValue && viewerUserId.Value == row.OwnerID)
                    isPrivate = false;

                var safeRentalAddress = isPrivate ? null : row.Address;
                var safeRentalLat = isPrivate ? null : row.Latitude;
                var safeRentalLng = isPrivate ? null : row.Longitude;

                return new FactoryStoreModel
                {
                    // Rental listing
                    Id = row.RentalID,
                    Type = "Rental",
                    Name = string.IsNullOrWhiteSpace(row.Address) ? "Rental" : row.Address.Trim(),
                    Description = isPrivate
                        ? "This rental is reserved. Details are available only for the accepted requester."
                        : row.Description,

                    Price = row.PricePerMonth,
                    PricePerMonth = row.PricePerMonth,
                    CreatedAt = row.AvailableFrom,
                    Status = row.Status.ToString(),
                    Unit = "month",
                    AvailableQuantity = 1,

                    // Rental location
                    RentalAddress = safeRentalAddress,
                    RentalLatitude = safeRentalLat,
                    RentalLongitude = safeRentalLng,

                    RentalArea = row.Area,
                    AvailableFrom = row.AvailableFrom,
                    AvailableUntil = row.AvailableUntil,
                    RentalCondition = row.Condition?.ToString(),
                    IsFurnished = row.IsFurnished,
                    HasElectricity = row.HasElectricity,
                    HasWater = row.HasWater,

                    ImageUrl = rentalImgs.FirstOrDefault(),
                    ImageUrls = rentalImgs,

                    // Seller basic
                    SellerUserId = row.OwnerID,
                    SellerName = !string.IsNullOrWhiteSpace(row.FactoryName) ? row.FactoryName : row.OwnerName,
                    IsVerifiedSeller = row.OwnerVerified,
                    SellerProfileImgUrl = row.OwnerProfile,

                    // Factory details
                    FactoryName = row.FactoryName,
                    FactoryImageUrls = factoryImgs,

                    // Factory location
                    FactoryAddress = row.FactoryAddress,
                    FactoryLatitude = row.FactoryLat,
                    FactoryLongitude = row.FactoryLng,

                    // Contact
                    SellerEmail = row.OwnerEmail,
                    SellerPhone = row.OwnerPhone,

                    // Orders count
                    OrdersCount = row.OrdersCount,

                    IsPrivate = isPrivate
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetPublicRentalDetailsAsync failed");
                return null;
            }
        }

        // Book Rental Order
        public async Task<ServiceResult> PlaceRentalOrderAsync(int buyerId, int rentalId, decimal amountPaid, decimal walletUsed, string provider, string providerPaymentId, CancellationToken ct = default)
        {
            amountPaid = Math.Round(amountPaid, 2, MidpointRounding.AwayFromZero);
            walletUsed = Math.Round(walletUsed, 2, MidpointRounding.AwayFromZero);

            if (amountPaid <= 0m) return ServiceResult.Fail("Invalid amount.");
            if (walletUsed > amountPaid) return ServiceResult.Fail("WalletUsed cannot exceed PaidAmount.");

            // PricingOptions
            var pricing = _pricingOptions.Value;
            var monthsUpfront = pricing.RentalMonthsUpfront <= 0 ? 3 : pricing.RentalMonthsUpfront;

            if (!string.IsNullOrWhiteSpace(providerPaymentId))
            {
                var txExists = await _db.PaymentTransactions.AsNoTracking()
                    .AnyAsync(p => p.Provider == provider && p.ProviderPaymentId == providerPaymentId, ct);

                if (txExists) return ServiceResult.Fail("Payment already processed.");
            }

            var rental = await _db.RentalStores
                .Include(r => r.Owner)
                .FirstOrDefaultAsync(r => r.RentalID == rentalId && r.Owner.Verified, ct);

            if (rental == null) return ServiceResult.Fail("Rental not found.");
            if (rental.OwnerID == buyerId) return ServiceResult.Fail("You can't order your own listing.");
            if (rental.Status != ProductStatus.Available) return ServiceResult.Fail("Rental is not available.");

            var hasAnyActiveForThisRental = await _db.RentalOrders.AsNoTracking()
                .AnyAsync(o =>
                    o.RentalStoreID == rentalId
                    && o.BuyerID == buyerId
                    && (o.Status == EnumsOrderStatus.Pending || o.Status == EnumsOrderStatus.Confirmed),
                    ct);

            if (hasAnyActiveForThisRental)
                return ServiceResult.Fail("You already have an active request for this rental.");

            // expected upfront amount based on options
            var expected = Math.Round(rental.PricePerMonth * monthsUpfront, 2, MidpointRounding.AwayFromZero);
            if (Math.Abs(expected - amountPaid) > 0.01m)
                return ServiceResult.Fail("Paid amount mismatch.");

            // Make sure there are no more confirmed cases
            var anyConfirmed = await _db.RentalOrders.AsNoTracking()
                .AnyAsync(o => o.RentalStoreID == rentalId && o.Status == EnumsOrderStatus.Confirmed, ct);

            if (anyConfirmed) return ServiceResult.Fail("Rental already reserved.");

            await using var tx = await _db.Database.BeginTransactionAsync(ct);

            try
            {
                long? buyerWalletTxnId = null;

                // Buyer wallet debit
                if (walletUsed > 0m)
                {
                    var buyerWallet = await GetOrCreateWalletAsync(buyerId, ct);

                    var availableBuyer = Math.Round(buyerWallet.Balance - buyerWallet.ReservedBalance, 2, MidpointRounding.AwayFromZero);
                    if (availableBuyer + 0.0001m < walletUsed)
                        return ServiceResult.Fail("Insufficient available wallet balance.");

                    var wtxn = await AddWalletTxnAsync(
                        wallet: buyerWallet,
                        type: WalletTxnType.PaymentDebit,
                        amount: -walletUsed,
                        currency: "EGP",
                        note: $"Rental order wallet payment (rentalId={rentalId})",
                        idempotencyKey: $"RENT_BUYER_PAY:{provider}:{providerPaymentId}",
                        ct: ct
                    );

                    buyerWalletTxnId = wtxn.Id;
                }

                var now = DateTime.UtcNow;

                var order = new RentalOrder
                {
                    RentalStoreID = rentalId,
                    BuyerID = buyerId,
                    Status = EnumsOrderStatus.Pending,
                    OrderDate = now,

                    AmountPaid = amountPaid,
                    MonthsPaid = monthsUpfront,

                    PaymentProvider = provider,
                    PaymentProviderId = providerPaymentId,
                    HeldAmount = amountPaid
                };

                _db.RentalOrders.Add(order);
                await _db.SaveChangesAsync(ct);

                // HOLD in owner wallet
                var ownerWallet = await GetOrCreateWalletAsync(rental.OwnerID, ct);

                await HoldAsync(
                    wallet: ownerWallet,
                    amount: amountPaid,
                    note: $"Rental HOLD (rentalId={rentalId}, orderId={order.RentalOrderID})",
                    idemKey: $"RENT_HOLD:{provider}:{providerPaymentId}",
                    ct: ct
                );

                // PaymentTransaction
                _db.PaymentTransactions.Add(new PaymentTransaction
                {
                    UserId = buyerId,
                    Provider = provider,
                    ProviderPaymentId = providerPaymentId,
                    Amount = amountPaid,
                    Currency = "EGP",
                    Status = PaymentStatus.Succeeded,
                    WalletTransactionId = buyerWalletTxnId,
                    CreatedAt = now
                });

                await _db.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);

                // Send Email
                var buyerRow = await _db.Users
                    .AsNoTracking()
                    .Where(u => u.UserId == buyerId)
                    .Select(u => new
                    {
                        EmailEncrypted = u.Email,
                        Name = (u.FullName ?? u.Email)
                    })
                    .FirstOrDefaultAsync(ct);

                string buyerEmail = "";
                string buyerName = buyerRow?.Name ?? "";

                if (buyerRow != null && !string.IsNullOrWhiteSpace(buyerRow.EmailEncrypted))
                {
                    try { buyerEmail = _dataCiphers.Decrypt(buyerRow.EmailEncrypted); }
                    catch { buyerEmail = ""; }
                }

                var imageUrls = new List<string?> { rental.RentalImgURL1, rental.RentalImgURL2, rental.RentalImgURL3 }
                    .Where(u => !string.IsNullOrWhiteSpace(u))
                    .Select(u => u!)
                    .Distinct()
                    .ToList();

                var imagesHtml = imageUrls.Any()
                    ? "<div style='margin:10px 0;'>" + string.Join("", imageUrls.Select(u =>
                        $@"<a href='{u}' target='_blank' style='text-decoration:none;'>
                       <img src='{EmailTemplateConfig.WebsiteUrl}/{u}' alt='Rental Image'
                            style='width:110px;height:110px;object-fit:cover;border-radius:10px;border:1px solid #eee;margin:6px;' />
                   </a>"
                      )) + "</div>"
                    : "<p style='color:#6c757d;margin:10px 0;'>No images available for this rental.</p>";

                static string HtmlEncode(string? value)
                {
                    if (string.IsNullOrEmpty(value)) return "";
                    return System.Net.WebUtility.HtmlEncode(value);
                }

                var factoryName =
                    rental.Owner?.FactoryProfile?.FactoryName
                    ?? rental.Owner?.FullName
                    ?? EmailTemplateConfig.CompanyName;

                var signingLocation = rental.Owner?.Address;

                var orderLink = $"{EmailTemplateConfig.WebsiteUrl}/CraftsMan/RentalOrderDetails/{order.RentalOrderID}";
                var rentalLink = $"{EmailTemplateConfig.WebsiteUrl}/CraftsMan/RentalDetails/{rental.RentalID}";

                var messageHtml = $@"
                    <p>✅ Your rental request has been created successfully!</p>
                    <p style='margin:8px 0;'>We’re happy to have you with <strong>{HtmlEncode(EmailTemplateConfig.CompanyName)}</strong> 🌿</p>

                    <div style='background:#e8f5e9;border:1px solid #c8e6c9;padding:16px;border-radius:10px;margin:16px 0;'>
                      <h3 style='margin:0 0 10px 0;'>🎉 Great news!</h3>
                      <p style='margin:6px 0;'>Your payment has been received and your request is now <strong>PENDING approval</strong>.</p>
                      <p style='margin:6px 0;'>Once the owner confirms your request, you’ll receive another email with the next steps.</p>
                    </div>

                    <div style='background:#f8f9fa;border:1px solid #eee;padding:16px;border-radius:10px;margin:16px 0;'>
                      <h3 style='margin:0 0 10px 0;'>🧾 Request Summary</h3>
                      <p style='margin:6px 0;'><strong>Request ID:</strong> #{order.RentalOrderID}</p>
                      <p style='margin:6px 0;'><strong>Status:</strong> {HtmlEncode(order.Status.ToString())}</p>
                      <p style='margin:6px 0;'><strong>Months Paid:</strong> {order.MonthsPaid}</p>
                      <p style='margin:6px 0;'><strong>Amount Paid:</strong> {order.AmountPaid:N2} EGP</p>
                      <p style='margin:6px 0;'><strong>Payment Provider:</strong> {HtmlEncode(order.PaymentProvider)}</p>
                    </div>

                    <div style='background:#fff;border:1px solid #eee;padding:16px;border-radius:10px;margin:16px 0;'>
                      <h3 style='margin:0 0 10px 0;'>📍 Rental Location</h3>
                      <p style='margin:6px 0;'><strong>Address:</strong> {HtmlEncode(rental.Address)}</p>
                      <p style='margin:6px 0;'><strong>Area:</strong> {rental.Area}</p>
                      <p style='margin:6px 0;'><strong>Condition:</strong> {HtmlEncode(rental.Condition?.ToString())}</p>
                      <p style='margin:6px 0;'><strong>Owner/Factory:</strong> {HtmlEncode(factoryName)}</p>
                    </div>

                    <div style='background:#fff3cd;border-left:4px solid #ffc107;padding:14px;border-radius:8px;margin:16px 0;'>
                      <p style='margin:0;color:#856404;'>
                        <strong>✍️ Contract Signing:</strong><br/>
                        Please visit our office / factory location to sign the rental paperwork:<br/>
                        <strong>{HtmlEncode(signingLocation)}</strong>
                      </p>
                    </div>

                    <div style='background:#fff;border:1px solid #eee;padding:16px;border-radius:10px;margin:16px 0;'>
                      <h3 style='margin:0 0 10px 0;'>🖼️ Rental Images</h3>
                      {imagesHtml}
                      <p style='margin:10px 0 0 0;'><a href='{rentalLink}' target='_blank'>Open rental page</a></p>
                    </div>

                    <div style='background:#e3f2fd;border:1px solid #bbdefb;padding:16px;border-radius:10px;margin:16px 0;'>
                      <h3 style='margin:0 0 10px 0;'>🔎 Track your request</h3>
                      <p style='margin:6px 0;'>Follow updates from here:</p>
                      <p style='margin:6px 0;'><a href='{orderLink}' target='_blank'>{orderLink}</a></p>
                    </div>
                ";

                if (!string.IsNullOrWhiteSpace(buyerEmail))
                {
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            var mail = _emailTemplateService.CreateNotificationEmail(
                                email: buyerEmail,
                                subject: $"Rental Request #{order.RentalOrderID} Created",
                                message: messageHtml,
                                userName: buyerName
                            );

                            await _emailTemplateService.SendEmailAsync(mail);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Rental request created email failed");
                        }
                    });
                }
                return ServiceResult.Ok("Rental request created successfully!", order.RentalOrderID);
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync(ct);
                _logger.LogError(ex, "PlaceRentalOrderAsync failed");
                return ServiceResult.Fail("Failed to create rental order.");
            }
        }

        // Get Rental Details before Book
        public async Task<List<RentalOrderDetailsModel>> GetRentalOrdersForBuyerAsync(int buyerId, int take = 200, CancellationToken ct = default)
        {
            var rows = await (
                from o in _db.RentalOrders.AsNoTracking()
                join r in _db.RentalStores.AsNoTracking() on o.RentalStoreID equals r.RentalID
                join owner in _db.Users.AsNoTracking() on r.OwnerID equals owner.UserId
                join buyer in _db.Users.AsNoTracking() on o.BuyerID equals buyer.UserId
                where o.BuyerID == buyerId && !o.HiddenForBuyer
                orderby o.OrderDate descending
                select new
                {
                    // Order
                    RentalOrderId = o.RentalOrderID,
                    Status = o.Status.ToString(),
                    o.OrderDate,

                    // Rental listing
                    RentalId = r.RentalID,
                    RentalAddress = r.Address,
                    r.PricePerMonth,
                    r.AvailableFrom,
                    r.AvailableUntil,

                    // Owner / Buyer
                    OwnerName = owner.FullName,
                    BuyerName = buyer.FullName,

                    // Images from RentalStore
                    r.RentalImgURL1,
                    r.RentalImgURL2,
                    r.RentalImgURL3
                }
            ).Take(take).ToListAsync(ct);

            var result = new List<RentalOrderDetailsModel>(rows.Count);

            foreach (var x in rows)
            {
                // Build ImageUrls list safely
                var imgs = new List<string>(3);

                void AddImg(string? url)
                {
                    if (string.IsNullOrWhiteSpace(url)) return;
                    var t = url.Trim();
                    if (!imgs.Contains(t)) imgs.Add(t);
                }

                AddImg(x.RentalImgURL1);
                AddImg(x.RentalImgURL2);
                AddImg(x.RentalImgURL3);

                result.Add(new RentalOrderDetailsModel
                {
                    RentalOrderId = x.RentalOrderId,
                    RentalId = x.RentalId,
                    RentalAddress = x.RentalAddress,
                    PricePerMonth = x.PricePerMonth,
                    AvailableFrom = x.AvailableFrom,
                    AvailableUntil = x.AvailableUntil,

                    BuyerId = buyerId,

                    BuyerName = x.BuyerName ?? "",

                    BuyerProfileImgUrl = null,

                    Status = x.Status ?? "",
                    OrderDate = x.OrderDate,

                    // default rental term
                    Months = _pricingOptions.Value.RentalMonthsUpfront,

                    // images for the view
                    ImageUrls = imgs
                });
            }

            return result;
        }

        // Get Details about order after Book
        public async Task<(RentalOrder? order, FactoryStoreModel? details)> GetRentalOrderDetailsAsync(int buyerId, int orderId, CancellationToken ct = default)
        {
            var order = await _db.RentalOrders
                .AsNoTracking()
                .Include(o => o.RentalStore).ThenInclude(r => r.Owner)
                .FirstOrDefaultAsync(o => o.RentalOrderID == orderId && o.BuyerID == buyerId, ct);

            if (order == null) return (null, null);

            // reuse your privacy-aware method
            var details = await GetPublicRentalDetailsAsync(order.RentalStoreID, buyerId);
            return (order, details);
        }

        // Cancel or Hide Order after Canceled from my list
        public async Task<ServiceResult> CancelOrDeleteRentalOrderByBuyerAsync(int buyerId, int rentalOrderId, CancellationToken ct = default)
        {
            await using var tx = await _db.Database.BeginTransactionAsync(ct);

            try
            {
                var order = await _db.RentalOrders
                    .Include(o => o.RentalStore)
                    .FirstOrDefaultAsync(o => o.RentalOrderID == rentalOrderId && o.BuyerID == buyerId, ct);

                if (order == null)
                    return ServiceResult.Fail("Order not found.");

                // If already hidden by
                if (order.HiddenForBuyer)
                    return ServiceResult.Ok("Already removed from your list.");

                // If the order is already canceled: Hide it only from the buyer's side
                if (order.Status == EnumsOrderStatus.Cancelled)
                {
                    order.HiddenForBuyer = true;

                    await _db.SaveChangesAsync(ct);
                    await tx.CommitAsync(ct);

                    return ServiceResult.Ok("Removed from your list.");
                }

                // only before Confirm
                if (order.Status != EnumsOrderStatus.Pending)
                    return ServiceResult.Fail("You can cancel/remove this request only before it is confirmed.");

                var rental = order.RentalStore;
                if (rental == null)
                    return ServiceResult.Fail("Rental not found.");

                // Refund for Pending only
                var refund = Math.Round(order.HeldAmount, 2, MidpointRounding.AwayFromZero);

                if (refund > 0m)
                {
                    var ownerWallet = await GetOrCreateWalletAsync(rental.OwnerID, ct);
                    var buyerWallet = await GetOrCreateWalletAsync(buyerId, ct);

                    // Get your money back from the owner's reserved account buyer's
                    await TransferAsync(
                        from: ownerWallet,
                        to: buyerWallet,
                        amount: refund,
                        consumeFromReserved: true,
                        note: $"Refund: buyer cancelled rental request (orderId={order.RentalOrderID}, rentalId={rental.RentalID})",
                        idemKey: $"RENT_BUYER_CANCEL_REF:{order.RentalOrderID}",
                        ct: ct
                    );
                }

                // Mark Cancelled + Hide for buyer
                order.Status = EnumsOrderStatus.Cancelled;
                order.CancelledAt = DateTime.UtcNow;

                await _db.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);

                return ServiceResult.Ok("Request cancelled and removed from your list.");
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync(ct);
                _logger.LogError(ex, "CancelAndDeleteRentalOrderByBuyerAsync failed");
                return ServiceResult.Fail("Failed to cancel/remove request.");
            }
        }

        ////////////////////////////////////////// Auction Methods ////////////////////////////////////
        /////////////////////////////// Factory Method ///////////////////////////////

        public async Task<List<FactoryStoreModel>> GetAuctionsAsync(int factoryId, SearchFilterModel? filter = null)
        {
            try
            {
                IQueryable<AuctionStore> query = _db.AuctionStores
                    .AsNoTracking()
                    .Where(a => a.SellerID == factoryId)
                    .Include(a => a.Seller);

                if (filter != null)
                {
                    if (!string.IsNullOrWhiteSpace(filter.Keyword))
                    {
                        var keyword = filter.Keyword.Trim();
                        query = query.Where(a =>
                            (a.ProductType != null && EF.Functions.Like(a.ProductType, $"%{keyword}%")) ||
                            (a.Description != null && EF.Functions.Like(a.Description, $"%{keyword}%")));
                    }

                    if (!string.IsNullOrWhiteSpace(filter.Status))
                    {
                        if (Enum.TryParse<ProductStatus>(filter.Status.Trim(), true, out var statusEnum))
                            query = query.Where(a => a.Status == statusEnum);
                    }

                    if (filter.MinPrice.HasValue) query = query.Where(a => a.StartPrice >= filter.MinPrice.Value);
                    if (filter.MaxPrice.HasValue) query = query.Where(a => a.StartPrice <= filter.MaxPrice.Value);

                    if (filter.DateFrom.HasValue)
                    {
                        var from = filter.DateFrom.Value.Date;
                        query = query.Where(a => a.StartDate.Date >= from);
                    }

                    if (filter.DateTo.HasValue)
                    {
                        var to = filter.DateTo.Value.Date;
                        query = query.Where(a => a.StartDate.Date <= to);
                    }
                }

                var rows = await query
                    .OrderByDescending(a => a.StartDate)
                    .Select(a => new
                    {
                        Auction = a,

                        OrdersCount = _db.AuctionOrders.Count(o => o.AuctionStoreID == a.AuctionID),
                        ActiveOrdersCount = _db.AuctionOrders.Count(o => o.AuctionStoreID == a.AuctionID
                            && (o.Status == EnumsOrderStatus.Pending || o.Status == EnumsOrderStatus.Confirmed)),

                        Top = _db.AuctionOrders
                            .Where(o => o.AuctionStoreID == a.AuctionID
                                && o.Status != EnumsOrderStatus.Cancelled
                                && o.Status != EnumsOrderStatus.DeletedByBuyer
                                && o.Status != EnumsOrderStatus.DeletedBySeller)
                            .OrderByDescending(o => o.BidAmount)
                            .ThenBy(o => o.OrderDate)
                            .Select(o => new { o.BidAmount, o.OrderDate, o.Winner.FullName })
                            .FirstOrDefault()
                    })
                    .ToListAsync();

                return rows.Select(x =>
                {
                    var a = x.Auction;

                    var images = new[] { a.ProductImgURL1, a.ProductImgURL2, a.ProductImgURL3 }
                        .Where(u => !string.IsNullOrWhiteSpace(u))
                        .Select(u => u!.Trim())
                        .Distinct()
                        .ToList();

                    bool? ended = a.EndDate.HasValue ? (DateTime.UtcNow > a.EndDate.Value) : (bool?)null;
                    int? daysRemaining = a.EndDate.HasValue ? (int?)(a.EndDate.Value.Date - DateTime.UtcNow.Date).TotalDays : null;

                    return new FactoryStoreModel
                    {
                        Id = a.AuctionID,
                        Type = "Auction",
                        Name = a.ProductType,
                        Description = a.Description,
                        Price = a.StartPrice,
                        AvailableQuantity = a.Quantity,
                        Status = a.Status.ToString(),
                        CreatedAt = a.StartDate,

                        AuctionStartDate = a.StartDate,
                        AuctionEndDate = a.EndDate,
                        IsAuctionEnded = ended,
                        DaysRemaining = daysRemaining,

                        ImageUrl = images.FirstOrDefault(),
                        ImageUrls = images,

                        SellerName = a.Seller?.FullName,
                        IsVerifiedSeller = a.Seller?.Verified ?? false,

                        OrdersCount = x.OrdersCount,
                        ActiveOrdersCount = x.ActiveOrdersCount,

                        TopBidAmount = x.Top?.BidAmount,
                        TopBidAt = x.Top?.OrderDate,
                        TopBidderName = x.Top?.FullName,

                        ConfirmedOrderId = a.ConfirmedOrderId,
                        ConfirmedAt = a.ConfirmedAt,
                        AuctionAddress = a.Address,
                        AuctionLatitude = a.Latitude,
                        AuctionLongitude = a.Latitude
                    };
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting auctions");
                return new();
            }
        }

        public async Task<AuctionProductDetailsModel?> GetAuctionByIdAsync(int id, int factoryId)
        {
            try
            {
                var auction = await _db.AuctionStores
                    .AsNoTracking()
                    .FirstOrDefaultAsync(a => a.AuctionID == id && a.SellerID == factoryId);

                if (auction == null) return null;

                return new AuctionProductDetailsModel
                {
                    Id = auction.AuctionID,
                    AuctionType = auction.ProductType,
                    Quantity = auction.Quantity,
                    StartPrice = auction.StartPrice,
                    Description = auction.Description,
                    StartDate = auction.StartDate,
                    EndDate = auction.EndDate,
                    Status = auction.Status,

                    // ✅ current images
                    CurrentImageUrl1 = auction.ProductImgURL1,
                    CurrentImageUrl2 = auction.ProductImgURL2,
                    CurrentImageUrl3 = auction.ProductImgURL3
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting auction by ID");
                return null;
            }
        }

        public async Task<bool> AddAuctionAsync(AuctionProductDetailsModel model, int factoryUserId)
        {
            try
            {
                var factoryUser = await _db.Users.AsNoTracking()
                    .FirstOrDefaultAsync(u => u.UserId == factoryUserId);

                if (factoryUser == null) return false;

                bool factoryHasLocation =
                    !string.IsNullOrWhiteSpace(factoryUser.Address) ||
                    (factoryUser.Latitude.HasValue && factoryUser.Longitude.HasValue);

                // Decide location source
                string? address = null;
                decimal? lat = null;
                decimal? lng = null;

                if (model.UseFactoryLocation)
                {
                    if (!factoryHasLocation) return false;

                    address = factoryUser.Address;
                    lat = factoryUser.Latitude;
                    lng = factoryUser.Longitude;
                }
                else
                {
                    bool hasAddress = !string.IsNullOrWhiteSpace(model.Address);
                    bool hasCoords = model.Latitude.HasValue && model.Longitude.HasValue;

                    if (!hasAddress && !hasCoords) return false;

                    address = model.Address?.Trim();
                    lat = model.Latitude;
                    lng = model.Longitude;
                }

                var entity = new AuctionStore
                {
                    SellerID = factoryUserId,

                    ProductType = model.AuctionType?.Trim(),
                    Quantity = model.Quantity,
                    StartPrice = model.StartPrice,
                    Description = model.Description?.Trim(),

                    StartDate = model.StartDate,
                    EndDate = model.EndDate,

                    Status = ProductStatus.Available,

                    // Location
                    Address = address,
                    Latitude = lat,
                    Longitude = lng,

                    // Optional initial bid state
                    CurrentTopBid = null,
                    CurrentTopBidderId = null,
                    ConfirmedOrderId = null,
                    ConfirmedAt = null
                };

                // Upload images
                if (model.AuctionImage1 != null && model.AuctionImage1.Length > 0)
                    entity.ProductImgURL1 = await _imageStorage.UploadAsync(model.AuctionImage1, "auctions");

                if (model.AuctionImage2 != null && model.AuctionImage2.Length > 0)
                    entity.ProductImgURL2 = await _imageStorage.UploadAsync(model.AuctionImage2, "auctions");

                if (model.AuctionImage3 != null && model.AuctionImage3.Length > 0)
                    entity.ProductImgURL3 = await _imageStorage.UploadAsync(model.AuctionImage3, "auctions");

                _db.AuctionStores.Add(entity);
                await _db.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding auction");
                return false;
            }
        }

        public async Task<bool> UpdateAuctionAsync(int id, AuctionProductDetailsModel model, int factoryId)
        {
            try
            {
                var auction = await _db.AuctionStores
                    .FirstOrDefaultAsync(a => a.AuctionID == id && a.SellerID == factoryId);

                if (auction == null) return false;

                // ✅ Status editable always
                auction.Status = model.Status;

                // ✅ update fields
                auction.ProductType = model.AuctionType?.Trim();
                auction.Quantity = model.Quantity;
                auction.StartPrice = model.StartPrice;
                auction.Description = model.Description?.Trim();
                auction.StartDate = model.StartDate;
                auction.EndDate = model.EndDate;

                // ✅ Replace images only if new uploaded
                if (model.AuctionImage1 != null && model.AuctionImage1.Length > 0)
                    auction.ProductImgURL1 = await _imageStorage.ReplaceAsync(model.AuctionImage1, "auctions", auction.ProductImgURL1);

                if (model.AuctionImage2 != null && model.AuctionImage2.Length > 0)
                    auction.ProductImgURL2 = await _imageStorage.ReplaceAsync(model.AuctionImage2, "auctions", auction.ProductImgURL2);

                if (model.AuctionImage3 != null && model.AuctionImage3.Length > 0)
                    auction.ProductImgURL3 = await _imageStorage.ReplaceAsync(model.AuctionImage3, "auctions", auction.ProductImgURL3);

                await _db.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating auction");
                return false;
            }
        }

        public async Task<ServiceResult> ConfirmAuctionWinnerAsync(int ownerId, int auctionId, int? winnerOrderId = null, CancellationToken ct = default)
        {
            await using var tx = await _db.Database.BeginTransactionAsync(ct);

            try
            {
                var auction = await _db.AuctionStores
                    .Include(a => a.Seller)
                    .FirstOrDefaultAsync(a => a.AuctionID == auctionId && a.SellerID == ownerId, ct);

                if (auction == null) return ServiceResult.Fail("Auction not found or no permission.");
                if (auction.Status != ProductStatus.Available) return ServiceResult.Fail("Auction is not available.");
                if (!auction.EndDate.HasValue) return ServiceResult.Fail("Auction end date is not set.");

                var nowUtc = DateTime.UtcNow;
                if (nowUtc < auction.EndDate.Value)
                    return ServiceResult.Fail("You can confirm winner only after auction end time.");

                // Prevent double confirm
                if (auction.ConfirmedOrderId.HasValue)
                    return ServiceResult.Fail("Auction already confirmed.");

                // pick winner order:
                AuctionOrder? winner;
                if (winnerOrderId.HasValue)
                {
                    winner = await _db.AuctionOrders
                        .Include(o => o.Winner)
                        .FirstOrDefaultAsync(o => o.AuctionOrderID == winnerOrderId.Value && o.AuctionStoreID == auctionId, ct);
                }
                else
                {
                    winner = await _db.AuctionOrders
                        .Include(o => o.Winner)
                        .Where(o => o.AuctionStoreID == auctionId
                                    && o.Status == EnumsOrderStatus.Pending)
                        .OrderByDescending(o => o.BidAmount)
                        .ThenBy(o => o.OrderDate)
                        .FirstOrDefaultAsync(ct);
                }

                if (winner == null) return ServiceResult.Fail("No pending bids to confirm.");

                // ✅ mark winner confirmed + close auction
                winner.Status = EnumsOrderStatus.Confirmed;
                auction.Status = ProductStatus.Inactive;
                auction.ConfirmedOrderId = winner.AuctionOrderID;
                auction.ConfirmedAt = nowUtc;

                // ✅ refund all other pending bids deposits
                var others = await _db.AuctionOrders
                    .Where(o => o.AuctionStoreID == auctionId
                                && o.AuctionOrderID != winner.AuctionOrderID
                                && o.Status == EnumsOrderStatus.Pending)
                    .ToListAsync(ct);

                if (others.Count > 0)
                {
                    var sellerWallet = await GetOrCreateWalletAsync(ownerId, ct);

                    foreach (var o in others)
                    {
                        var refund = Math.Round(o.HeldAmount, 2, MidpointRounding.AwayFromZero);

                        if (refund > 0m)
                        {
                            var bidderWallet = await GetOrCreateWalletAsync(o.WinnerID, ct);

                            await TransferAsync(
                                from: sellerWallet,
                                to: bidderWallet,
                                amount: refund,
                                consumeFromReserved: true,
                                note: $"Refund: outbid/losing bid (auctionId={auctionId}, orderId={o.AuctionOrderID})",
                                idemKey: $"AUC_REFUND_LOSER:{o.AuctionOrderID}",
                                ct: ct
                            );
                        }

                        o.Status = EnumsOrderStatus.Cancelled;
                        o.CancelledAt = nowUtc;
                    }
                }

                // ✅ settlement on confirm (deposit becomes seller’s money minus platform fee)
                var pricing = _pricingOptions.Value;

                var sellerWallet2 = await GetOrCreateWalletAsync(ownerId, ct);

                var amount = Math.Round(winner.HeldAmount, 2, MidpointRounding.AwayFromZero);
                if (amount <= 0m) return ServiceResult.Fail("Invalid held amount.");

                if (sellerWallet2.ReservedBalance + 0.0001m < amount)
                    return ServiceResult.Fail("Seller reserved balance is insufficient (hold missing).");

                var fee = Math.Round(amount * pricing.PlatformFeePercent, 2, MidpointRounding.AwayFromZero);
                if (fee > amount) fee = amount;

                var net = Math.Round(amount - fee, 2, MidpointRounding.AwayFromZero);

                if (net > 0m)
                {
                    await ReleaseHoldAsync(
                        wallet: sellerWallet2,
                        amount: net,
                        note: $"Auction confirm: release net deposit (auctionId={auctionId}, orderId={winner.AuctionOrderID})",
                        idemKey: $"AUC_REL_NET:{winner.AuctionOrderID}",
                        ct: ct
                    );
                }

                if (fee > 0m)
                {
                    if (sellerWallet2.ReservedBalance + 0.0001m < fee)
                        return ServiceResult.Fail("Seller reserved balance is insufficient to deduct platform fee.");

                    sellerWallet2.ReservedBalance = Math.Round(sellerWallet2.ReservedBalance - fee, 2, MidpointRounding.AwayFromZero);

                    await AddWalletTxnAsync(
                        wallet: sellerWallet2,
                        type: WalletTxnType.Adjustment,
                        amount: -fee,
                        currency: "EGP",
                        note: $"Auction confirm: platform fee deducted (fee={fee:0.00}) auctionId={auctionId}, orderId={winner.AuctionOrderID}",
                        idempotencyKey: $"AUC_FEE:{winner.AuctionOrderID}",
                        ct: ct
                    );
                }

                var fp = auction.Seller?.FactoryProfile;
                if (fp != null)
                {
                    fp.TotalBalancePercentageRequests =
                        Math.Round((fp.TotalBalancePercentageRequests ?? 0m) + fee, 2, MidpointRounding.AwayFromZero);
                }

                await _db.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);

                return ServiceResult.Ok("Winner confirmed successfully.");
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync(ct);
                _logger.LogError(ex, "ConfirmAuctionWinnerAsync failed");
                return ServiceResult.Fail("Failed to confirm auction winner.");
            }
        }

        public async Task<(AuctionOrder? order, FactoryStoreModel? details)> GetAuctionOrderDetailsAsync(int winnerId, int orderId, CancellationToken ct = default)
        {
            var order = await _db.AuctionOrders
                .AsNoTracking()
                .Include(o => o.AuctionStore).ThenInclude(a => a.Seller)
                .FirstOrDefaultAsync(o => o.AuctionOrderID == orderId && o.WinnerID == winnerId, ct);

            if (order == null) return (null, null);

            var details = await GetPublicAuctionDetailsAsync(order.AuctionStoreID); // موجودة عندك
            return (order, details);
        }

        public async Task<List<AuctionOrderDetailsModel>> GetAuctionOrdersForFactoryAsync(int ownerId, int take = 300, CancellationToken ct = default)
        {
            var raw = await (
                from a in _db.AuctionStores.AsNoTracking()
                where a.SellerID == ownerId
                join o in _db.AuctionOrders.AsNoTracking() on a.AuctionID equals o.AuctionStoreID
                group o by new
                {
                    a.AuctionID,
                    a.ProductType,
                    a.StartPrice,
                    a.Quantity,
                    a.StartDate,
                    a.EndDate,
                    a.ProductImgURL1,
                    a.ProductImgURL2,
                    a.ProductImgURL3
                } into g
                orderby g.Max(x => x.OrderDate) descending
                select new
                {
                    AuctionId = g.Key.AuctionID,
                    AuctionType = g.Key.ProductType,
                    StartPrice = g.Key.StartPrice,
                    Quantity = g.Key.Quantity,
                    StartDate = g.Key.StartDate,
                    EndDate = g.Key.EndDate,

                    Img1 = g.Key.ProductImgURL1,
                    Img2 = g.Key.ProductImgURL2,
                    Img3 = g.Key.ProductImgURL3,

                    OrdersCount = g.Count(),
                    PendingCount = g.Count(x => x.Status == EnumsOrderStatus.Pending),
                    ConfirmedCount = g.Count(x => x.Status == EnumsOrderStatus.Confirmed),
                }
            ).Take(take).ToListAsync(ct);

            var rows = raw.Select(x =>
            {
                var imgs = new List<string>(3);

                if (!string.IsNullOrWhiteSpace(x.Img1)) imgs.Add(x.Img1.Trim());
                if (!string.IsNullOrWhiteSpace(x.Img2)) imgs.Add(x.Img2.Trim());
                if (!string.IsNullOrWhiteSpace(x.Img3)) imgs.Add(x.Img3.Trim());

                imgs = imgs.Distinct().ToList();

                return new AuctionOrderDetailsModel
                {
                    AuctionId = x.AuctionId,
                    AuctionType = x.AuctionType,
                    StartPrice = x.StartPrice,
                    Quantity = x.Quantity,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,

                    ImageUrls = imgs,
                    ImageUrl = imgs.FirstOrDefault(),

                    OrdersCount = x.OrdersCount,
                    PendingCount = x.PendingCount,
                    ConfirmedCount = x.ConfirmedCount,
                };
            }).ToList();

            return rows;
        }

        public async Task<List<AuctionOrderDetailsModel>> GetAuctionOrdersByAuctionIdForFactoryAsync(int ownerId, int auctionId, CancellationToken ct = default)
        {
            var raw = await (
                from a in _db.AuctionStores.AsNoTracking()
                where a.SellerID == ownerId && a.AuctionID == auctionId
                join o in _db.AuctionOrders.AsNoTracking() on a.AuctionID equals o.AuctionStoreID
                join u in _db.Users.AsNoTracking() on o.WinnerID equals u.UserId
                orderby o.OrderDate descending
                select new
                {
                    a.AuctionID,
                    AuctionOrderId = o.AuctionOrderID,

                    AuctionType = a.ProductType,
                    a.Quantity,
                    a.StartPrice,
                    a.StartDate,
                    a.EndDate,

                    o.BidAmount,
                    o.AmountPaid,
                    o.HeldAmount,

                    Status = o.Status.ToString(),
                    o.OrderDate,

                    bidderId = u.UserId,
                    bidderName = u.FullName,
                    bidderProfileImgUrl = u.UserProfileImgURL,
                    bidderVerified = u.Verified,
                    bidderEmail = u.Email,
                    bidderPhone = u.phoneNumber,
                    bidderAddress = u.Address,

                    Img1 = a.ProductImgURL1,
                    Img2 = a.ProductImgURL2,
                    Img3 = a.ProductImgURL3,
                }
            ).ToListAsync(ct);

            var rows = raw.Select(x =>
            {
                var imgs = new List<string>(3);

                if (!string.IsNullOrWhiteSpace(x.Img1)) imgs.Add(x.Img1.Trim());
                if (!string.IsNullOrWhiteSpace(x.Img2)) imgs.Add(x.Img2.Trim());
                if (!string.IsNullOrWhiteSpace(x.Img3)) imgs.Add(x.Img3.Trim());

                imgs = imgs.Distinct().ToList();

                var vm = new AuctionOrderDetailsModel
                {
                    AuctionId = x.AuctionID,
                    AuctionOrderId = x.AuctionOrderId,

                    AuctionType = x.AuctionType,
                    Quantity = x.Quantity,
                    StartPrice = x.StartPrice,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,

                    BidAmount = x.BidAmount,
                    AmountPaid = x.AmountPaid,
                    HeldAmount = x.HeldAmount,
                    DepositPaid = (x.AmountPaid - x.HeldAmount),

                    Status = x.Status,
                    OrderDate = x.OrderDate,

                    bidderId = x.bidderId,
                    bidderName = string.IsNullOrWhiteSpace(x.bidderName) ? "Bidder" : x.bidderName,
                    bidderProfileImgUrl = x.bidderProfileImgUrl,
                    bidderVerified = x.bidderVerified,

                    bidderEmail = x.bidderEmail != null ? _dataCiphers.Decrypt(x.bidderEmail!) : "",
                    bidderPhone = x.bidderPhone != null ? _dataCiphers.Decrypt(x.bidderPhone!) : "",
                    bidderAddress = x.bidderAddress,

                    ImageUrls = imgs,
                    ImageUrl = imgs.FirstOrDefault(),
                };

                // UI helpers
                vm.IsEnded = vm.EndDate.HasValue && vm.EndDate.Value <= DateTime.Now;
                vm.CanConfirmNow = vm.IsEnded && string.Equals(vm.Status, "Pending", StringComparison.OrdinalIgnoreCase);

                return vm;
            }).ToList();

            return rows;
        }

        public async Task<List<AuctionOrderDetailsModel>> GetAuctionOrderDetailsOwnerAsync(int ownerId, int auctionId, int take = 400, CancellationToken ct = default)
        {
            try
            {
                var auction = await _db.AuctionStores
                    .AsNoTracking()
                    .Include(a => a.Seller)
                    .FirstOrDefaultAsync(a => a.AuctionID == auctionId && a.SellerID == ownerId, ct);

                if (auction == null) return new();

                var imgs = new[] { auction.ProductImgURL1, auction.ProductImgURL2, auction.ProductImgURL3 }
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => x!.Trim())
                    .Distinct()
                    .ToList();

                var rows = await _db.AuctionOrders
                    .AsNoTracking()
                    .Where(o => o.AuctionStoreID == auctionId)
                    .OrderByDescending(o => o.BidAmount)
                    .ThenBy(o => o.OrderDate)
                    .Take(take)
                    .Join(_db.Users.AsNoTracking(),
                        o => o.WinnerID,
                        u => u.UserId,
                        (o, u) => new { o, u })
                    .ToListAsync(ct);

                var now = DateTime.UtcNow;
                var ended = auction.EndDate.HasValue && now >= auction.EndDate.Value;

                var result = rows.Select(x =>
                {
                    var o = x.o;
                    var u = x.u;

                    return new AuctionOrderDetailsModel
                    {
                        AuctionId = auction.AuctionID,
                        AuctionType = auction.ProductType,
                        StartPrice = auction.StartPrice,
                        Quantity = auction.Quantity,
                        StartDate = auction.StartDate,
                        EndDate = auction.EndDate,

                        ImageUrl = imgs.FirstOrDefault(),
                        ImageUrls = imgs,

                        AuctionOrderId = o.AuctionOrderID,
                        Status = o.Status.ToString(),
                        OrderDate = o.OrderDate,

                        BidAmount = o.BidAmount,
                        AmountPaid = o.AmountPaid,
                        HeldAmount = o.HeldAmount,

                        bidderId = u.UserId,
                        bidderName = u.FullName ?? "Bidder",
                        bidderProfileImgUrl = u.UserProfileImgURL,
                        bidderVerified = u.Verified,
                        bidderEmail = u.Email,
                        bidderPhone = u.phoneNumber,
                        bidderAddress = u.Address,

                        IsEnded = ended,
                        CanConfirmNow = ended && o.Status == EnumsOrderStatus.Pending
                    };
                }).ToList();

                // fill counts on each row
                var ordersCount = result.Count;
                var pendingCount = result.Count(x => x.Status.Equals("Pending", StringComparison.OrdinalIgnoreCase));
                var confirmedCount = result.Count(x => x.Status.Equals("Confirmed", StringComparison.OrdinalIgnoreCase));

                foreach (var vm in result)
                {
                    vm.OrdersCount = ordersCount;
                    vm.PendingCount = pendingCount;
                    vm.ConfirmedCount = confirmedCount;
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetAuctionOrderDetailsOwnerAsync failed (ownerId={ownerId}, auctionId={auctionId})", ownerId, auctionId);
                return new();
            }
        }

        public async Task<ServiceResult> DeleteAuctionAsync(int id, int factoryId, CancellationToken ct = default)
        {
            await using var tx = await _db.Database.BeginTransactionAsync(ct);

            try
            {
                var auction = await _db.AuctionStores
                    .FirstOrDefaultAsync(a => a.AuctionID == id && a.SellerID == factoryId, ct);

                if (auction == null)
                    return ServiceResult.Fail("Auction not found or you don't have permission.");

                var orders = await _db.AuctionOrders
                    .Where(o => o.AuctionStoreID == id)
                    .ToListAsync(ct);

                // No orders => hard delete
                if (orders.Count == 0)
                {
                    var urls = new[] { auction.ProductImgURL1, auction.ProductImgURL2, auction.ProductImgURL3 }
                        .Where(x => !string.IsNullOrWhiteSpace(x))
                        .Select(x => x!.Trim())
                        .Distinct()
                        .ToList();

                    _db.AuctionStores.Remove(auction);
                    await _db.SaveChangesAsync(ct);
                    await tx.CommitAsync(ct);

                    foreach (var url in urls)
                        await _imageStorage.DeleteAsync(url);

                    return ServiceResult.Ok("Auction deleted successfully!");
                }

                bool isAllowedToHideOnly(EnumsOrderStatus s) =>
                    s == EnumsOrderStatus.Pending ||
                    s == EnumsOrderStatus.Cancelled ||
                    s == EnumsOrderStatus.Refunded;

                var hasBlocking = orders.Any(o => !isAllowedToHideOnly(o.Status));
                if (hasBlocking)
                {
                    var bad = string.Join(", ", orders.Where(o => !isAllowedToHideOnly(o.Status))
                        .Select(o => o.Status).Distinct());

                    return ServiceResult.Fail($"Cannot delete/hide this auction because it has orders in these statuses: {bad}");
                }

                // Cancel pending + refund deposits
                var pending = orders.Where(o => o.Status == EnumsOrderStatus.Pending).ToList();
                if (pending.Count > 0)
                {
                    var sellerWallet = await GetOrCreateWalletAsync(factoryId, ct);

                    foreach (var o in pending)
                    {
                        var refund = Math.Round(o.HeldAmount, 2, MidpointRounding.AwayFromZero);
                        if (refund > 0m)
                        {
                            var bidderWallet = await GetOrCreateWalletAsync(o.WinnerID, ct);

                            await TransferAsync(
                                from: sellerWallet,
                                to: bidderWallet,
                                amount: refund,
                                consumeFromReserved: true,
                                note: $"Refund: auction listing removed by seller (auctionId={id}, orderId={o.AuctionOrderID})",
                                idemKey: $"AUC_DEL_REF:{o.AuctionOrderID}",
                                ct: ct
                            );
                        }

                        o.Status = EnumsOrderStatus.Cancelled;
                        o.CancelledAt = DateTime.UtcNow;
                    }
                }

                // Mark orders deleted-by-seller (اختياري لو عندك enum مناسب)
                foreach (var o in orders)
                    o.Status = EnumsOrderStatus.DeletedBySeller;

                // Soft delete auction
                auction.Status = ProductStatus.RemovedByFactory;

                await _db.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);

                return ServiceResult.Ok("Auction removed successfully. Pending bids were cancelled and refunded.");
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync(ct);
                _logger.LogError(ex, "DeleteAuctionAsync failed");
                return ServiceResult.Fail("Failed to delete/hide auction. Please try again.");
            }
        }

        ////////////////////////////////////////// Auction Methods ////////////////////////////////////
        /////////////////////////////// User Method ///////////////////////////////

        public async Task<List<FactoryStoreModel>> GetPublicAuctionsAsync(SearchFilterModel? filter = null)
        {
            try
            {
                var now = DateTime.UtcNow;

                var q = _db.AuctionStores
                    .AsNoTracking()
                    .Include(x => x.Seller)
                    .Where(x => x.Status == ProductStatus.Available && x.Seller.Verified);

                if (filter != null)
                {
                    if (!string.IsNullOrWhiteSpace(filter.Keyword))
                    {
                        var kw = filter.Keyword.Trim();
                        q = q.Where(x =>
                            (x.ProductType != null && EF.Functions.Like(x.ProductType, $"%{kw}%")) ||
                            (x.Description != null && EF.Functions.Like(x.Description, $"%{kw}%")));
                    }

                    if (filter.MinPrice.HasValue) q = q.Where(x => x.StartPrice >= filter.MinPrice.Value);
                    if (filter.MaxPrice.HasValue) q = q.Where(x => x.StartPrice <= filter.MaxPrice.Value);

                    if (filter.DateFrom.HasValue)
                    {
                        var from = filter.DateFrom.Value.Date;
                        q = q.Where(x => x.StartDate.Date >= from);
                    }
                    if (filter.DateTo.HasValue)
                    {
                        var to = filter.DateTo.Value.Date;
                        q = q.Where(x => x.StartDate.Date <= to);
                    }
                }

                var rows = await q
                    .OrderByDescending(x => x.StartDate)
                    .Select(x => new
                    {
                        x.AuctionID,
                        x.ProductType,
                        x.Description,
                        x.Quantity,
                        x.StartPrice,
                        x.StartDate,
                        x.EndDate,
                        x.Status,
                        x.ProductImgURL1,
                        x.ProductImgURL2,
                        x.ProductImgURL3,

                         x.Address,
                         x.Latitude,
                         x.Longitude,

                        SellerId = x.SellerID,
                        SellerName = x.Seller.FullName,
                        SellerVerified = x.Seller.Verified,
                        SellerProfile = x.Seller.UserProfileImgURL,

                        OrdersCount = _db.AuctionOrders.Count(o => o.AuctionStoreID == x.AuctionID),
                        ActiveOrdersCount = _db.AuctionOrders.Count(o =>
                            o.AuctionStoreID == x.AuctionID &&
                            (o.Status == EnumsOrderStatus.Pending || o.Status == EnumsOrderStatus.Completed)),

                        TopBid = _db.AuctionOrders
                            .Where(o => o.AuctionStoreID == x.AuctionID && o.Status != EnumsOrderStatus.Cancelled)
                            .OrderByDescending(o => o.BidAmount)
                            .ThenByDescending(o => o.OrderDate)
                            .Select(o => new
                            {
                                o.BidAmount,
                                o.OrderDate,
                                BidderName = o.Winner.FullName
                            })
                            .FirstOrDefault()
                    })
                    .ToListAsync();

                return rows.Select(r =>
                {
                    var imgs = new[] { r.ProductImgURL1, r.ProductImgURL2, r.ProductImgURL3 }
                        .Where(x => !string.IsNullOrWhiteSpace(x))
                        .Select(x => x!.Trim())
                        .Distinct()
                        .ToList();

                    bool ended = r.EndDate.HasValue && now > r.EndDate.Value;
                    int? daysRemaining = r.EndDate.HasValue ? (int?)(r.EndDate.Value.Date - now.Date).TotalDays : null;

                    return new FactoryStoreModel
                    {
                        Id = r.AuctionID,
                        Type = "Auction",
                        Name = r.ProductType,
                        ProductType = r.ProductType,
                        Description = r.Description,
                        Price = r.StartPrice,
                        AvailableQuantity = r.Quantity,
                        CreatedAt = r.StartDate,
                        Status = r.Status.ToString(),

                        AuctionStartDate = r.StartDate,
                        AuctionEndDate = r.EndDate,
                        IsAuctionEnded = ended,
                        DaysRemaining = daysRemaining,

                        ImageUrl = imgs.FirstOrDefault(),
                        ImageUrls = imgs,

                        SellerUserId = r.SellerId,
                        SellerName = r.SellerName,
                        IsVerifiedSeller = r.SellerVerified,
                        SellerProfileImgUrl = r.SellerProfile,

                        OrdersCount = r.OrdersCount,
                        ActiveOrdersCount = r.ActiveOrdersCount,

                        TopBidAmount = r.TopBid?.BidAmount,
                        TopBidAt = r.TopBid?.OrderDate,
                        TopBidderName = r.TopBid?.BidderName,

                        // OPTIONAL:
                        // Address = r.Address,
                        // Latitude = r.Latitude,
                        // Longitude = r.Longitude,
                    };
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetPublicAuctionsAsync failed");
                return new();
            }
        }

        public async Task<FactoryStoreModel?> GetPublicAuctionDetailsAsync(int id)
        {
            try
            {
                var today = DateTime.UtcNow.Date;

                var row = await _db.AuctionStores
                    .AsNoTracking()
                    .Include(a => a.Seller).ThenInclude(u => u.FactoryProfile)
                    .Where(a =>
                        a.AuctionID == id &&
                        a.Status == ProductStatus.Available &&
                        a.Seller != null &&
                        a.Seller.Verified
                    )
                    .Select(a => new
                    {
                        a.AuctionID,
                        a.ProductType,
                        a.Description,
                        a.Quantity,
                        a.StartPrice,
                        a.StartDate,
                        a.EndDate,
                        a.Status,
                        a.ProductImgURL1,
                        a.ProductImgURL2,
                        a.ProductImgURL3,

                        // Seller (Company / Factory Owner)
                        SellerUserId = a.Seller.UserId,
                        SellerOwnerName = a.Seller.FullName,
                        SellerVerified = a.Seller.Verified,
                        SellerProfileImg = a.Seller.UserProfileImgURL,

                        // Seller/Factory location (from seller profile)
                        SellerAddress = a.Seller.Address,
                        SellerLat = a.Seller.Latitude,
                        SellerLng = a.Seller.Longitude,
                        SellerPhoneNumber = a.Seller.phoneNumber,
                        SellerEmail = a.Seller.Email,
                        
                        // Factory profile
                        FactoryName = a.Seller.FactoryProfile != null ? a.Seller.FactoryProfile.FactoryName : null,
                        FactoryImg1 = a.Seller.FactoryProfile != null ? a.Seller.FactoryProfile.FactoryImgURL1 : null,
                        FactoryImg2 = a.Seller.FactoryProfile != null ? a.Seller.FactoryProfile.FactoryImgURL2 : null,
                        FactoryImg3 = a.Seller.FactoryProfile != null ? a.Seller.FactoryProfile.FactoryImgURL3 : null,

                        // Orders count
                        OrdersCount = _db.AuctionOrders.Count(o =>
                            o.AuctionStoreID == a.AuctionID &&
                            o.Status != EnumsOrderStatus.Cancelled &&
                            o.Status != EnumsOrderStatus.DeletedByBuyer &&
                            o.Status != EnumsOrderStatus.DeletedBySeller
                        ),

                        ActiveOrdersCount = _db.AuctionOrders.Count(o =>
                            o.AuctionStoreID == a.AuctionID &&
                            o.Status != EnumsOrderStatus.Cancelled &&
                            o.Status != EnumsOrderStatus.DeletedByBuyer &&
                            o.Status != EnumsOrderStatus.DeletedBySeller &&
                            (o.Status == EnumsOrderStatus.Pending || o.Status == EnumsOrderStatus.Processing || o.Status == EnumsOrderStatus.Confirmed)
                        )
                    })
                    .FirstOrDefaultAsync();

                if (row == null) return null;

                var imgs = new[] { row.ProductImgURL1, row.ProductImgURL2, row.ProductImgURL3 }
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => x!.Trim())
                    .Distinct()
                    .ToList();

                var factoryImgs = new[] { row.FactoryImg1, row.FactoryImg2, row.FactoryImg3 }
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => x!.Trim())
                    .Distinct()
                    .ToList();

                bool ended = row.EndDate.HasValue && today > row.EndDate.Value.Date;
                int? daysRemaining = row.EndDate.HasValue ? (int?)(row.EndDate.Value.Date - today).TotalDays : null;

                // ✅ OPTIONAL: best top bid info for UI (safe + fast)
                var top = await _db.AuctionOrders
                    .AsNoTracking()
                    .Where(o => o.AuctionStoreID == id
                                && o.Status != EnumsOrderStatus.Cancelled
                                && o.Status != EnumsOrderStatus.DeletedByBuyer
                                && o.Status != EnumsOrderStatus.DeletedBySeller)
                    .OrderByDescending(o => o.BidAmount)
                    .ThenBy(o => o.OrderDate)
                    .Select(o => new
                    {
                        o.BidAmount,
                        o.OrderDate,
                        BidderName = o.Winner.FullName //"Tommmy ammmhhh"//o. != null ? o.Buyer.FullName : null
                    })
                    .FirstOrDefaultAsync();

                return new FactoryStoreModel
                {
                    Id = row.AuctionID,
                    Type = "Auction",

                    Name = row.ProductType,
                    ProductType = row.ProductType,
                    Description = row.Description,

                    Price = row.StartPrice,
                    AvailableQuantity = row.Quantity,
                    CreatedAt = row.StartDate,
                    Status = row.Status.ToString(),

                    AuctionStartDate = row.StartDate,
                    AuctionEndDate = row.EndDate,
                    IsAuctionEnded = ended,
                    DaysRemaining = daysRemaining,

                    ImageUrl = imgs.FirstOrDefault(),
                    ImageUrls = imgs,

                    // Seller
                    SellerUserId = row.SellerUserId,
                    SellerName = !string.IsNullOrWhiteSpace(row.FactoryName) ? row.FactoryName : row.SellerOwnerName,
                    IsVerifiedSeller = row.SellerVerified,
                    SellerProfileImgUrl = row.SellerProfileImg,
                    SellerEmail = row.SellerEmail,
                    SellerPhone = row.SellerPhoneNumber,

                    // ✅ Factory info (used by your View)
                    FactoryName = row.FactoryName,
                    FactoryImageUrls = factoryImgs,

                    // ✅ Location for "Factory Location" section
                    FactoryAddress = row.SellerAddress,
                    FactoryLatitude = row.SellerLat,
                    FactoryLongitude = row.SellerLng,
                    

                    // Useful counters
                    OrdersCount = row.OrdersCount,
                    ActiveOrdersCount = row.ActiveOrdersCount,

                    // Optional for better Auction UI
                    TopBidAmount = top?.BidAmount,
                    TopBidAt = top?.OrderDate,
                    TopBidderName = top?.BidderName
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetPublicAuctionDetailsAsync failed");
                return null;
            }
        }

        public async Task<ServiceResult> PlaceAuctionOrderAsync(int winnerId, int auctionId, decimal bidAmount, decimal amountPaid, decimal walletUsed, string provider, string providerPaymentId, CancellationToken ct = default)
        {
            bidAmount = Math.Round(bidAmount, 2, MidpointRounding.AwayFromZero);
            amountPaid = Math.Round(amountPaid, 2, MidpointRounding.AwayFromZero);
            walletUsed = Math.Round(walletUsed, 2, MidpointRounding.AwayFromZero);

            if (bidAmount <= 0m) return ServiceResult.Fail("Invalid bid amount.");
            if (amountPaid <= 0m) return ServiceResult.Fail("Invalid amount.");
            if (walletUsed > amountPaid) return ServiceResult.Fail("WalletUsed cannot exceed PaidAmount.");

            var pricing = _pricingOptions.Value;
            var depPct = pricing.AuctionDepositPercent <= 0 ? 0.10m : pricing.AuctionDepositPercent;

            // idempotency check
            if (!string.IsNullOrWhiteSpace(providerPaymentId))
            {
                var txExists = await _db.PaymentTransactions.AsNoTracking()
                    .AnyAsync(p => p.Provider == provider && p.ProviderPaymentId == providerPaymentId, ct);
                if (txExists) return ServiceResult.Fail("Payment already processed.");
            }

            var auction = await _db.AuctionStores
                .Include(a => a.Seller)
                .FirstOrDefaultAsync(a => a.AuctionID == auctionId && a.Seller.Verified, ct);

            if (auction == null) return ServiceResult.Fail("Auction not found.");
            if (auction.SellerID == winnerId) return ServiceResult.Fail("You can't bid on your own auction.");
            if (auction.Status != ProductStatus.Available) return ServiceResult.Fail("Auction is not available.");
            if (!auction.EndDate.HasValue) return ServiceResult.Fail("Auction end date is not set.");

            var nowUtc = DateTime.UtcNow;
            if (nowUtc > auction.EndDate.Value)
                return ServiceResult.Fail("Auction already ended.");

            // ✅ Must be higher than current highest bid
            var highest = await _db.AuctionOrders.AsNoTracking()
                .Where(o => o.AuctionStoreID == auctionId
                            && o.Status != EnumsOrderStatus.Cancelled
                            && o.Status != EnumsOrderStatus.DeletedByBuyer
                            && o.Status != EnumsOrderStatus.DeletedBySeller)
                .MaxAsync(o => (decimal?)o.BidAmount, ct) ?? 0m;

            if (bidAmount <= highest)
                return ServiceResult.Fail($"Bid must be higher than current top bid ({highest:0.00}).");

            // ✅ expected deposit
            var expectedDeposit = Math.Round(bidAmount * depPct, 2, MidpointRounding.AwayFromZero);
            if (Math.Abs(expectedDeposit - amountPaid) > 0.01m)
                return ServiceResult.Fail("Paid amount mismatch (deposit).");

            await using var tx = await _db.Database.BeginTransactionAsync(ct);

            try
            {
                long? bidderWalletTxnId = null;

                // bidder wallet debit if split
                if (walletUsed > 0m)
                {
                    var bidderWallet = await GetOrCreateWalletAsync(winnerId, ct);
                    var available = Math.Round(bidderWallet.Balance - bidderWallet.ReservedBalance, 2, MidpointRounding.AwayFromZero);
                    if (available + 0.0001m < walletUsed)
                        return ServiceResult.Fail("Insufficient available wallet balance.");

                    var wtxn = await AddWalletTxnAsync(
                        wallet: bidderWallet,
                        type: WalletTxnType.PaymentDebit,
                        amount: -walletUsed,
                        currency: "EGP",
                        note: $"Auction bid wallet payment (auctionId={auctionId})",
                        idempotencyKey: $"AUC_BID_WALLET:{provider}:{providerPaymentId}",
                        ct: ct
                    );
                    bidderWalletTxnId = wtxn.Id;
                }

                var order = new AuctionOrder
                {
                    AuctionStoreID = auctionId,
                    WinnerID = winnerId,
                    Status = EnumsOrderStatus.Pending,
                    OrderDate = nowUtc,

                    BidAmount = bidAmount,
                    AmountPaid = amountPaid,
                    HeldAmount = amountPaid,
                    DepositPercentUsed = depPct,

                    PaymentProvider = provider,
                    PaymentProviderId = providerPaymentId
                };

                _db.AuctionOrders.Add(order);
                await _db.SaveChangesAsync(ct);

                // HOLD deposit in seller wallet reserved
                var sellerWallet = await GetOrCreateWalletAsync(auction.SellerID, ct);
                await HoldAsync(
                    wallet: sellerWallet,
                    amount: amountPaid,
                    note: $"Auction BID HOLD (auctionId={auctionId}, orderId={order.AuctionOrderID})",
                    idemKey: $"AUC_HOLD:{provider}:{providerPaymentId}",
                    ct: ct
                );

                // PaymentTransaction
                _db.PaymentTransactions.Add(new PaymentTransaction
                {
                    UserId = winnerId,
                    Provider = provider,
                    ProviderPaymentId = providerPaymentId,
                    Amount = amountPaid,
                    Currency = "EGP",
                    Status = PaymentStatus.Succeeded,
                    WalletTransactionId = bidderWalletTxnId,
                    CreatedAt = nowUtc
                });

                // Update top bid cache (optional)
                auction.CurrentTopBid = bidAmount;
                auction.CurrentTopBidderId = winnerId;

                await _db.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);

                return ServiceResult.Ok("Bid placed successfully!", order.AuctionOrderID);
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync(ct);
                _logger.LogError(ex, "PlaceAuctionBidAsync failed");
                return ServiceResult.Fail("Failed to place bid.");
            }
        }

        public async Task<List<AuctionOrderDetailsModel>> GetAuctionOrdersForWinnerAsync(int winnerId, int take = 300, CancellationToken ct = default)
        {
            var rows = await (
                from o in _db.AuctionOrders.AsNoTracking()
                join a in _db.AuctionStores.AsNoTracking() on o.AuctionStoreID equals a.AuctionID
                join seller in _db.Users.AsNoTracking() on a.SellerID equals seller.UserId
                where o.WinnerID == winnerId && !o.HiddenForBidder
                orderby o.OrderDate descending
                select new
                {
                    o.AuctionOrderID,
                    o.Status,
                    o.OrderDate,
                    o.BidAmount,
                    o.AmountPaid,

                    a.AuctionID,
                    a.ProductType,
                    a.StartPrice,
                    a.StartDate,
                    a.EndDate,
                    a.ProductImgURL1,
                    a.ProductImgURL2,
                    a.ProductImgURL3,

                    SellerName = seller.FullName
                }
            ).Take(take).ToListAsync(ct);

            return rows.Select(x =>
            {
                var imgs = new[] { x.ProductImgURL1, x.ProductImgURL2, x.ProductImgURL3 }
                    .Where(z => !string.IsNullOrWhiteSpace(z))
                    .Select(z => z!.Trim())
                    .Distinct()
                    .ToList();

                return new AuctionOrderDetailsModel
                {
                    AuctionOrderId = x.AuctionOrderID,
                    Status = x.Status.ToString(),
                    OrderDate = x.OrderDate,

                    BidAmount = x.BidAmount,
                    AmountPaid = x.AmountPaid,

                    AuctionId = x.AuctionID,
                    AuctionType = x.ProductType,
                    StartPrice = x.StartPrice,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    SellerName = x.SellerName,
                    ImageUrl = imgs.FirstOrDefault(),
                    ImageUrls = imgs
                };
            }).ToList();
        }

        public async Task<ServiceResult> CancelOrDeleteAuctionOrderByWinnerAsync(int bidderId, int auctionOrderId, CancellationToken ct = default)
        {
            await using var tx = await _db.Database.BeginTransactionAsync(ct);

            try
            {
                var order = await _db.AuctionOrders
                    .Include(o => o.AuctionStore)
                    .FirstOrDefaultAsync(o => o.AuctionOrderID == auctionOrderId && o.WinnerID == bidderId, ct);

                if (order == null) return ServiceResult.Fail("Order not found.");

                if (order.HiddenForBidder)
                    return ServiceResult.Ok("Already removed from your list.");

                // If already cancelled -> hide only
                if (order.Status == EnumsOrderStatus.Cancelled)
                {
                    order.HiddenForBidder = true;
                    await _db.SaveChangesAsync(ct);
                    await tx.CommitAsync(ct);
                    return ServiceResult.Ok("Removed from your list.");
                }

                // ✅ After Confirm => NO REFUND
                if (order.Status == EnumsOrderStatus.Confirmed)
                {
                    order.Status = EnumsOrderStatus.Cancelled;
                    order.CancelledAt = DateTime.UtcNow;
                    order.HiddenForBidder = true;

                    await _db.SaveChangesAsync(ct);
                    await tx.CommitAsync(ct);

                    return ServiceResult.Ok("Cancelled. No refund will be issued because the order was already confirmed.");
                }

                // Only Pending can be cancelled with refund
                if (order.Status != EnumsOrderStatus.Pending)
                    return ServiceResult.Fail("You can cancel/remove this bid only while it is pending.");

                var auction = order.AuctionStore;
                if (auction == null) return ServiceResult.Fail("Auction not found.");

                // Refund deposit from seller reserved to bidder wallet
                var refund = Math.Round(order.HeldAmount, 2, MidpointRounding.AwayFromZero);

                if (refund > 0m)
                {
                    var sellerWallet = await GetOrCreateWalletAsync(auction.SellerID, ct);
                    var bidderWallet = await GetOrCreateWalletAsync(bidderId, ct);

                    await TransferAsync(
                        from: sellerWallet,
                        to: bidderWallet,
                        amount: refund,
                        consumeFromReserved: true,
                        note: $"Refund: bidder cancelled auction bid (orderId={order.AuctionOrderID}, auctionId={auction.AuctionID})",
                        idemKey: $"AUC_BIDDER_CANCEL_REF:{order.AuctionOrderID}",
                        ct: ct
                    );
                }

                order.Status = EnumsOrderStatus.Cancelled;
                order.CancelledAt = DateTime.UtcNow;
                order.HiddenForBidder = true;

                await _db.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);

                return ServiceResult.Ok("Bid cancelled and removed from your list.");
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync(ct);
                _logger.LogError(ex, "CancelOrDeleteAuctionOrderByBidderAsync failed");
                return ServiceResult.Fail("Failed to cancel/remove bid.");
            }
        }

        ////////////////////////////////////////// Job Methods ////////////////////////////////////
        /////////////////////////////// Factory Method ///////////////////////////////

        private async Task<(bool ok, string? error, decimal? lat, decimal? lng, string? normalizedAddress)> ResolveJobLocationAsync(JobProductDetailsModel model, CancellationToken ct = default)
        {
            // نفس منطق Signup/Rental:
            var loc = _locationService.ExtractAndValidateFromForm(
                model.Latitude?.ToString(CultureInfo.InvariantCulture),
                model.Longitude?.ToString(CultureInfo.InvariantCulture),
                model.Location
            );

            if (!loc.IsValid)
                return (false, loc.Error, null, null, null);

            var gpsOrMapProvided = loc.Latitude.HasValue && loc.Longitude.HasValue;

            if (gpsOrMapProvided)
            {
                // user picked from map/GPS
                var lat = loc.Latitude!.Value;
                var lng = loc.Longitude!.Value;

                // لو المستخدم مدخلش عنوان → نعمل ReverseGeocode
                string? address = loc.Address;
                if (string.IsNullOrWhiteSpace(address))
                {
                    var rev = await _locationService.ReverseGeocodeAsync(lat, lng, ct);
                    address = rev;
                }

                if (string.IsNullOrWhiteSpace(address))
                    address = model.Location;

                return (true, null, lat, lng, address);
            }

            // No lat/lng provided -> must have address
            if (!string.IsNullOrWhiteSpace(loc.Address))
            {
                var geo = await _locationService.GetLocationFromAddressAsync(loc.Address!, ct);
                if (geo != null)
                {
                    var normalized = string.IsNullOrWhiteSpace(geo.NormalizedAddress)
                        ? loc.Address
                        : geo.NormalizedAddress;

                    return (true, null, geo.Latitude, geo.Longitude, normalized);
                }

                // لو الجيوكود فشل: نخزن Address فقط (lat/lng null)
                return (true, null, null, null, loc.Address);
            }

            return (false, "Please select location on map or type an address.", null, null, null);
        }

        private static string? GetEnumDisplayName<TEnum>(TEnum? value) where TEnum : struct, Enum
        {
            if (!value.HasValue) return null;

            var member = typeof(TEnum).GetMember(value.Value.ToString()).FirstOrDefault();
            var display = member?.GetCustomAttribute<DisplayAttribute>();
            return display?.Name ?? value.Value.ToString();
        }

        public async Task<bool> JobHasOrdersAsync(int jobId, int factoryId, CancellationToken ct = default)
        {
            return await _db.JobOrders
                .AsNoTracking()
                .AnyAsync(o => o.JobStoreID == jobId && o.JobStore.PostedBy == factoryId, ct);
        }

        public async Task<List<FactoryStoreModel>> GetJobsAsync(int factoryId, SearchFilterModel? filter = null)
        {
            try
            {
                IQueryable<JobStore> query = _db.JobStores
                    .AsNoTracking()
                    .Where(j => j.PostedBy == factoryId)
                    .Include(j => j.User);

                if (filter != null)
                {
                    // Keyword in JobType/Description/Skills
                    if (!string.IsNullOrWhiteSpace(filter.Keyword))
                    {
                        var keyword = filter.Keyword.Trim();
                        query = query.Where(j =>
                            (j.JobType != null && EF.Functions.Like(j.JobType, $"%{keyword}%")) ||
                            (j.Description != null && EF.Functions.Like(j.Description, $"%{keyword}%")) ||
                            (j.RequiredSkills != null && EF.Functions.Like(j.RequiredSkills, $"%{keyword}%")));
                    }

                    // Status
                    if (!string.IsNullOrWhiteSpace(filter.Status))
                    {
                        if (Enum.TryParse<ProductStatus>(filter.Status.Trim(), true, out var statusEnum))
                            query = query.Where(j => j.Status == statusEnum);
                    }

                    // Location contains
                    if (!string.IsNullOrWhiteSpace(filter.Location))
                    {
                        var loc = filter.Location.Trim();
                        query = query.Where(j => j.Location != null && EF.Functions.Like(j.Location, $"%{loc}%"));
                    }

                    // Salary range (MinPrice/MaxPrice reuse)
                    if (filter.MinPrice.HasValue)
                        query = query.Where(j => j.Salary >= filter.MinPrice.Value);

                    if (filter.MaxPrice.HasValue)
                        query = query.Where(j => j.Salary <= filter.MaxPrice.Value);

                    // Date range (CreatedAt)
                    if (filter.DateFrom.HasValue)
                    {
                        var from = filter.DateFrom.Value.Date;
                        query = query.Where(j => j.CreatedAt.Date >= from);
                    }

                    if (filter.DateTo.HasValue)
                    {
                        var to = filter.DateTo.Value.Date;
                        query = query.Where(j => j.CreatedAt.Date <= to);
                    }
                }

                var rows = await query
                    .OrderByDescending(j => j.CreatedAt)
                    .Select(j => new
                    {
                        j.JobID,
                        j.JobType,
                        j.WorkHours,
                        j.Latitude,
                        j.Longitude,
                        j.Location,
                        j.Salary,
                        j.Description,
                        j.Status,
                        j.ExpiryDate,
                        j.RequiredSkills,
                        j.ExperienceLevel,
                        j.EmploymentType,

                        SellerName = j.User.FullName,
                        SellerVerified = j.User.Verified
                    })
                    .ToListAsync();

                var jobs = rows.Select(r => new FactoryStoreModel
                {
                    Id = r.JobID,
                    Type = "Job",

                    // generic fields
                    Name = r.JobType,
                    Price = r.Salary,
                    Status = r.Status.ToString(),
                    SellerName = r.SellerName,
                    CreatedAt = r.ExpiryDate ?? DateTime.Now, // ✅ لو تحب CreatedAt الحقيقي استخدم r.CreatedAt (بس لازم تضيفه في Select)
                    IsVerifiedSeller = r.SellerVerified,

                    // ✅ Job fields (NEW)
                    JobType = r.JobType,
                    WorkHours = r.WorkHours,
                    JobLatitude = r.Latitude,
                    JobLongitude = r.Longitude,
                    JobLocation = r.Location,
                    JobSalary = r.Salary,
                    JobExpiryDate = r.ExpiryDate,
                    RequiredSkills = r.RequiredSkills,
                    ExperienceLevel = r.ExperienceLevel,
                    EmploymentType = GetEnumDisplayName<TypeEmployment>(r.EmploymentType)

                }).ToList();

                // ✅ تصحيح مهم: CreatedAt لازم يكون CreatedAt الحقيقي مش ExpiryDate
                // لو عايزه CreatedAt الصح: عدّل Select فوق وخليه يجيب j.CreatedAt ثم هنا CreatedAt = r.CreatedAt
                return jobs;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting jobs");
                return new List<FactoryStoreModel>();
            }
        }

        public async Task<JobProductDetailsModel?> GetJobByIdAsync(int id, int factoryId)
        {
            try
            {
                var job = await _db.JobStores
                    .AsNoTracking()
                    .FirstOrDefaultAsync(j => j.JobID == id && j.PostedBy == factoryId);

                if (job == null) return null;

                return new JobProductDetailsModel
                {
                    Id = job.JobID,
                    JobType = job.JobType,
                    WorkHours = job.WorkHours,

                    // location
                    Latitude = job.Latitude,
                    Longitude = job.Longitude,
                    Location = job.Location,

                    Salary = job.Salary,
                    Description = job.Description,
                    Status = job.Status,

                    ExpiryDate = job.ExpiryDate,
                    RequiredSkills = job.RequiredSkills,
                    ExperienceLevel = job.ExperienceLevel,

                    // convert enum to display text
                    EmploymentType = GetEnumDisplayName<TypeEmployment>(job.EmploymentType) ?? "Full-time"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting job by ID");
                return null;
            }
        }

        public async Task<ServiceResult> AddJobAsync(JobProductDetailsModel model, int factoryId, CancellationToken ct = default)
        {
            try
            {
                var resolved = await ResolveJobLocationAsync(model, ct);
                if (!resolved.ok)
                    return ServiceResult.Fail(resolved.error ?? "Invalid location.");

                // map EmploymentType string -> enum (لو داخل من UI string)
                TypeEmployment? emp = null;
                if (!string.IsNullOrWhiteSpace(model.EmploymentType))
                {
                    // allow "Full-time" or "FullTime"...
                    var raw = model.EmploymentType.Trim();

                    // normalize
                    raw = raw.Replace("-", "").Replace(" ", "");

                    if (Enum.TryParse<TypeEmployment>(raw, true, out var empEnum))
                        emp = empEnum;
                    else
                        emp = TypeEmployment.FullTime;
                }

                var entity = new JobStore
                {
                    PostedBy = factoryId,

                    JobType = model.JobType?.Trim(),
                    WorkHours = model.WorkHours,

                    // ✅ location resolved
                    Location = resolved.normalizedAddress?.Trim(),
                    Latitude = resolved.lat,
                    Longitude = resolved.lng,

                    Salary = model.Salary,
                    Description = model.Description?.Trim(),

                    Status = ProductStatus.Available,
                    ExpiryDate = model.ExpiryDate,

                    RequiredSkills = model.RequiredSkills?.Trim(),
                    ExperienceLevel = model.ExperienceLevel?.Trim(),

                    EmploymentType = emp ?? TypeEmployment.FullTime,

                    CreatedAt = DateTime.Now,
                    UpdatedAt = null
                };

                _db.JobStores.Add(entity);
                await _db.SaveChangesAsync(ct);

                return ServiceResult.Ok("Job posted successfully!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding job");
                return ServiceResult.Fail("Failed to post job. Please try again.");
            }
        }

        public async Task<ServiceResult> UpdateJobAsync(int id, JobProductDetailsModel model, int factoryId, CancellationToken ct = default)
        {
            try
            {
                var job = await _db.JobStores
                    .FirstOrDefaultAsync(j => j.JobID == id && j.PostedBy == factoryId, ct);

                if (job == null)
                    return ServiceResult.Fail("Job not found or you don't have permission.");

                // ✅ منع أي تعديل بعد وجود Orders
                var hasOrders = await _db.JobOrders.AsNoTracking()
                    .AnyAsync(o => o.JobStoreID == id, ct);

                if (hasOrders)
                    return ServiceResult.Fail("You cannot edit this job because it already has orders.");

                // ========= normal update (زي اللي عندك) =========
                var resolved = await ResolveJobLocationAsync(model, ct);
                if (!resolved.ok)
                    return ServiceResult.Fail(resolved.error ?? "Invalid location.");

                TypeEmployment? emp = job.EmploymentType;
                if (!string.IsNullOrWhiteSpace(model.EmploymentType))
                {
                    var raw = model.EmploymentType.Trim().Replace("-", "").Replace(" ", "");
                    if (Enum.TryParse<TypeEmployment>(raw, true, out var empEnum))
                        emp = empEnum;
                }

                job.Status = model.Status; // حتى دي ممنوعة لو hasOrders (واحنا منعنا فوق)
                job.JobType = model.JobType?.Trim();
                job.WorkHours = model.WorkHours;

                job.Location = resolved.normalizedAddress?.Trim();
                job.Latitude = resolved.lat;
                job.Longitude = resolved.lng;

                job.Salary = model.Salary;
                job.Description = model.Description?.Trim();

                job.ExpiryDate = model.ExpiryDate;
                job.RequiredSkills = model.RequiredSkills?.Trim();
                job.ExperienceLevel = model.ExperienceLevel?.Trim();

                job.EmploymentType = emp ?? TypeEmployment.FullTime;
                job.UpdatedAt = DateTime.Now;

                await _db.SaveChangesAsync(ct);
                return ServiceResult.Ok("Job updated successfully!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating job");
                return ServiceResult.Fail("Failed to update job. Please try again.");
            }
        }

        public async Task<ServiceResult> DeleteJobAsync(int id, int factoryId)
        {
            try
            {
                var job = await _db.JobStores
                    .FirstOrDefaultAsync(j => j.JobID == id && j.PostedBy == factoryId);

                if (job == null)
                    return ServiceResult.Fail("Job not found or you don't have permission.");

                // ✅ Soft delete بدل Remove
                job.Status = ProductStatus.Inactive;
                job.UpdatedAt = DateTime.Now;

                // ✅ OPTIONAL (مفيد للـ Individual): الغي كل الـ Orders لكن لا تحذفها
                var orders = await _db.JobOrders
                    .Where(o => o.JobStoreID == id)
                    .ToListAsync();

                foreach (var o in orders)
                {
                    o.Status = JobOrderStatus.Cancelled; // لو enum عندك فيه Cancelled
                }

                await _db.SaveChangesAsync();

                return ServiceResult.Ok("Job removed from public listing. Existing orders were kept and marked as cancelled.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting job");
                return ServiceResult.Fail("An error occurred while deleting the job.");
            }
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public async Task<List<FactoryStoreModel>> GetPublicJobsAsync(SearchFilterModel? filter = null)
        {
            filter ??= new SearchFilterModel();

            var todayUtc = DateTime.UtcNow.Date;

            IQueryable<JobStore> q = _db.JobStores
                .AsNoTracking()
                .Include(x => x.User)
                .Where(x =>
                    x.Status == ProductStatus.Available &&
                    x.User != null &&
                    x.User.Verified &&
                    (!x.ExpiryDate.HasValue || x.ExpiryDate.Value.Date > todayUtc)
                );

            // 🔎 Keyword
            if (!string.IsNullOrWhiteSpace(filter.Keyword))
            {
                var kw = filter.Keyword.Trim();
                q = q.Where(x =>
                    (x.JobType != null && EF.Functions.Like(x.JobType, $"%{kw}%")) ||
                    (x.Description != null && EF.Functions.Like(x.Description, $"%{kw}%")) ||
                    (x.RequiredSkills != null && EF.Functions.Like(x.RequiredSkills, $"%{kw}%")) ||
                    (x.ExperienceLevel != null && EF.Functions.Like(x.ExperienceLevel, $"%{kw}%"))
                );
            }

            // 📍 Location contains
            if (!string.IsNullOrWhiteSpace(filter.Location))
            {
                var loc = filter.Location.Trim();
                q = q.Where(x => x.Location != null && EF.Functions.Like(x.Location, $"%{loc}%"));
            }

            // 🎓 Experience
            if (!string.IsNullOrWhiteSpace(filter.ExperienceLevel))
            {
                var exp = filter.ExperienceLevel.Trim();
                q = q.Where(x => x.ExperienceLevel != null && EF.Functions.Like(x.ExperienceLevel, $"%{exp}%"));
            }

            // 💰 Salary range
            if (filter.MinPrice.HasValue)
                q = q.Where(x => x.Salary >= filter.MinPrice.Value);

            if (filter.MaxPrice.HasValue)
                q = q.Where(x => x.Salary <= filter.MaxPrice.Value);

            // ================= Projection =================
            var list = await q
                .Select(x => new FactoryStoreModel
                {
                    Id = x.JobID,
                    CreatedAt = x.CreatedAt,

                    // Job fields
                    JobType = x.JobType,
                    JobLocation = x.Location,
                    JobLatitude = x.Latitude,
                    JobLongitude = x.Longitude,
                    JobSalary = x.Salary,
                    WorkHours = x.WorkHours,
                    EmploymentType = x.EmploymentType.ToString(),
                    ExperienceLevel = x.ExperienceLevel,
                    RequiredSkills = x.RequiredSkills,
                    JobExpiryDate = x.ExpiryDate,
                    Description = x.Description,

                    // Factory (Seller)
                    SellerUserId = x.User.UserId,
                    SellerName = x.User.FullName,
                    IsVerifiedSeller = x.User.Verified,
                    SellerProfileImgUrl = x.User.UserProfileImgURL,

                    Status = x.Status.ToString()
                })
                .ToListAsync();

            // 📏 Distance filter (in-memory)
            if (filter.UserLat.HasValue && filter.UserLng.HasValue && filter.MaxDistanceKm.HasValue)
            {
                list = _locationService.FilterWithinKm(
                    list,
                    filter.UserLat.Value,
                    filter.UserLng.Value,
                    filter.MaxDistanceKm.Value,
                    x => x.JobLatitude,
                    x => x.JobLongitude
                ).ToList();
            }

            // 🔃 Sorting
            var sortBy = (filter.SortBy ?? "newest").Trim().ToLower();
            var asc = string.Equals(filter.SortDir, "asc", StringComparison.OrdinalIgnoreCase);

            return sortBy switch
            {
                "salary" => asc
                    ? list.OrderBy(x => x.JobSalary).ToList()
                    : list.OrderByDescending(x => x.JobSalary).ToList(),

                "expiry" => asc
                    ? list.OrderBy(x => x.JobExpiryDate ?? DateTime.MaxValue).ToList()
                    : list.OrderByDescending(x => x.JobExpiryDate ?? DateTime.MinValue).ToList(),

                "distance" when filter.UserLat.HasValue && filter.UserLng.HasValue
                    => _locationService.SortByDistance(
                        list,
                        filter.UserLat.Value,
                        filter.UserLng.Value,
                        x => x.JobLatitude,
                        x => x.JobLongitude,
                        ascending: asc
                    ).ToList(),

                _ => asc
                    ? list.OrderBy(x => x.CreatedAt).ToList()
                    : list.OrderByDescending(x => x.CreatedAt).ToList(),
            };
        }

        public async Task<FactoryStoreModel?> GetPublicJobDetailsAsync(int jobId)
        {
            var todayUtc = DateTime.UtcNow.Date;

            return await _db.JobStores
                .AsNoTracking()
                .Include(x => x.User)
                .Where(x =>
                    x.JobID == jobId &&
                    x.Status == ProductStatus.Available &&
                    x.User != null &&
                    x.User.Verified &&
                    (!x.ExpiryDate.HasValue || x.ExpiryDate.Value.Date > todayUtc)
                )
                .Select(x => new FactoryStoreModel
                {
                    Id = x.JobID,
                    CreatedAt = x.CreatedAt,

                    // Job
                    JobType = x.JobType,
                    JobLocation = x.Location,
                    JobLatitude = x.Latitude,
                    JobLongitude = x.Longitude,
                    JobSalary = x.Salary,
                    WorkHours = x.WorkHours,
                    EmploymentType = x.EmploymentType.ToString(),
                    ExperienceLevel = x.ExperienceLevel,
                    RequiredSkills = x.RequiredSkills,
                    JobExpiryDate = x.ExpiryDate,
                    Description = x.Description,

                    // Factory
                    SellerUserId = x.User.UserId,
                    SellerName = x.User.FullName,
                    IsVerifiedSeller = x.User.Verified,
                    SellerProfileImgUrl = x.User.UserProfileImgURL,

                    Status = x.Status.ToString()
                })
                .FirstOrDefaultAsync();
        }

        public async Task<List<JobOrderDetailsModel>> GetJobOrdersForFactoryAsync(int jobId, int factoryId, CancellationToken ct = default)
        {
            // تأكد إن الجوب ده بتاع نفس المصنع
            var owns = await _db.JobStores.AsNoTracking()
                .AnyAsync(j => j.JobID == jobId && j.PostedBy == factoryId, ct);

            if (!owns) return new List<JobOrderDetailsModel>();

            var rows = await (
                from o in _db.JobOrders.AsNoTracking()
                where o.JobStoreID == jobId
                join u in _db.Users.AsNoTracking() on o.UserID equals u.UserId
                join ut in _db.UserTypes.AsNoTracking() on u.UserTypeID equals ut.TypeID into gut
                from ut in gut.DefaultIfEmpty()
                select new JobOrderDetailsModel
                {
                    JobOrderID = o.JobOrderID,
                    OrderDate = o.OrderDate,
                    OrderStatus = o.Status.ToString(),

                    IndividualUserID = u.UserId,
                    FullName = u.FullName,
                    Email = u.Email,
                    Phone = u.phoneNumber,
                    Address = u.Address,
                    Latitude = u.Latitude,
                    Longitude = u.Longitude,
                    IsVerified = u.Verified,
                    UserProfileImgUrl = u.UserProfileImgURL,

                    UserTypeName = ut != null ? ut.TypeName : null
                }
            )
            // ✅ ترتيب المستخدمين (الأحدث أولاً)
            .OrderByDescending(x => x.OrderDate)
            .ToListAsync(ct);

            return rows;
        }

        public async Task<ServiceResult> PlaceJobOrderAsync(int userId, int jobId, CancellationToken ct = default)
        {
            try
            {
                var today = DateTime.UtcNow.Date;

                var job = await _db.JobStores
                    .Include(x => x.User)
                    .FirstOrDefaultAsync(x =>
                        x.JobID == jobId &&
                        x.Status == ProductStatus.Available &&
                        x.User.Verified &&
                        (!x.ExpiryDate.HasValue || x.ExpiryDate.Value.Date > today)
                    , ct);

                if (job == null) return ServiceResult.Fail("Job not available.");
                if (job.PostedBy == userId) return ServiceResult.Fail("You can't apply to your own job.");

                var already = await _db.JobOrders.AsNoTracking()
                    .AnyAsync(o => o.JobStoreID == jobId && o.UserID == userId, ct);

                if (already) return ServiceResult.Fail("You already applied to this job.");

                var order = new JobOrder
                {
                    JobStoreID = jobId,
                    UserID = userId,
                    Status = JobOrderStatus.Pending,
                    OrderDate = DateTime.UtcNow
                };

                _db.JobOrders.Add(order);
                await _db.SaveChangesAsync(ct);

                return ServiceResult.Ok("Job application sent successfully!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PlaceJobOrderAsync failed");
                return ServiceResult.Fail("Failed to apply for the job.");
            }
        }

        public Task<List<JobOrder>> GetJobOrdersForUserAsync(int userId)
    => _db.JobOrders.AsNoTracking()
        .Include(o => o.JobStore).ThenInclude(s => s.User)
        .Where(o => o.UserID == userId)
        .OrderByDescending(o => o.OrderDate)
        .ToListAsync();
    }
}