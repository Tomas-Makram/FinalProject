namespace EcoRecyclersGreenTech.Models.CraftsMan
{
    public class CraftsManDashboardModel
    {
        // Public store counts (Available + verified sellers)
        public int PublicMaterials { get; set; }
        public int PublicMachines { get; set; }
        public int PublicRentals { get; set; }
        public int PublicAuctions { get; set; }

        // My pending orders (not delivered/completed)
        public int MyPendingMaterialOrders { get; set; }
        public int MyPendingMachineOrders { get; set; }
        public int MyPendingRentalOrders { get; set; }
        public int MyPendingAuctionOrders { get; set; }

        public int TotalMyPendingOrders => MyPendingMaterialOrders + MyPendingMachineOrders + MyPendingRentalOrders + MyPendingAuctionOrders;

        public string UserName { get; set; } = "User";
        public bool IsVerified { get; set; }

        public List<RecentPendingOrderModel> RecentPending { get; set; } = new();
    }
}
