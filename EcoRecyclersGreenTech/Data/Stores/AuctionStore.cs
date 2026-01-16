using EcoRecyclersGreenTech.Data.Users;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
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
    }
}
