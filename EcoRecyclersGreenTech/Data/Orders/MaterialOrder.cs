using EcoRecyclersGreenTech.Data.Stores;
using EcoRecyclersGreenTech.Data.Users;

namespace EcoRecyclersGreenTech.Data.Orders
{
    public class MaterialOrder
    {
        public int MaterialOrderID { get; set; }
        public int MaterialStoreID { get; set; }
        public MaterialStore MaterialStore { get; set; } = null!;
        public int BuyerID { get; set; }
        public User Buyer { get; set; } = null!;
        public string Status { get; set; } = "Pending";
        public DateTime OrderDate { get; set; } = DateTime.Now;
        public DateTime? ArrivalDate { get; set; }
        public string? PickupLocation { get; set; }
    }
}
