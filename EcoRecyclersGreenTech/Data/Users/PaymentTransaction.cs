using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcoRecyclersGreenTech.Data.Users
{
    public enum PaymentStatus
    {
        Pending = 1,
        Succeeded = 2,
        Failed = 3,
        Refunded = 4,
        Canceled = 5
    }

    public class PaymentTransaction
    {
        [Key]
        public long Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public User User { get; set; } = null!;

        [MaxLength(100)]
        public string Provider { get; set; } = "Stripe";

        [Required]
        [MaxLength(255)]
        public string ProviderPaymentId { get; set; } = "";

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [MaxLength(3)]
        public string Currency { get; set; } = "EGP";

        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

        public long? WalletTransactionId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
