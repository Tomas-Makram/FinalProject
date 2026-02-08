using EcoRecyclersGreenTech.Data.Orders;

namespace EcoRecyclersGreenTech.Models.Home
{
    public class FactoryHomeModel
    {
        public int TotalActive { get; set; }
        public int OverdueCount { get; set; }
        public int DueTodayCount { get; set; }
        public int DueSoonCount { get; set; }
        public int CreatedTodayCount { get; set; }

        public List<FactoryOrdersModel> Overdue { get; set; } = new();
        public List<FactoryOrdersModel> DueToday { get; set; } = new();
        public List<FactoryOrdersModel> DueSoon { get; set; } = new();
        public List<FactoryOrdersModel> CreatedToday { get; set; } = new();
        public List<FactoryOrdersModel> ActiveAll { get; set; } = new();
    }
}