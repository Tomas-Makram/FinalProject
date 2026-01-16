using EcoRecyclersGreenTech.Models.FactoryStore.Dashboard;

namespace EcoRecyclersGreenTech.Models.FactoryStore.Orders
{
    public class DashboardModel
    {
        public int TotalProducts { get; set; }
        public int ActiveProducts { get; set; }
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public int CompletedOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal PendingRevenue { get; set; }
        public int ActiveMaterials { get; set; }
        public int ActiveMachines { get; set; }
        public int ActiveRentals { get; set; }
        public int ActiveAuctions { get; set; }
        public int ActiveJobs { get; set; }
        public List<RecentOrderModel>? RecentOrders { get; set; }
        public List<TopProductModel>? TopProducts { get; set; }
    }

}
