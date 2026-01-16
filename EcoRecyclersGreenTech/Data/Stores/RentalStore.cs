using EcoRecyclersGreenTech.Data.Users;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static EcoRecyclersGreenTech.Data.Stores.EnumsProductStatus;

namespace EcoRecyclersGreenTech.Data.Stores
{
    public enum RentalCondition
    {
        EmptyLand = 1,      // أرض فضاء (سواء صحراوية أو زراعية أو سكنية)
        Property = 2,       // عقار (مبنى أو محل أو مخزن أو مصنع)
        CommercialSpace = 3, // مساحة تجارية (مكتب أو صالة عرض)
        Other = 4
    }

    public class RentalStore
    {
        [Key]
        public int RentalID { get; set; }
        public int OwnerID { get; set; }
        public User Owner { get; set; } = null!;

        // GPS Coordinates
        [Column(TypeName = "decimal(9,6)")]
        public decimal? Latitude { get; set; }

        [Column(TypeName = "decimal(9,6)")]
        public decimal? Longitude { get; set; }

        [MaxLength(255)]
        public string? Address { get; set; }
        public double Area { get; set; }

        [Precision(18, 2)]
        public decimal PricePerMonth { get; set; }

        public string? RentalImgURL1 { get; set; }
        public string? RentalImgURL2 { get; set; }
        public string? RentalImgURL3 { get; set; }

        public string? Description { get; set; }
        public DateTime AvailableFrom { get; set; }
        public DateTime? AvailableUntil { get; set; }
        public RentalCondition? Condition { get; set; }
        public ProductStatus Status { get; set; } = ProductStatus.Available;
        public bool IsFurnished { get; set; }
        public bool HasElectricity { get; set; }
        public bool HasWater { get; set; }
    }
}
