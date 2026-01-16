using EcoRecyclersGreenTech.Data.Orders;
using EcoRecyclersGreenTech.Data.Stores;
using EcoRecyclersGreenTech.Models;
using EcoRecyclersGreenTech.Models.FactoryStore;
using EcoRecyclersGreenTech.Models.FactoryStore.Dashboard;
using EcoRecyclersGreenTech.Models.FactoryStore.Orders;
using EcoRecyclersGreenTech.Models.FactoryStore.Products;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Reflection;
using static EcoRecyclersGreenTech.Data.Stores.EnumsProductStatus;
using static EcoRecyclersGreenTech.Services.IFactoryStoreService;

namespace EcoRecyclersGreenTech.Services
{
    public interface IFactoryStoreService
    {

        public class ServiceResult
        {
            public bool Success { get; set; }
            public string Message { get; set; } = "";
            public static ServiceResult Ok(string msg) => new() { Success = true, Message = msg };
            public static ServiceResult Fail(string msg) => new() { Success = false, Message = msg };
        }

        // Dashboard
        Task<DashboardModel> GetDashboardStatsAsync(int factoryId);

        // Materials
        Task<List<FactoryStoreModel>> GetMaterialsAsync(int factoryId, SearchFilterModel? filter = null);
        Task<MaterialModel?> GetMaterialByIdAsync(int id, int factoryId);
        Task<bool> AddMaterialAsync(MaterialModel model, int factoryId);
        Task<bool> UpdateMaterialAsync(int id, MaterialModel model, int factoryId);
        Task<ServiceResult> DeleteMaterialAsync(int id, int factoryId);
        Task<bool> ToggleProductStatusAsync(int id, string status, int factoryId, string productType);

        // Machines
        Task<List<FactoryStoreModel>> GetMachinesAsync(int factoryId, SearchFilterModel? filter = null);
        Task<MachineModel?> GetMachineByIdAsync(int id, int factoryId);
        Task<bool> AddMachineAsync(MachineModel model, int factoryId);
        Task<bool> UpdateMachineAsync(int id, MachineModel model, int factoryId);
        Task<ServiceResult> DeleteMachineAsync(int id, int factoryId);

        // Rentals
        Task<List<FactoryStoreModel>> GetRentalsAsync(int factoryId, SearchFilterModel? filter = null);
        Task<RentalModel?> GetRentalByIdAsync(int id, int factoryId);
        Task<ServiceResult> AddRentalAsync(RentalModel model, int factoryId, CancellationToken ct = default);
        Task<ServiceResult> UpdateRentalAsync(int id, RentalModel model, int factoryId, CancellationToken ct = default);
        Task<ServiceResult> DeleteRentalAsync(int id, int factoryId);

        // Auctions
        Task<List<FactoryStoreModel>> GetAuctionsAsync(int factoryId, SearchFilterModel? filter = null);
        Task<AuctionModel?> GetAuctionByIdAsync(int id, int factoryId);
        Task<bool> AddAuctionAsync(AuctionModel model, int factoryId);
        Task<bool> UpdateAuctionAsync(int id, AuctionModel model, int factoryId);
        Task<ServiceResult> DeleteAuctionAsync(int id, int factoryId);

        // Jobs
        Task<List<FactoryStoreModel>> GetJobsAsync(int factoryId, SearchFilterModel? filter = null);
        Task<bool> JobHasOrdersAsync(int jobId, int factoryId, CancellationToken ct = default);
        Task<JobModel?> GetJobByIdAsync(int id, int factoryId);
        Task<ServiceResult> AddJobAsync(JobModel model, int factoryId, CancellationToken ct = default);
        Task<ServiceResult> UpdateJobAsync(int id, JobModel model, int factoryId, CancellationToken ct = default);
        Task<ServiceResult> DeleteJobAsync(int id, int factoryId);

        // Public Marketplace
        Task<List<FactoryStoreModel>> GetPublicMaterialsAsync(SearchFilterModel? filter = null);
        Task<List<FactoryStoreModel>> GetPublicMachinesAsync(SearchFilterModel? filter = null);
        Task<List<FactoryStoreModel>> GetPublicRentalsAsync(SearchFilterModel? filter = null);
        Task<List<FactoryStoreModel>> GetPublicAuctionsAsync(SearchFilterModel? filter = null);
        Task<List<FactoryStoreModel>> GetPublicJobsAsync(SearchFilterModel? filter = null);
        Task<List<JobOrderRowModel>> GetJobOrdersForFactoryAsync(int jobId, int factoryId, CancellationToken ct = default);
        Task<FactoryStoreModel?> GetPublicJobDetailsAsync(int jobId);

        // Validation
        Task<bool> IsFactoryVerifiedAsync(int factoryId);
        Task<bool> CanFactoryAddProductAsync(int factoryId);
        Task<bool> CanFactoryModifyProductAsync(int productId, int factoryId, string productType);

        // File Upload
        Task<string?> UploadImageAsync(IFormFile? image, string folder);
    }

    public class FactoryStoreService : IFactoryStoreService
    {
        private readonly DBContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly IImageStorageService _imageStorage;
        private readonly ILocationService _locationService;
        private readonly ILogger<FactoryStoreService> _logger;

        public FactoryStoreService(DBContext context, IWebHostEnvironment environment, ILogger<FactoryStoreService> logger, IImageStorageService imageStorage, ILocationService locationService)
        {
            _context = context;
            _environment = environment;
            _logger = logger;
            _imageStorage = imageStorage;
            _locationService = locationService;
        }

        public async Task<string?> UploadImageAsync(IFormFile? image, string folder)
        {
            if (image == null || image.Length == 0)
                return null;

            try
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", folder);
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }

