using EcoRecyclersGreenTech.Data.Users;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
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
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public ProductStatus Status { get; set; } = ProductStatus.Available;
        public string? Unit { get; set; } = "unit";

        [Precision(18, 2)]
        public decimal? MinOrderQuantity { get; set; }
    }

}
