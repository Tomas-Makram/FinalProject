using EcoRecyclersGreenTech.Data.Stores;
using EcoRecyclersGreenTech.Data.Users;

namespace EcoRecyclersGreenTech.Data.Orders
{
    public class AuctionOrder
    {
        public int AuctionOrderID { get; set; }
        public int AuctionStoreID { get; set; }
        public AuctionStore AuctionStore { get; set; } = null!;
        public int WinnerID { get; set; }
        public User Winner { get; set; } = null!;
        public string Status { get; set; } = "Pending";
        public DateTime OrderDate { get; set; } = DateTime.Now;
    }
}
