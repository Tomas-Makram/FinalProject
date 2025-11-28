using EcoRecyclersGreenTech.Data.Users;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace EcoRecyclersGreenTech.Data.Stores
{
    public class MaterialStore
    {
        [Key]
        public int MaterialID { get; set; }
        public int SellerID { get; set; }
        public User Seller { get; set; } = null!;
        public string? ProductType { get; set; }
        public string? ProductImgURL { get; set; }
        public int Quantity { get; set; }
        public string? Description { get; set; }
        [Precision(18, 2)]
        public decimal Price { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

}
