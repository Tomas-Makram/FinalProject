using EcoRecyclersGreenTech.Data.Orders;

namespace EcoRecyclersGreenTech.Models.FactoryStore.Orders
{
    public class MachineOrderDetailsModel
    {
        // Order
        public int OrderId { get; set; }
        public string OrderStatus { get; set; } = "";
        public DateTime OrderDate { get; set; }

        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Total => UnitPrice * Quantity;

        public DateTime? CancelUntil { get; set; }
        public DateTime? ExpectedArrivalDate { get; set; }
        public string? PickupLocation { get; set; }

        // Machine
        public int MachineId { get; set; }
        public string? MachineType { get; set; }
        public string? MachineName { get; set; }
        public string? Description { get; set; }
        public decimal MachinePrice { get; set; }
        public int MachineAvailableQty { get; set; }
        public string? MachineStatus { get; set; }
        public List<string>? MachineImages { get; set; }

        // Buyer
        public int BuyerId { get; set; }
        public string? BuyerName { get; set; }
        public string? BuyerEmail { get; set; }
        public string? BuyerPhone { get; set; }
        public string? BuyerAddress { get; set; }
        public string? BuyerProfileImg { get; set; }
        public bool BuyerVerified { get; set; }
        public EnumsOrderStatus? Status { get; set; }

        // Seller
        public int FactoryId { get; set; }
        public string? FactoryName { get; set; }
        public string? FactoryAddress { get; set; }
        public string? FactoryEmail { get; set; }
        public string? FactoryPhone { get; set; }
        public string? FactoryProfileImg { get; set; }
        public bool FactoryVerified { get; set; }

        // Wallet 
        public decimal TotalPrice { get; set; }
        public decimal DepositPaid { get; set; }

    }
}
