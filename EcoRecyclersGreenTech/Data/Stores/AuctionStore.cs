using EcoRecyclersGreenTech.Data.Users;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace EcoRecyclersGreenTech.Data.Stores
{
    public class AuctionStore
    {
        [Key]
        public int AuctionID { get; set; }
        public int SellerID { get; set; }
        public User Seller { get; set; } = null!;
        public string? ProductType { get; set; }
        public string? ProductImgURL { get; set; }
        public int Quantity { get; set; }

        [Precision(18, 2)]
        public decimal StartPrice { get; set; }
        public DateTime AuctionDate { get; set; }
    }

}
