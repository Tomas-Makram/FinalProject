namespace EcoRecyclersGreenTech.Models.CraftsMan
{
    public class RecentPendingOrderModel
    {
        public int Id { get; set; }
        public string Type { get; set; } = "";
        public string OrderNumber { get; set; } = "";
        public string SellerOrOwner { get; set; } = "";
        public decimal Amount { get; set; }
        public string Status { get; set; } = "";
        public DateTime OrderDate { get; set; }

        public string DetailsController { get; set; } = "";
        public string DetailsAction { get; set; } = "";
    }
}