using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EcoRecyclersGreenTech.Data.Stores;
using EcoRecyclersGreenTech.Data.Users;
using Microsoft.EntityFrameworkCore;

namespace EcoRecyclersGreenTech.Data.Orders
{
    public class AuctionOrder
    {
        [Key]
        public int AuctionOrderID { get; set; }

        [Required]
        public int AuctionStoreID { get; set; }
        [ForeignKey(nameof(AuctionStoreID))]
        public AuctionStore AuctionStore { get; set; } = null!;

        [Required]
        public int WinnerID { get; set; }
        [ForeignKey(nameof(WinnerID))]
        public User Winner { get; set; } = null!;

        [Required]
        public EnumsOrderStatus Status { get; set; } = EnumsOrderStatus.Pending;
        [Required]
        public DateTime OrderDate { get; set; } = DateTime.Now;

        // Bid info
        [Precision(18, 2)]
        public decimal BidAmount { get; set; }

        // Paid/Deposit info
        [Precision(18, 2)]
        public decimal AmountPaid { get; set; }
        [Precision(18, 2)]
        public decimal HeldAmount { get; set; }

        [Precision(18, 4)]
        public decimal DepositPercentUsed { get; set; }

        public string? PaymentProvider { get; set; }
        public string? PaymentProviderId { get; set; }

        public DateTime? CancelledAt { get; set; }

        // Buyer hide
        public bool HiddenForBidder { get; set; }
    }
}