                return $"/uploads/{folder}/{fileName}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading image");
                return null;
            }
        }

        ////////////////////////////////////////// Dashboard Methods ////////////////////////////////////

        public async Task<DashboardModel> GetDashboardStatsAsync(int factoryId)
        {
            try
            {
                var stats = new DashboardModel
                {
                    ActiveMaterials = await _context.MaterialStores
                        .CountAsync(m => m.SellerID == factoryId && m.Status == ProductStatus.Available),
                    ActiveMachines = await _context.MachineStores
                        .CountAsync(m => m.SellerID == factoryId && m.Status == ProductStatus.Available),
                    ActiveRentals = await _context.RentalStores
                        .CountAsync(r => r.OwnerID == factoryId && r.Status == ProductStatus.Available),
                    ActiveAuctions = await _context.AuctionStores
                        .CountAsync(a => a.SellerID == factoryId && a.Status == ProductStatus.Available),
                    ActiveJobs = await _context.JobStores
                        .CountAsync(j => j.PostedBy == factoryId && j.Status == ProductStatus.Available)
                };

                stats.TotalProducts = stats.ActiveMaterials + stats.ActiveMachines +
                                     stats.ActiveRentals + stats.ActiveAuctions + stats.ActiveJobs;

                // Get recent orders
                var recentOrders = await _context.MaterialOrders
                    .Where(o => _context.MaterialStores.Any(m => m.MaterialID == o.MaterialStoreID && m.SellerID == factoryId))
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
        public async Task<List<FactoryStoreModel>> GetMaterialsAsync(int factoryId, SearchFilterModel? filter = null)
        {
            try
            {
                IQueryable<MaterialStore> query = _context.MaterialStores
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
                        m.MaterialID,
                        m.ProductType,
                        m.Price,
                        m.Quantity,
                        m.Unit,
                        m.Status,
                        m.CreatedAt,
                        m.ProductImgURL1,
                        m.ProductImgURL2,
                        m.ProductImgURL3,
                        SellerName = m.Seller.FullName,
                        SellerVerified = m.Seller.Verified
                    })
                    .ToListAsync();

                var materials = rows.Select(r =>
                {
                    var images = new[] { r.ProductImgURL1, r.ProductImgURL2, r.ProductImgURL3 }
                        .Where(x => !string.IsNullOrWhiteSpace(x))
                        .Select(x => x!.Trim())
                        .Distinct()
                        .ToList();

                    return new FactoryStoreModel
                    {
                        Id = r.MaterialID,
                        Name = r.ProductType,
                        Type = "Material",

                        ImageUrl = images.FirstOrDefault(),
                        ImageUrls = images,

                        Price = r.Price,
                        AvailableQuantity = r.Quantity,
                        Unit = r.Unit,
                        Status = r.Status.ToString(),

                        SellerName = r.SellerName,
                        CreatedAt = r.CreatedAt,
                        IsVerifiedSeller = r.SellerVerified
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

        public async Task<MaterialModel?> GetMaterialByIdAsync(int id, int factoryId)
        {
            try
            {
                var material = await _context.MaterialStores
                    .AsNoTracking()
                    .FirstOrDefaultAsync(m => m.MaterialID == id && m.SellerID == factoryId);

                if (material == null) return null;

                return new MaterialModel
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

                    Status = material.Status
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting material by ID");
                return null;
            }
        }

        public async Task<bool> AddMaterialAsync(MaterialModel model, int factoryId)
        {
            try
            {
                var entity = new MaterialStore
                {
                    SellerID = factoryId,
                    ProductType = model.ProductType?.Trim(),
                    Quantity = model.Quantity,
                    Description = model.Description?.Trim(),
                    Price = model.Price,
                    Unit = string.IsNullOrWhiteSpace(model.Unit) ? "kg" : model.Unit.Trim(),
                    MinOrderQuantity = model.MinOrderQuantity,
                    Status = ProductStatus.Available,
                    CreatedAt = DateTime.Now
                };

                // ✅ Upload images via ImageStorageService
                if (model.ProductImage1 != null && model.ProductImage1.Length > 0)
                    entity.ProductImgURL1 = await _imageStorage.UploadAsync(model.ProductImage1, "materials");

                if (model.ProductImage2 != null && model.ProductImage2.Length > 0)
                    entity.ProductImgURL2 = await _imageStorage.UploadAsync(model.ProductImage2, "materials");

                if (model.ProductImage3 != null && model.ProductImage3.Length > 0)
                    entity.ProductImgURL3 = await _imageStorage.UploadAsync(model.ProductImage3, "materials");

                _context.MaterialStores.Add(entity);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding material");
                return false;
            }
        }

        public async Task<bool> UpdateMaterialAsync(int id, MaterialModel model, int factoryId)
        {
            try
            {
                var material = await _context.MaterialStores
                    .FirstOrDefaultAsync(m => m.MaterialID == id && m.SellerID == factoryId);

                if (material == null) return false;

                // ✅ Status: مسموح تغييره دائمًا
                material.Status = model.Status;

                var hasPendingOrders = await _context.MaterialOrders
                    .AnyAsync(o => o.MaterialStoreID == id && o.Status == "Pending");

                if (hasPendingOrders)
                {
                    material.Description = model.Description?.Trim();
                    material.Unit = string.IsNullOrWhiteSpace(model.Unit) ? material.Unit : model.Unit!.Trim();
                    material.MinOrderQuantity = model.MinOrderQuantity;
                }
                else
                {
                    material.ProductType = model.ProductType?.Trim();
                    material.Quantity = model.Quantity;
                    material.Description = model.Description?.Trim();
                    material.Price = model.Price;
                    material.Unit = string.IsNullOrWhiteSpace(model.Unit) ? "kg" : model.Unit.Trim();
                    material.MinOrderQuantity = model.MinOrderQuantity;
                }

                // ✅ Replace images via ImageStorageService (only if new file uploaded)
                if (model.ProductImage1 != null && model.ProductImage1.Length > 0)
                    material.ProductImgURL1 = await _imageStorage.ReplaceAsync(model.ProductImage1, "materials", material.ProductImgURL1);

                if (model.ProductImage2 != null && model.ProductImage2.Length > 0)
                    material.ProductImgURL2 = await _imageStorage.ReplaceAsync(model.ProductImage2, "materials", material.ProductImgURL2);

                if (model.ProductImage3 != null && model.ProductImage3.Length > 0)
                    material.ProductImgURL3 = await _imageStorage.ReplaceAsync(model.ProductImage3, "materials", material.ProductImgURL3);

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating material");
                return false;
            }
        }

        public async Task<ServiceResult> DeleteMaterialAsync(int id, int factoryId)
        {
            try
            {
                var material = await _context.MaterialStores
                    .FirstOrDefaultAsync(m => m.MaterialID == id && m.SellerID == factoryId);

                if (material == null)
                    return IFactoryStoreService.ServiceResult.Fail("Material not found or you don't have permission.");

                var hasOrders = await _context.MaterialOrders.AnyAsync(o => o.MaterialStoreID == id);
                if (hasOrders)
                {
                    material.Status = ProductStatus.Inactive;
                    await _context.SaveChangesAsync();
                    return IFactoryStoreService.ServiceResult.Ok("Material has orders, so it was marked as inactive.");
                }

                // ✅ Capture URLs before delete
                var urls = new[] { material.ProductImgURL1, material.ProductImgURL2, material.ProductImgURL3 }
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => x!.Trim())
                    .Distinct()
                    .ToList();

                _context.MaterialStores.Remove(material);
                await _context.SaveChangesAsync();

                // ✅ Delete local files via ImageStorageService
                foreach (var url in urls)
                {
                    await _imageStorage.DeleteAsync(url);
                }

                return IFactoryStoreService.ServiceResult.Ok("Material deleted successfully!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting material");
                return IFactoryStoreService.ServiceResult.Fail("An error occurred while deleting the material.");
            }
        }

        ////////////////////////////////////////// Machine Methods ////////////////////////////////////
        public async Task<List<FactoryStoreModel>> GetMachinesAsync(int factoryId, SearchFilterModel? filter = null)
        {
            try
            {
                IQueryable<MachineStore> query = _context.MachineStores
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

                // نجيب rows أولاً عشان نكوّن ImageUrls في الذاكرة
                var rows = await query
                    .OrderByDescending(m => m.ManufactureDate) // أو لو عندك CreatedAt استخدمه
                    .Select(m => new
                    {
                        m.MachineID,
                        m.MachineType,
                        m.Price,
                        m.Quantity,
                        m.Status,
                        m.ManufactureDate,

                        m.MachineImgURL1,
                        m.MachineImgURL2,
                        m.MachineImgURL3,

                        m.Condition,
                        m.Brand,
                        m.Model,
                        m.WarrantyMonths,

                        SellerName = m.Seller.FullName,
                        SellerVerified = m.Seller.Verified
                    })
                    .ToListAsync();

                var machines = rows.Select(r =>
                {
                    var images = new[] { r.MachineImgURL1, r.MachineImgURL2, r.MachineImgURL3 }
                        .Where(x => !string.IsNullOrWhiteSpace(x))
                        .Select(x => x!.Trim())
                        .Distinct()
                        .ToList();

                    return new FactoryStoreModel
                    {
                        Id = r.MachineID,
                        Name = r.MachineType,
                        Type = "Machine",

                        ImageUrl = images.FirstOrDefault(),
                        ImageUrls = images,

                        Price = r.Price,
                        AvailableQuantity = r.Quantity,
                        Unit = null, // machines عادة مفيهاش unit
                        Status = r.Status.ToString(),

                        SellerName = r.SellerName,
                        CreatedAt = r.ManufactureDate,
                        IsVerifiedSeller = r.SellerVerified,

                        // ProductListingVM.Condition = string للعرض
                        MachineCondition = r.Condition.HasValue ? r.Condition.Value.ToString() : null,

                        Brand = r.Brand,
                        Model = r.Model,
                        WarrantyMonths = r.WarrantyMonths
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

        public async Task<MachineModel?> GetMachineByIdAsync(int id, int factoryId)
        {
            try
            {
                var machine = await _context.MachineStores
                    .AsNoTracking()
                    .FirstOrDefaultAsync(m => m.MachineID == id && m.SellerID == factoryId);

                if (machine == null) return null;

                return new MachineModel
                {
                    Id = machine.MachineID,
                    MachineType = machine.MachineType,
                    Quantity = machine.Quantity,
                    Description = machine.Description,
                    Price = machine.Price,
                    Status = machine.Status,
                    MinOrderQuantity = machine.MinOrderQuantity,
                    ManufactureDate = machine.ManufactureDate,

                    Condition = machine.Condition, // ✅ Enum

                    Brand = machine.Brand,
                    Model = machine.Model,
                    WarrantyMonths = machine.WarrantyMonths,

                    CurrentImageUrl1 = machine.MachineImgURL1,
                    CurrentImageUrl2 = machine.MachineImgURL2,
                    CurrentImageUrl3 = machine.MachineImgURL3
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting machine by ID");
                return null;
            }
        }

        public async Task<bool> AddMachineAsync(MachineModel model, int factoryId)
        {
            try
            {
                var entity = new MachineStore
                {
                    SellerID = factoryId,
                    MachineType = model.MachineType?.Trim(),
                    Quantity = model.Quantity,
                    Description = model.Description?.Trim(),
                    Price = model.Price,

                    Status = ProductStatus.Available,
                    MinOrderQuantity = model.MinOrderQuantity,
                    ManufactureDate = model.ManufactureDate,

                    Condition = model.Condition, // ✅ Enum

                    Brand = model.Brand?.Trim(),
                    Model = model.Model?.Trim(),
                    WarrantyMonths = model.WarrantyMonths
                };

                // ✅ Upload images
                if (model.MachineImage1 != null && model.MachineImage1.Length > 0)
                    entity.MachineImgURL1 = await _imageStorage.UploadAsync(model.MachineImage1, "machines");

                if (model.MachineImage2 != null && model.MachineImage2.Length > 0)
                    entity.MachineImgURL2 = await _imageStorage.UploadAsync(model.MachineImage2, "machines");

                if (model.MachineImage3 != null && model.MachineImage3.Length > 0)
                    entity.MachineImgURL3 = await _imageStorage.UploadAsync(model.MachineImage3, "machines");

                _context.MachineStores.Add(entity);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding machine");
                return false;
            }
        }

        public async Task<bool> UpdateMachineAsync(int id, MachineModel model, int factoryId)
        {
            try
            {
                var machine = await _context.MachineStores
                    .FirstOrDefaultAsync(m => m.MachineID == id && m.SellerID == factoryId);

                if (machine == null) return false;

                // مسموح تغييره دائمًا
                machine.Status = model.Status;

                // لو عندك Orders logic زي materials تقدر تضيفه هنا (اختياري)
                // var hasPendingOrders = await _context.MachineOrders.AnyAsync(o => o.MachineStoreID == id && o.Status == "Pending");

                machine.MachineType = model.MachineType?.Trim();
                machine.Quantity = model.Quantity;
                machine.ManufactureDate = model.ManufactureDate;
                machine.Description = model.Description?.Trim();
                machine.Condition = model.Condition;   // ✅ Enum
                machine.Price = model.Price;
                machine.MinOrderQuantity = model.MinOrderQuantity;
                machine.Brand = model.Brand?.Trim();
                machine.Model = model.Model?.Trim();
                machine.WarrantyMonths = model.WarrantyMonths;

                // ✅ Replace images only if new file uploaded
                if (model.MachineImage1 != null && model.MachineImage1.Length > 0)
                    machine.MachineImgURL1 = await _imageStorage.ReplaceAsync(model.MachineImage1, "machines", machine.MachineImgURL1);

                if (model.MachineImage2 != null && model.MachineImage2.Length > 0)
                    machine.MachineImgURL2 = await _imageStorage.ReplaceAsync(model.MachineImage2, "machines", machine.MachineImgURL2);

                if (model.MachineImage3 != null && model.MachineImage3.Length > 0)
                    machine.MachineImgURL3 = await _imageStorage.ReplaceAsync(model.MachineImage3, "machines", machine.MachineImgURL3);

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating machine");
                return false;
            }
        }

        public async Task<ServiceResult> DeleteMachineAsync(int id, int factoryId)
        {
            try
            {
                var machine = await _context.MachineStores
                    .FirstOrDefaultAsync(m => m.MachineID == id && m.SellerID == factoryId);

                if (machine == null)
                    return IFactoryStoreService.ServiceResult.Fail("Machine not found or you don't have permission.");

                var hasOrders = await _context.MachineOrders.AnyAsync(o => o.MachineStoreID == id);
                if (hasOrders)
                {
                    machine.Status = ProductStatus.Inactive;
                    await _context.SaveChangesAsync();
                    return IFactoryStoreService.ServiceResult.Ok("Machine has orders, so it was marked as inactive.");
                }

                // ✅ Capture URLs before delete
                var urls = new[] { machine.MachineImgURL1, machine.MachineImgURL2, machine.MachineImgURL3 }
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => x!.Trim())
                    .Distinct()
                    .ToList();

                _context.MachineStores.Remove(machine);
                await _context.SaveChangesAsync();

                // ✅ Delete local files
                foreach (var url in urls)
                    await _imageStorage.DeleteAsync(url);

                return IFactoryStoreService.ServiceResult.Ok("Machine deleted successfully!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting machine");
                return IFactoryStoreService.ServiceResult.Fail("An error occurred while deleting the machine.");
            }
        }

        ////////////////////////////////////////// Rental Methods ////////////////////////////////////
       
        private async Task<(bool ok, string? error, decimal? lat, decimal? lng, string? normalizedAddress)> ResolveRentalLocationAsync(RentalModel model, CancellationToken ct = default)
        {
            // نفس منطق Signup:
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

                // Location عندك Required في RentalVM، بس هنحافظ على fallback
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

                // لو الجيوكود فشل: نخزن Address فقط (lat/lng null)
                return (true, null, null, null, loc.Address);
            }

            // لا إحداثيات ولا عنوان
            return (false, "Please select location on map or type an address.", null, null, null);
        }

        public async Task<List<FactoryStoreModel>> GetRentalsAsync(int factoryId, SearchFilterModel? filter = null)
        {
            try
            {
                IQueryable<RentalStore> query = _context.RentalStores
                    .AsNoTracking()
                    .Where(r => r.OwnerID == factoryId)
                    .Include(r => r.Owner);

                if (filter != null)
                {
                    // Keyword in Address/Description
                    if (!string.IsNullOrWhiteSpace(filter.Keyword))
                    {
                        var keyword = filter.Keyword.Trim();
                        query = query.Where(r =>
                            (r.Address != null && EF.Functions.Like(r.Address, $"%{keyword}%")) ||
                            (r.Description != null && EF.Functions.Like(r.Description, $"%{keyword}%")));
                    }

                    // Status
                    if (!string.IsNullOrWhiteSpace(filter.Status))
                    {
                        if (Enum.TryParse<ProductStatus>(filter.Status.Trim(), true, out var statusEnum))
                            query = query.Where(r => r.Status == statusEnum);
                    }

                    // Location filter
                    if (!string.IsNullOrWhiteSpace(filter.Location))
                    {
                        var loc = filter.Location.Trim();
                        query = query.Where(r => r.Address != null && EF.Functions.Like(r.Address, $"%{loc}%"));
                    }

                    // Date range AvailableFrom
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

                    // Price range
                    if (filter.MinPrice.HasValue)
                        query = query.Where(r => r.PricePerMonth >= filter.MinPrice.Value);

                    if (filter.MaxPrice.HasValue)
                        query = query.Where(r => r.PricePerMonth <= filter.MaxPrice.Value);
                }

                var rows = await query
                    .OrderByDescending(r => r.AvailableFrom)
                    .Select(r => new
                    {
                        r.RentalID,
                        r.Address,
                        r.Area,
                        r.PricePerMonth,
                        r.Description,
                        r.Status,
                        r.AvailableFrom,
                        r.AvailableUntil,
                        r.Condition,
                        r.IsFurnished,
                        r.HasElectricity,
                        r.HasWater,

                        // ✅ location
                        r.Latitude,
                        r.Longitude,

                        // ✅ images
                        r.RentalImgURL1,
                        r.RentalImgURL2,
                        r.RentalImgURL3,

                        OwnerName = r.Owner.FullName,
                        OwnerVerified = r.Owner.Verified
                    })
                    .ToListAsync();

                var rentals = rows.Select(r =>
                {
                    var images = new[] { r.RentalImgURL1, r.RentalImgURL2, r.RentalImgURL3 }
                        .Where(x => !string.IsNullOrWhiteSpace(x))
                        .Select(x => x!.Trim())
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
                        SellerName = r.OwnerName,
                        CreatedAt = r.AvailableFrom,
                        IsVerifiedSeller = r.OwnerVerified,

                        // Rental details
                        RentalAddress = r.Address,
                        RentalArea = r.Area,
                        AvailableFrom = r.AvailableFrom,
                        AvailableUntil = r.AvailableUntil,
                        RentalCondition = r.Condition.HasValue ? r.Condition.Value.ToString() : null,
                        IsFurnished = r.IsFurnished,
                        HasElectricity = r.HasElectricity,
                        HasWater = r.HasWater,

                        // ✅ location
                        RentalLatitude = r.Latitude,
                        RentalLongitude = r.Longitude,

                        // ✅ images
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

        public async Task<RentalModel?> GetRentalByIdAsync(int id, int factoryId)
        {
            try
            {
                var rental = await _context.RentalStores
                    .AsNoTracking()
                    .FirstOrDefaultAsync(r => r.RentalID == id && r.OwnerID == factoryId);

                if (rental == null) return null;

                return new RentalModel
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

                    // ✅ location
                    Latitude = rental.Latitude,
                    Longitude = rental.Longitude,

                    // ✅ current images
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

        public async Task<ServiceResult> AddRentalAsync(RentalModel model, int factoryId, CancellationToken ct = default)
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

                _context.RentalStores.Add(entity);
                await _context.SaveChangesAsync(ct);

                return ServiceResult.Ok("Rental property added successfully!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding rental");
                return ServiceResult.Fail("Failed to add rental property. Please try again.");
            }
        }

        public async Task<ServiceResult> UpdateRentalAsync(int id, RentalModel model, int factoryId, CancellationToken ct = default)
        {
            try
            {
                var rental = await _context.RentalStores
                    .FirstOrDefaultAsync(r => r.RentalID == id && r.OwnerID == factoryId, ct);

                if (rental == null)
                    return ServiceResult.Fail("Rental not found or you don't have permission.");

                // ✅ Status always editable
                rental.Status = model.Status;

                // ✅ Resolve location again (لو المستخدم غيّرها)
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

                // ✅ Replace images only if new uploaded
                if (model.RentalImage1 != null && model.RentalImage1.Length > 0)
                    rental.RentalImgURL1 = await _imageStorage.ReplaceAsync(model.RentalImage1, "rentals", rental.RentalImgURL1);

                if (model.RentalImage2 != null && model.RentalImage2.Length > 0)
                    rental.RentalImgURL2 = await _imageStorage.ReplaceAsync(model.RentalImage2, "rentals", rental.RentalImgURL2);

                if (model.RentalImage3 != null && model.RentalImage3.Length > 0)
                    rental.RentalImgURL3 = await _imageStorage.ReplaceAsync(model.RentalImage3, "rentals", rental.RentalImgURL3);

                await _context.SaveChangesAsync(ct);
                return ServiceResult.Ok("Rental property updated successfully!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating rental");
                return ServiceResult.Fail("Failed to update rental property. Please try again.");
            }
        }

        public async Task<ServiceResult> DeleteRentalAsync(int id, int factoryId)
        {
            try
            {
                var rental = await _context.RentalStores
                    .FirstOrDefaultAsync(r => r.RentalID == id && r.OwnerID == factoryId);

                if (rental == null)
                    return ServiceResult.Fail("Rental not found or you don't have permission.");

                // ✅ Capture urls before delete
                var urls = new[] { rental.RentalImgURL1, rental.RentalImgURL2, rental.RentalImgURL3 }
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => x!.Trim())
                    .Distinct()
                    .ToList();

                _context.RentalStores.Remove(rental);
                await _context.SaveChangesAsync();

                // ✅ delete from storage
                foreach (var url in urls)
                {
                    await _imageStorage.DeleteAsync(url);
                }

                return ServiceResult.Ok("Rental deleted successfully!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting rental");
                return ServiceResult.Fail("An error occurred while deleting the rental.");
            }
        }

        ////////////////////////////////////////// Auction Methods ////////////////////////////////////

        public async Task<List<FactoryStoreModel>> GetAuctionsAsync(int factoryId, SearchFilterModel? filter = null)
        {
            try
            {
                IQueryable<AuctionStore> query = _context.AuctionStores
                    .AsNoTracking()
                    .Where(a => a.SellerID == factoryId)
                    .Include(a => a.Seller);

                if (filter != null)
                {
                    // Keyword in ProductType/Description
                    if (!string.IsNullOrWhiteSpace(filter.Keyword))
                    {
                        var keyword = filter.Keyword.Trim();
                        query = query.Where(a =>
                            (a.ProductType != null && EF.Functions.Like(a.ProductType, $"%{keyword}%")) ||
                            (a.Description != null && EF.Functions.Like(a.Description, $"%{keyword}%")));
                    }

                    // Status
                    if (!string.IsNullOrWhiteSpace(filter.Status))
                    {
                        if (Enum.TryParse<ProductStatus>(filter.Status.Trim(), true, out var statusEnum))
                            query = query.Where(a => a.Status == statusEnum);
                    }

                    // Price range
                    if (filter.MinPrice.HasValue)
                        query = query.Where(a => a.StartPrice >= filter.MinPrice.Value);

                    if (filter.MaxPrice.HasValue)
                        query = query.Where(a => a.StartPrice <= filter.MaxPrice.Value);

                    // Date range (StartDate)
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
                        a.AuctionID,
                        a.ProductType,
                        a.Quantity,
                        a.StartPrice,
                        a.Description,
                        a.StartDate,
                        a.EndDate,
                        a.Status,

                        // ✅ 3 images
                        a.ProductImgURL1,
                        a.ProductImgURL2,
                        a.ProductImgURL3,

                        SellerName = a.Seller != null ? a.Seller.FullName : null,
                        SellerVerified = a.Seller != null && a.Seller.Verified

                        // ✅ لو عندك Bids بعدين ضيف هنا TopBidder / TopBidAmount / TopBidAt
                    })
                    .ToListAsync();

                var auctions = rows.Select(r =>
                {
                    var images = new[] { r.ProductImgURL1, r.ProductImgURL2, r.ProductImgURL3 }
                        .Where(x => !string.IsNullOrWhiteSpace(x))
                        .Select(x => x!.Trim())
                        .Distinct()
                        .ToList();

                    // computed fields (optional)
                    bool? ended = r.EndDate.HasValue ? (DateTime.UtcNow.Date > r.EndDate.Value.Date) : (bool?)null;
                    int? daysRemaining = r.EndDate.HasValue ? (int?)(r.EndDate.Value.Date - DateTime.UtcNow.Date).TotalDays : null;

                    return new FactoryStoreModel
                    {
                        Id = r.AuctionID,
                        Name = r.ProductType,
                        Type = "Auction",

                        ImageUrl = images.FirstOrDefault(),
                        ImageUrls = images,

                        Price = r.StartPrice,
                        AvailableQuantity = r.Quantity,

                        Status = r.Status.ToString(),
                        SellerName = r.SellerName,
                        CreatedAt = r.StartDate,
                        IsVerifiedSeller = r.SellerVerified,

                        // ✅ NEW (matches your updated ProductListingVM)
                        AuctionStartDate = r.StartDate,
                        AuctionEndDate = r.EndDate,
                        IsAuctionEnded = ended,
                        DaysRemaining = daysRemaining,

                        // ✅ Top bidder fields (leave null until you implement bids)
                        TopBidderName = null,
                        TopBidAmount = null,
                        TopBidAt = null
                    };
                }).ToList();

                return auctions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting auctions");
                return new List<FactoryStoreModel>();
            }
        }

        public async Task<AuctionModel?> GetAuctionByIdAsync(int id, int factoryId)
        {
            try
            {
                var auction = await _context.AuctionStores
                    .AsNoTracking()
                    .FirstOrDefaultAsync(a => a.AuctionID == id && a.SellerID == factoryId);

                if (auction == null) return null;

                return new AuctionModel
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

        public async Task<bool> AddAuctionAsync(AuctionModel model, int factoryId)
        {
            try
            {
                var entity = new AuctionStore
                {
                    SellerID = factoryId,
                    ProductType = model.AuctionType?.Trim(),
                    Quantity = model.Quantity,
                    StartPrice = model.StartPrice,
                    Description = model.Description?.Trim(),
                    StartDate = model.StartDate,
                    EndDate = model.EndDate,
                    Status = ProductStatus.Available
                };

                // ✅ Upload images (same style as machines/materials)
                if (model.AuctionImage1 != null && model.AuctionImage1.Length > 0)
                    entity.ProductImgURL1 = await _imageStorage.UploadAsync(model.AuctionImage1, "auctions");

                if (model.AuctionImage2 != null && model.AuctionImage2.Length > 0)
                    entity.ProductImgURL2 = await _imageStorage.UploadAsync(model.AuctionImage2, "auctions");

                if (model.AuctionImage3 != null && model.AuctionImage3.Length > 0)
                    entity.ProductImgURL3 = await _imageStorage.UploadAsync(model.AuctionImage3, "auctions");

                _context.AuctionStores.Add(entity);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding auction");
                return false;
            }
        }

        public async Task<bool> UpdateAuctionAsync(int id, AuctionModel model, int factoryId)
        {
            try
            {
                var auction = await _context.AuctionStores
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

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating auction");
                return false;
            }
        }

        public async Task<ServiceResult> DeleteAuctionAsync(int id, int factoryId)
        {
            try
            {
                var auction = await _context.AuctionStores
                    .FirstOrDefaultAsync(a => a.AuctionID == id && a.SellerID == factoryId);

                if (auction == null)
                    return ServiceResult.Fail("Auction not found or you don't have permission.");

                // ✅ Capture image urls before delete
                var urls = new[] { auction.ProductImgURL1, auction.ProductImgURL2, auction.ProductImgURL3 }
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => x!.Trim())
                    .Distinct()
                    .ToList();

                _context.AuctionStores.Remove(auction);
                await _context.SaveChangesAsync();

                // ✅ Delete stored images
                foreach (var url in urls)
                    await _imageStorage.DeleteAsync(url);

                return ServiceResult.Ok("Auction deleted successfully!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting auction");
                return ServiceResult.Fail("An error occurred while deleting the auction.");
            }
        }

        ////////////////////////////////////////// Job Methods ////////////////////////////////////

        private async Task<(bool ok, string? error, decimal? lat, decimal? lng, string? normalizedAddress)> ResolveJobLocationAsync(JobModel model, CancellationToken ct = default)
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
            return await _context.JobOrders
                .AsNoTracking()
                .AnyAsync(o => o.JobStoreID == jobId && o.JobStore.PostedBy == factoryId, ct);
        }

        public async Task<List<FactoryStoreModel>> GetJobsAsync(int factoryId, SearchFilterModel? filter = null)
        {
            try
            {
                IQueryable<JobStore> query = _context.JobStores
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

        public async Task<JobModel?> GetJobByIdAsync(int id, int factoryId)
        {
            try
            {
                var job = await _context.JobStores
                    .AsNoTracking()
                    .FirstOrDefaultAsync(j => j.JobID == id && j.PostedBy == factoryId);

                if (job == null) return null;

                return new JobModel
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

        public async Task<ServiceResult> AddJobAsync(JobModel model, int factoryId, CancellationToken ct = default)
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

                _context.JobStores.Add(entity);
                await _context.SaveChangesAsync(ct);

                return ServiceResult.Ok("Job posted successfully!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding job");
                return ServiceResult.Fail("Failed to post job. Please try again.");
            }
        }

        public async Task<ServiceResult> UpdateJobAsync(int id, JobModel model, int factoryId, CancellationToken ct = default)
        {
            try
            {
                var job = await _context.JobStores
                    .FirstOrDefaultAsync(j => j.JobID == id && j.PostedBy == factoryId, ct);

                if (job == null)
                    return ServiceResult.Fail("Job not found or you don't have permission.");

                // ✅ منع أي تعديل بعد وجود Orders
                var hasOrders = await _context.JobOrders.AsNoTracking()
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

                await _context.SaveChangesAsync(ct);
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
                var job = await _context.JobStores
                    .FirstOrDefaultAsync(j => j.JobID == id && j.PostedBy == factoryId);

                if (job == null)
                    return ServiceResult.Fail("Job not found or you don't have permission.");

                // ✅ Soft delete بدل Remove
                job.Status = ProductStatus.Inactive;
                job.UpdatedAt = DateTime.Now;

                // ✅ OPTIONAL (مفيد للـ Individual): الغي كل الـ Orders لكن لا تحذفها
                var orders = await _context.JobOrders
                    .Where(o => o.JobStoreID == id)
                    .ToListAsync();

                foreach (var o in orders)
                {
                    o.Status = JobOrderStatus.Cancelled; // لو enum عندك فيه Cancelled
                }

                await _context.SaveChangesAsync();

                return ServiceResult.Ok("Job removed from public listing. Existing orders were kept and marked as cancelled.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting job");
                return ServiceResult.Fail("An error occurred while deleting the job.");
            }
        }

        ////////////////////////////////////////// Status Change Method ////////////////////////////////////

        public async Task<bool> ToggleProductStatusAsync(int id, string status, int factoryId, string productType)
        {
            try
            {
                var parsedStatus = Enum.Parse<ProductStatus>(status);

                switch (productType.ToLower())
                {
                    case "material":
                        var material = await _context.MaterialStores
                            .FirstOrDefaultAsync(m => m.MaterialID == id && m.SellerID == factoryId);
                        if (material == null) return false;
                        material.Status = parsedStatus;
                        break;

                    case "machine":
                        var machine = await _context.MachineStores
                            .FirstOrDefaultAsync(m => m.MachineID == id && m.SellerID == factoryId);
                        if (machine == null) return false;
                        machine.Status = parsedStatus;
                        break;

                    case "rental":
                        var rental = await _context.RentalStores
                            .FirstOrDefaultAsync(r => r.RentalID == id && r.OwnerID == factoryId);
                        if (rental == null) return false;
                        rental.Status = parsedStatus;
                        break;

                    case "auction":
                        var auction = await _context.AuctionStores
                            .FirstOrDefaultAsync(a => a.AuctionID == id && a.SellerID == factoryId);
                        if (auction == null) return false;
                        auction.Status = parsedStatus;
                        break;

                    case "job":
                        var job = await _context.JobStores
                            .FirstOrDefaultAsync(j => j.JobID == id && j.PostedBy == factoryId);
                        if (job == null) return false;
                        job.Status = parsedStatus;
                        break;

                    default:
                        return false;
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling product status");
                return false;
            }
        }


        ////////////////////////////////////////// Public Marketplace Methods ////////////////////////////////////
        
        public async Task<List<FactoryStoreModel>> GetPublicMaterialsAsync(SearchFilterModel? filter = null)
        {
            try
            {
                IQueryable<MaterialStore> query = _context.MaterialStores
                    .AsNoTracking()
                    .Include(m => m.Seller)
                    .Where(m => m.Status == ProductStatus.Available && m.Seller != null && m.Seller.Verified);

                if (filter != null)
                {
                    if (!string.IsNullOrWhiteSpace(filter.Keyword))
                    {
                        var keyword = filter.Keyword.Trim();
                        query = query.Where(m =>
                            (m.ProductType != null && EF.Functions.Like(m.ProductType, $"%{keyword}%")) ||
                            (m.Description != null && EF.Functions.Like(m.Description, $"%{keyword}%")));
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
                        m.MaterialID,
                        m.ProductType,
                        m.Price,
                        m.Quantity,
                        m.Unit,
                        m.Status,
                        m.CreatedAt,
                        m.ProductImgURL1,
                        m.ProductImgURL2,
                        m.ProductImgURL3,
                        SellerName = m.Seller!.FullName,
                        SellerVerified = m.Seller!.Verified
                    })
                    .ToListAsync();

                var materials = rows.Select(r =>
                {
                    var images = new[] { r.ProductImgURL1, r.ProductImgURL2, r.ProductImgURL3 }
                        .Where(x => !string.IsNullOrWhiteSpace(x))
                        .Select(x => x!.Trim())
                        .Distinct()
                        .ToList();

                    return new FactoryStoreModel
                    {
                        Id = r.MaterialID,
                        Name = r.ProductType,
                        Type = "Material",

                        ImageUrl = images.FirstOrDefault(),
                        ImageUrls = images,

                        Price = r.Price,
                        AvailableQuantity = r.Quantity,
                        Unit = r.Unit,
                        Status = r.Status.ToString(),

                        SellerName = r.SellerName,
                        CreatedAt = r.CreatedAt,
                        IsVerifiedSeller = r.SellerVerified
                    };
                }).ToList();

                return materials;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting public materials");
                return new List<FactoryStoreModel>();
            }
        }

        public async Task<List<FactoryStoreModel>> GetPublicMachinesAsync(SearchFilterModel? filter = null)
        {
            try
            {
                IQueryable<MachineStore> query = _context.MachineStores
                    .AsNoTracking()
                    .Include(m => m.Seller)
                    .Where(m => m.Status == ProductStatus.Available && m.Seller != null && m.Seller.Verified);

                if (filter != null)
                {
                    if (!string.IsNullOrWhiteSpace(filter.Keyword))
                    {
                        var keyword = filter.Keyword.Trim();
                        query = query.Where(m =>
                            (m.MachineType != null && EF.Functions.Like(m.MachineType, $"%{keyword}%")) ||
                            (m.Description != null && EF.Functions.Like(m.Description, $"%{keyword}%")));
                    }

                    if (filter.MinPrice.HasValue)
                        query = query.Where(m => m.Price >= filter.MinPrice.Value);

                    if (filter.MaxPrice.HasValue)
                        query = query.Where(m => m.Price <= filter.MaxPrice.Value);
                }

                // نجيب بيانات خام الأول (عشان ImageUrls تتعمل في الذاكرة)
                var rows = await query
                    .OrderByDescending(m => m.ManufactureDate)
                    .Select(m => new
                    {
                        m.MachineID,
                        m.MachineType,
                        m.Price,
                        m.Quantity,
                        m.Status,
                        m.ManufactureDate,

                        m.MachineImgURL1,
                        m.MachineImgURL2,
                        m.MachineImgURL3,

                        m.Condition,
                        m.Brand,
                        m.Model,
                        m.WarrantyMonths,

                        SellerName = m.Seller!.FullName,
                        SellerVerified = m.Seller!.Verified
                    })
                    .ToListAsync();

                var machines = rows.Select(r =>
                {
                    var images = new[] { r.MachineImgURL1, r.MachineImgURL2, r.MachineImgURL3 }
                        .Where(x => !string.IsNullOrWhiteSpace(x))
                        .Select(x => x!.Trim())
                        .Distinct()
                        .ToList();

                    return new FactoryStoreModel
                    {
                        Id = r.MachineID,
                        Name = r.MachineType,
                        Type = "Machine",

                        ImageUrl = images.FirstOrDefault(),
                        ImageUrls = images,

                        Price = r.Price,
                        AvailableQuantity = r.Quantity,
                        Status = r.Status.ToString(),

                        SellerName = r.SellerName,
                        CreatedAt = r.ManufactureDate,
                        IsVerifiedSeller = r.SellerVerified,

                        // ✅ enum? -> string
                        MachineCondition = r.Condition.HasValue ? r.Condition.Value.ToString() : null,

                        Brand = r.Brand,
                        Model = r.Model,
                        WarrantyMonths = r.WarrantyMonths
                    };
                }).ToList();

                return machines;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting public machines");
                return new List<FactoryStoreModel>();
            }
        }

        public async Task<List<FactoryStoreModel>> GetPublicRentalsAsync(SearchFilterModel? filter = null)
        {
            try
            {
                IQueryable<RentalStore> query = _context.RentalStores
                    .AsNoTracking()
                    .Where(r => r.Status == ProductStatus.Available)
                    .Include(r => r.Owner)
                    .Where(r => r.Owner != null && r.Owner.Verified);

                if (filter != null)
                {
                    // Keyword
                    if (!string.IsNullOrWhiteSpace(filter.Keyword))
                    {
                        var keyword = filter.Keyword.Trim();
                        query = query.Where(r =>
                            (r.Address != null && EF.Functions.Like(r.Address, $"%{keyword}%")) ||
                            (r.Description != null && EF.Functions.Like(r.Description, $"%{keyword}%")));
                    }

                    // Location (separate)
                    if (!string.IsNullOrWhiteSpace(filter.Location))
                    {
                        var loc = filter.Location.Trim();
                        query = query.Where(r => r.Address != null && EF.Functions.Like(r.Address, $"%{loc}%"));
                    }

                    // Optional: Min/Max Price (لو بتحب تدعمهم في public كمان)
                    if (filter.MinPrice.HasValue)
                        query = query.Where(r => r.PricePerMonth >= filter.MinPrice.Value);

                    if (filter.MaxPrice.HasValue)
                        query = query.Where(r => r.PricePerMonth <= filter.MaxPrice.Value);

                    // Optional: Date range (لو بتحب)
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
                }

                // rows first
                var rows = await query
                    .OrderByDescending(r => r.AvailableFrom)
                    .Select(r => new
                    {
                        r.RentalID,
                        r.Address,
                        r.Area,
                        r.PricePerMonth,
                        r.Status,
                        r.AvailableFrom,
                        r.AvailableUntil,
                        r.Condition,
                        r.IsFurnished,
                        r.HasElectricity,
                        r.HasWater,
                        OwnerName = r.Owner!.FullName,
                        OwnerVerified = r.Owner!.Verified
                    })
                    .ToListAsync();

                var rentals = rows.Select(r => new FactoryStoreModel
                {
                    Id = r.RentalID,
                    Type = "Rental",

                    Name = !string.IsNullOrWhiteSpace(r.Address) ? r.Address!.Trim() : "Rental",
                    Price = r.PricePerMonth,
                    Status = r.Status.ToString(),

                    SellerName = r.OwnerName,
                    CreatedAt = r.AvailableFrom,
                    IsVerifiedSeller = r.OwnerVerified,

                    // ✅ Rental fields (حسب الـ ProductListingVM الجديد)
                    RentalAddress = r.Address,
                    RentalArea = r.Area,
                    AvailableFrom = r.AvailableFrom,
                    AvailableUntil = r.AvailableUntil,
                    RentalCondition = r.Condition.HasValue ? r.Condition.Value.ToString() : null,
                    IsFurnished = r.IsFurnished,
                    HasElectricity = r.HasElectricity,
                    HasWater = r.HasWater,

                    // rentals مفيهاش quantity فعلي
                    AvailableQuantity = 1,
                    Unit = "month",

                    // no images currently in RentalStore
                    ImageUrl = null,
                    ImageUrls = new List<string>()
                }).ToList();

                return rentals;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting public rentals");
                return new List<FactoryStoreModel>();
            }
        }

        public async Task<List<FactoryStoreModel>> GetPublicAuctionsAsync(SearchFilterModel? filter = null)
        {
            try
            {
                IQueryable<AuctionStore> query = _context.AuctionStores
                    .AsNoTracking()
                    .Include(a => a.Seller)
                    .Where(a => a.Status == ProductStatus.Available)
                    .Where(a => a.Seller != null && a.Seller.Verified);

                if (filter != null)
                {
                    // Keyword in ProductType/Description
                    if (!string.IsNullOrWhiteSpace(filter.Keyword))
                    {
                        var keyword = filter.Keyword.Trim();
                        query = query.Where(a =>
                            (a.ProductType != null && EF.Functions.Like(a.ProductType, $"%{keyword}%")) ||
                            (a.Description != null && EF.Functions.Like(a.Description, $"%{keyword}%")));
                    }

                    // Price range (optional)
                    if (filter.MinPrice.HasValue)
                        query = query.Where(a => a.StartPrice >= filter.MinPrice.Value);

                    if (filter.MaxPrice.HasValue)
                        query = query.Where(a => a.StartPrice <= filter.MaxPrice.Value);

                    // Date range (optional)
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

                    // Status filter ignored intentionally (Public = Available only)
                    // لو عايز تسمح بيه شيل السطر ده وبدّل شرط Available فوق
                }

                var rows = await query
                    .OrderByDescending(a => a.StartDate)
                    .Select(a => new
                    {
                        a.AuctionID,
                        a.ProductType,
                        a.Quantity,
                        a.StartPrice,
                        a.Status,
                        a.StartDate,

                        a.ProductImgURL1,
                        a.ProductImgURL2,
                        a.ProductImgURL3,

                        SellerName = a.Seller!.FullName,
                        SellerVerified = a.Seller!.Verified
                    })
                    .ToListAsync();

                var auctions = rows.Select(r =>
                {
                    var images = new[] { r.ProductImgURL1, r.ProductImgURL2, r.ProductImgURL3 }
                        .Where(x => !string.IsNullOrWhiteSpace(x))
                        .Select(x => x!.Trim())
                        .Distinct()
                        .ToList();

                    return new FactoryStoreModel
                    {
                        Id = r.AuctionID,
                        Name = r.ProductType,
                        Type = "Auction",

                        ImageUrl = images.FirstOrDefault(),
                        ImageUrls = images,

                        Price = r.StartPrice,
                        AvailableQuantity = r.Quantity,
                        Unit = "unit",

                        Status = r.Status.ToString(),
                        SellerName = r.SellerName,
                        CreatedAt = r.StartDate,
                        IsVerifiedSeller = r.SellerVerified
                    };
                }).ToList();

                return auctions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting public auctions");
                return new List<FactoryStoreModel>();
            }
        }

        public async Task<List<FactoryStoreModel>> GetPublicJobsAsync(SearchFilterModel? filter = null)
        {
            filter ??= new SearchFilterModel();

            var todayUtc = DateTime.UtcNow.Date;

            IQueryable<JobStore> q = _context.JobStores
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
                    SellerUserId = x.User.UserID,
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

            return await _context.JobStores
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
                    SellerUserId = x.User.UserID,
                    SellerName = x.User.FullName,
                    IsVerifiedSeller = x.User.Verified,
                    SellerProfileImgUrl = x.User.UserProfileImgURL,

                    Status = x.Status.ToString()
                })
                .FirstOrDefaultAsync();
        }

        public async Task<List<JobOrderRowModel>> GetJobOrdersForFactoryAsync(int jobId, int factoryId, CancellationToken ct = default)
        {
            // تأكد إن الجوب ده بتاع نفس المصنع
            var owns = await _context.JobStores.AsNoTracking()
                .AnyAsync(j => j.JobID == jobId && j.PostedBy == factoryId, ct);

            if (!owns) return new List<JobOrderRowModel>();

            var rows = await (
                from o in _context.JobOrders.AsNoTracking()
                where o.JobStoreID == jobId
                join u in _context.Users.AsNoTracking() on o.UserID equals u.UserID
                join ut in _context.UserTypes.AsNoTracking() on u.UserTypeID equals ut.TypeID into gut
                from ut in gut.DefaultIfEmpty()
                select new JobOrderRowModel
                {
                    JobOrderID = o.JobOrderID,
                    OrderDate = o.OrderDate,
                    OrderStatus = o.Status.ToString(),

                    IndividualUserID = u.UserID,
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

        ////////////////////////////////////////// Validation Methods ////////////////////////////////////

        public async Task<bool> IsFactoryVerifiedAsync(int factoryId)
        {
            try
            {
                var factory = await _context.Users.FindAsync(factoryId);
                return factory?.Verified == true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking factory verification");
                return false;
            }
        }

        public async Task<bool> CanFactoryAddProductAsync(int factoryId)
        {
            try
            {
                var factory = await _context.Users.FindAsync(factoryId);
                return factory?.Verified == true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if factory can add product");
                return false;
            }
        }

        public async Task<bool> CanFactoryModifyProductAsync(int productId, int factoryId, string productType)
        {
            try
            {
                return productType.ToLower() switch
                {
                    "material" => !await _context.MaterialOrders
                        .AnyAsync(o => o.MaterialStoreID == productId && o.Status == "Pending"),
                    "machine" => !await _context.MachineOrders
                        .AnyAsync(o => o.MachineStoreID == productId && o.Status == "Pending"),
                    "rental" => !await _context.RentalOrders
                        .AnyAsync(o => o.RentalStoreID == productId && o.Status == "Pending"),
                    _ => true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if factory can modify product");
                return false;
            }
        }
    }
}