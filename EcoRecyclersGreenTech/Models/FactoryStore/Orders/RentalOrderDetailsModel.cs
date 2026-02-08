namespace EcoRecyclersGreenTech.Models.FactoryStore.Orders
{
    public class RentalOrderDetailsModel
    {
        // Order
        public int RentalOrderId { get; set; }
        public string Status { get; set; } = "";
        public DateTime OrderDate { get; set; }

        // Listing (Rental)
        public int RentalId { get; set; }
        public FactoryStoreModel Listing { get; set; } = default!;

        public string? RentalAddress { get; set; }
        public decimal PricePerMonth { get; set; }

        // Make dates nullable (safer)
        public DateTime? AvailableFrom { get; set; }
        public DateTime? AvailableUntil { get; set; }

        // Buyer
        public int BuyerId { get; set; }
        public string BuyerName { get; set; } = "";
        public string? BuyerProfileImgUrl { get; set; }
        public bool BuyerVerified { get; set; }
        public string? BuyerEmail { get; set; }
        public string? BuyerPhone { get; set; }
        public string? BuyerAddress { get; set; }

        // Cancel rules
        public bool CanCancel { get; set; }
        public DateTime? CancelUntil { get; set; }

        // Rental terms
        public int Months { get; set; } = 3;

        // Payment summary (optional)
        public decimal? TotalPaid { get; set; }
        public decimal? WalletUsed { get; set; }
        public decimal? StripePaid { get; set; }
        public decimal? PlatformFee { get; set; }
        public decimal? HeldAmount { get; set; }

        public string? ImageUrl { get; set; }
        public int OrdersCount { get; set; }
        public int PendingCount { get; set; }
        public int ConfirmedCount { get; set; }
        public List<string> ImageUrls { get; set; } = new();
    }
}
