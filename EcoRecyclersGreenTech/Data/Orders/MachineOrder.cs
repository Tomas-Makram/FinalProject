using EcoRecyclersGreenTech.Data.Stores;
using EcoRecyclersGreenTech.Data.Users;

namespace EcoRecyclersGreenTech.Data.Orders
{
    public class MachineOrder
    {
        public int MachineOrderID { get; set; }
        public int MachineStoreID { get; set; }
        public MachineStore MachineStore { get; set; } = null!;
        public int BuyerID { get; set; }
        public User Buyer { get; set; } = null!;
        public string Status { get; set; } = "Pending";
        public DateTime OrderDate { get; set; } = DateTime.Now;
        public DateTime? ArrivalDate { get; set; }
        public string? PickupLocation { get; set; }
    }
}
