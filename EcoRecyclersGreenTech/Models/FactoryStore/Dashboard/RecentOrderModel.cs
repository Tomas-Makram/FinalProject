namespace EcoRecyclersGreenTech.Models.FactoryStore.Dashboard
{
    public class RecentOrderModel
    {
        public string? OrderNumber { get; set; }
        public string? CustomerName { get; set; }
        public string? ProductName { get; set; }
        public decimal Amount { get; set; }
        public string? Status { get; set; }
        public DateTime OrderDate { get; set; }
    }
}
