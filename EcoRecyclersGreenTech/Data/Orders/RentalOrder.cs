using EcoRecyclersGreenTech.Data.Orders;
using EcoRecyclersGreenTech.Data.Stores;
using EcoRecyclersGreenTech.Data.Users;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace EcoRecyclersGreenTech.Data.Orders
{
    public class RentalOrder
    {
        [Key]
        public int RentalOrderID { get; set; }

        [Required]
        public int RentalStoreID { get; set; }
        [ForeignKey(nameof(RentalStoreID))]
        public RentalStore RentalStore { get; set; } = null!;

        [Required]
        public int BuyerID { get; set; }
        [ForeignKey(nameof(BuyerID))]
        public User Buyer { get; set; } = null!;

        [Required]
        public EnumsOrderStatus Status { get; set; } = EnumsOrderStatus.Pending;

        public bool HiddenForBuyer { get; set; }

        [Required]
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        [Precision(18, 2)]
        public decimal AmountPaid { get; set; }

        public int MonthsPaid { get; set; } = 3;

        [MaxLength(30)]
        public string PaymentProvider { get; set; } = "Stripe";

        [MaxLength(200)]
        public string PaymentProviderId { get; set; } = ""; // PaymentIntentId / SessionId

        [Precision(18, 2)]
        public decimal HeldAmount { get; set; }

        public DateTime? CancelledAt { get; set; }
    }
}