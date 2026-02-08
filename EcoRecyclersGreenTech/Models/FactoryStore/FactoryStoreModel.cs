using EcoRecyclersGreenTech.Models.FactoryStore.Products;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcoRecyclersGreenTech.Models.FactoryStore
{
    public class FactoryStoreModel
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Type { get; set; }

        public string? ImageUrl { get; set; }
        public List<string>? ImageUrls { get; set; }

        public decimal Price { get; set; }

        public string? SellerProfileImgUrl { get; set; }
        public int SellerUserId { get; set; }

        // materials/machines
        public int AvailableQuantity { get; set; }
        public string? Unit { get; set; }

        public string? Status { get; set; }
        public string? SellerName { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsVerifiedSeller { get; set; }

        public string? Description { get; set; }

        // ===== shared fields =====
        public string? ProductType { get; set; }
        public string? MachineType { get; set; }

        [Column(TypeName = "decimal(9,6)")]
        public decimal? SellerLatitude { get; set; }

        [Column(TypeName = "decimal(9,6)")]
        public decimal? SellerLongitude { get; set; }

        [MaxLength(255)]
        public string? SellerAddress { get; set; }

        // ===================== Material fields =====================

        // Material Pickup (from MaterialStore table)
        [Column(TypeName = "decimal(9,6)")]
        public decimal? MaterialLatitude { get; set; }

        [Column(TypeName = "decimal(9,6)")]
        public decimal? MaterialLongitude { get; set; }

        [MaxLength(255)]
        public string? MaterialPickupAddress { get; set; }

        // ===================== Machine fields =====================
        public string? MachineCondition { get; set; }
        public DateTime? ManufactureDate { get; set; }
        public decimal? MinOrderQuantity { get; set; }
        public string? Brand { get; set; }
        public string? Model { get; set; }
        public int? WarrantyMonths { get; set; }

        // ===================== Rental fields =====================
        [Column(TypeName = "decimal(9,6)")]
        public decimal? RentalLatitude { get; set; }

        [Column(TypeName = "decimal(9,6)")]
        public decimal? RentalLongitude { get; set; }

        [MaxLength(255)]
        public string? RentalAddress { get; set; }

        public double? RentalArea { get; set; }
        public DateTime? AvailableFrom { get; set; }
        public DateTime? AvailableUntil { get; set; }
        public string? RentalCondition { get; set; }

        public bool? IsFurnished { get; set; }
        public bool? HasElectricity { get; set; }
        public bool? HasWater { get; set; }

        public bool IsPrivate { get; set; } = false;

        // ===================== Auction fields =====================
        public DateTime? AuctionStartDate { get; set; }
        public DateTime? AuctionEndDate { get; set; }

        public bool? IsAuctionEnded { get; set; }
        public int? DaysRemaining { get; set; }
        public string? TopBidderName { get; set; }
        public decimal? TopBidAmount { get; set; }
        public DateTime? TopBidAt { get; set; }
        public int? ConfirmedOrderId { get; set; }
        public DateTime? ConfirmedAt { get; set; }

        [Column(TypeName = "decimal(9,6)")]
        public decimal? AuctionLatitude { get; set; }

        [Column(TypeName = "decimal(9,6)")]
        public decimal? AuctionLongitude { get; set; }

        [MaxLength(255)]
        public string? AuctionAddress { get; set; }

        // ===================== Job fields =====================
        public string? JobType { get; set; }
        public int? WorkHours { get; set; }

        [Column(TypeName = "decimal(9,6)")]
        public decimal? JobLatitude { get; set; }

        [Column(TypeName = "decimal(9,6)")]
        public decimal? JobLongitude { get; set; }

        [MaxLength(255)]
        public string? JobLocation { get; set; }

        public decimal? JobSalary { get; set; }
        public DateTime? JobExpiryDate { get; set; }
        public string? RequiredSkills { get; set; }
        public string? ExperienceLevel { get; set; }
        public string? EmploymentType { get; set; }

        // Factory Info
        public string? FactoryName { get; set; }
        public List<string> FactoryImageUrls { get; set; } = new();

        // Contact
        public string? SellerEmail { get; set; }
        public string? SellerPhone { get; set; }

        [Column(TypeName = "decimal(9,6)")]
        public decimal? FactoryLatitude { get; set; }

        [Column(TypeName = "decimal(9,6)")]
        public decimal? FactoryLongitude { get; set; }

        [MaxLength(255)]
        // Store list extras
        public string? FactoryAddress { get; set; }

        // Buyer-only order extras (Material/Machine/Auction/Job)
        public int? MyOrderId { get; set; }
        public string? MyOrderStatus { get; set; }
        public DateTime? MyOrderDate { get; set; }
        public int? MyOrderQuantity { get; set; }
        public DateTime? CancelUntil { get; set; }
        public DateTime? ExpectedArrivalDate { get; set; }
        public bool CanCancel { get; set; }

        // Order identifiers
        public int RentalOrderId { get; set; }              // REN order id
        public int RentalId { get; set; }                   // listing rental id

        // Order meta
        public DateTime OrderDate { get; set; }             // order created date/time
        public int Months { get; set; } = 3;                // upfront months

        // Pricing
        public decimal PricePerMonth { get; set; }
        public decimal? TotalPaid { get; set; }
        public decimal? WalletUsed { get; set; }
        public decimal? StripePaid { get; set; }
        public decimal? PlatformFee { get; set; }

        // Buyer info
        public int? BuyerId { get; set; }
        public string? BuyerName { get; set; }
        public string? BuyerProfileImgUrl { get; set; }

        public FactoryStoreModel? Listing { get; set; }

        // Order 
        public int OrdersCount { get; set; }
        public int ActiveOrdersCount { get; set; }
    }
}
