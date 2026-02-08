namespace EcoRecyclersGreenTech.Models.FactoryStore.Orders
{
    public class AuctionOrderDetailsModel
    {
        public int AuctionId { get; set; }
        public int AuctionOrderId { get; set; }

        // Auction
        public string? AuctionType { get; set; }
        public int Quantity { get; set; }
        public decimal StartPrice { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        // Bid
        public decimal BidAmount { get; set; }

        // Deposit
        public decimal AmountPaid { get; set; }
        public decimal DepositPaid { get; set; }
        public decimal HeldAmount { get; set; }

        public string Status { get; set; } = "";
        public DateTime OrderDate { get; set; }

        // bidder
        public int bidderId { get; set; }
        public string bidderName { get; set; } = "";
        public string? bidderProfileImgUrl { get; set; }
        public bool bidderVerified { get; set; }
        public string? bidderEmail { get; set; }
        public string? bidderPhone { get; set; }
        public string? bidderAddress { get; set; }

        // counts
        public int OrdersCount { get; set; }
        public int PendingCount { get; set; }
        public int ConfirmedCount { get; set; }

        // UI helpers
        public bool IsEnded { get; set; }
        public bool CanConfirmNow { get; set; }

        // Seller
        public string? SellerName { get; set; }

        public string? ImageUrl { get; set; }
        public List<string> ImageUrls { get; set; } = new();
    }
}