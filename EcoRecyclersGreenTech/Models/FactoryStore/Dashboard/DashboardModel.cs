using EcoRecyclersGreenTech.Models.FactoryStore.Dashboard;

namespace EcoRecyclersGreenTech.Models.FactoryStore.Orders
{
    public class DashboardModel
    {
        public int TotalProducts { get; set; }
        public decimal TotalRevenue { get; set; }
        public int ActiveMaterials { get; set; }
        public int ActiveMachines { get; set; }
        public int ActiveRentals { get; set; }
        public int ActiveAuctions { get; set; }
        public int ActiveJobs { get; set; }
        public List<RecentOrderModel>? RecentOrders { get; set; }
    }
}
