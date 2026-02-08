using EcoRecyclersGreenTech.Data.Orders;

namespace EcoRecyclersGreenTech.Models.FactoryStore.Orders
{
    public class MaterialOrderDetailsModel
    {
        public int OrderId { get; set; }
        public int MaterialId { get; set; }
        public string? ProductType { get; set; }
        public EnumsOrderStatus OrderStatus { get; set; }
        public int Quantity { get; set; }
        public string? Unit { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal DepositPaid { get; set; }
        public decimal MaterialPrice { get; set; }
        public int MaterialAvailableQty { get; set; }
        public string? MaterialStatus { get; set; }
        public List<string> MaterialImages { get; set; } = new();

        public decimal Total => UnitPrice * Quantity;

        public EnumsOrderStatus? Status { get; set; }
        public DateTime OrderDate { get; set; }
        public string? PickupLocation { get; set; }
        public string? Description { get; set; }
        public DateTime? CancelUntil { get; set; }
        public DateTime? ExpectedArrivalDate { get; set; }

        // Buyer info
        public int BuyerId { get; set; }
        public string? BuyerName { get; set; }
        public string? BuyerEmail { get; set; }
        public string? BuyerPhone { get; set; }
        public string? BuyerAddress { get; set; }
        public string? BuyerProfileImg { get; set; }
        public bool BuyerVerified { get; set; }

        // Factory/Seller info (for factory view)
        public int FactoryId { get; set; }
        public string? FactoryName { get; set; }
        public string? FactoryAddress { get; set; }
        public string? FactoryEmail { get; set; }
        public string? FactoryPhone { get; set; }
        public string? FactoryProfileImg { get; set; }
        public bool FactoryVerified { get; set; }
    }
}
