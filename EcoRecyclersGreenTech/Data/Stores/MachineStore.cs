using EcoRecyclersGreenTech.Data.Users;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static EcoRecyclersGreenTech.Data.Stores.EnumsProductStatus;

namespace EcoRecyclersGreenTech.Data.Stores
{
    public enum MachineCondition
    {
        New = 1,
        LikeNew = 2,
        Used = 3,
        UsedFrequently = 4,
        AlmostBroken = 5,
        Disabled =6
    }

    public class MachineStore
    {
        [Key]
        public int MachineID { get; set; }
        public int SellerID { get; set; }
        public User Seller { get; set; } = null!;
        public string? MachineType { get; set; }

        [Precision(18, 2)]
        public int Quantity { get; set; }

        public string? MachineImgURL1 { get; set; }
        public string? MachineImgURL2 { get; set; }
        public string? MachineImgURL3 { get; set; }
        public MachineCondition? Condition { get; set; } = MachineCondition.Used;

        [Precision(18, 2)]
        public decimal Price { get; set; }
        public string? Brand { get; set; }
        public string? Model { get; set; }
        public DateTime ManufactureDate { get; set; }
        public int? WarrantyMonths { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ProductStatus Status { get; set; } = ProductStatus.Available;

        // GPS Coordinates
        [Column(TypeName = "decimal(9,6)")]
        public decimal? Latitude { get; set; }

        [Column(TypeName = "decimal(9,6)")]
        public decimal? Longitude { get; set; }

        [MaxLength(255)]
        public string? Address { get; set; }

        [Precision(18, 2)]
        public decimal? MinOrderQuantity { get; set; }
        public int CancelWindowDays { get; set; } = 3;
        public int DeliveryDays { get; set; } = 5;
    }
}
