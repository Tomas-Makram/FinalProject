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

        // ✅ للـ materials/machines
        public int AvailableQuantity { get; set; }
        public string? Unit { get; set; }

        public string? Status { get; set; }
        public string? SellerName { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsVerifiedSeller { get; set; }

        public string? Description { get; set; }


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

        // ===================== Auction fields (NEW) =====================
        public DateTime? AuctionStartDate { get; set; }
        public DateTime? AuctionEndDate { get; set; }

        // Optional: computed/derived in service
        public bool? IsAuctionEnded { get; set; }
        public int? DaysRemaining { get; set; }

        // ✅ Leader / Top bidder info (English)
        public string? TopBidderName { get; set; }          // "John Smith"
        public decimal? TopBidAmount { get; set; }          // highest bid
        public DateTime? TopBidAt { get; set; }             // when top bid placed

        // ===================== Job fields (NEW) =====================
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

        // لو هتستخدم EmploymentType من JobVM (Full-time / Part-time)
        public string? EmploymentType { get; set; }


    }
}