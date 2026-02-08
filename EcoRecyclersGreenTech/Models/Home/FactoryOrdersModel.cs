namespace EcoRecyclersGreenTech.Models.Home
{
    public class FactoryOrdersModel
    {
        public string Source { get; set; } = "";      // Material/Machine/Auction/Rental/Job
        public string OrderNo { get; set; } = "";     // MAT-1, MAC-2...
        public string Status { get; set; } = "";
        public DateTime OrderDate { get; set; }
        public DateTime? DueDate { get; set; }
        public string? Location { get; set; }
        public string? CustomerLabel { get; set; }
        public decimal? TotalAmount { get; set; }
        public string? Note { get; set; }
    }

}
