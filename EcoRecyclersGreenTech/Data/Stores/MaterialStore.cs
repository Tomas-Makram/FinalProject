using EcoRecyclersGreenTech.Data.Users;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static EcoRecyclersGreenTech.Data.Stores.EnumsProductStatus;

namespace EcoRecyclersGreenTech.Data.Stores
{
    public class MaterialStore
    {
        [Key]
        public int MaterialID { get; set; }

        public int SellerID { get; set; }
        public User Seller { get; set; } = null!;

        public string? ProductType { get; set; }
        public int Quantity { get; set; }

        public string? ProductImgURL1 { get; set; }
        public string? ProductImgURL2 { get; set; }
        public string? ProductImgURL3 { get; set; }

        [Precision(18, 2)]
        public decimal Price { get; set; }

        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ProductStatus Status { get; set; } = ProductStatus.Available;

        public string? Unit { get; set; } = "unit";

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