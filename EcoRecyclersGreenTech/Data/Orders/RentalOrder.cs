using EcoRecyclersGreenTech.Data.Stores;
using EcoRecyclersGreenTech.Data.Users;

namespace EcoRecyclersGreenTech.Data.Orders
{
    public class RentalOrder
    {
        public int RentalOrderID { get; set; }
        public int RentalStoreID { get; set; }
        public RentalStore RentalStore { get; set; } = null!;
        public int BuyerID { get; set; }
        public User Buyer { get; set; } = null!;
        public string Status { get; set; } = "Pending";
        public DateTime OrderDate { get; set; } = DateTime.Now;
    }
}
