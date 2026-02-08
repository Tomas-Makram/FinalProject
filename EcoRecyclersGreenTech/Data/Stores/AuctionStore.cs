using EcoRecyclersGreenTech.Data.Users;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static EcoRecyclersGreenTech.Data.Stores.EnumsProductStatus;

namespace EcoRecyclersGreenTech.Data.Stores
{
    public class AuctionStore
    {
        [Key]
        public int AuctionID { get; set; }
        public int SellerID { get; set; }
        public User Seller { get; set; } = null!;
        public string? ProductType { get; set; }
        public string? ProductImgURL1 { get; set; }
        public string? ProductImgURL2 { get; set; }
        public string? ProductImgURL3 { get; set; }

        public int Quantity { get; set; }

        [Precision(18, 2)]
        public decimal StartPrice { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public ProductStatus Status { get; set; } = ProductStatus.Available;
        public string? Description { get; set; }
        
        public int? ConfirmedOrderId { get; set; }
        public DateTime? ConfirmedAt { get; set; }

        [Precision(18, 2)]
        public decimal? CurrentTopBid { get; set; }

        public int? CurrentTopBidderId { get; set; }

        // GPS Coordinates
        [Column(TypeName = "decimal(9,6)")]
        public decimal? Latitude { get; set; }

        [Column(TypeName = "decimal(9,6)")]
        public decimal? Longitude { get; set; }

        [MaxLength(255)]
        public string? Address { get; set; }
    }
}
