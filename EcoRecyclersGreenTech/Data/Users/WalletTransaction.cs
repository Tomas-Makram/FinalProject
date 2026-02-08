using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcoRecyclersGreenTech.Data.Users
{
    // Wallet transaction type
    public enum WalletTxnType
    {
        Bonus = 1,
        Fine = 2,
        PaymentCredit = 3,
        PaymentDebit = 4,
        Refund = 5,
        Adjustment = 6,
        Hold = 7,
        ReleaseHold = 8
    }

    // Movement status
    public enum WalletTxnStatus
    {
        Succeeded = 1,
        Pending = 2,
        Failed = 3,
        Reversed = 4
    }

    public class WalletTransaction
    {
        [Key]
        public long Id { get; set; }

        [Required]
        public int WalletId { get; set; }

        [ForeignKey(nameof(WalletId))]
        public Wallet Wallet { get; set; } = null!;

        [Required]
        public WalletTxnType Type { get; set; }

        [Required]
        public WalletTxnStatus Status { get; set; } = WalletTxnStatus.Succeeded;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal BalanceAfter { get; set; }

        [MaxLength(3)]
        public string Currency { get; set; } = "EGP";

        [MaxLength(100)]
        public string? IdempotencyKey { get; set; }

        [MaxLength(500)]
        public string? Note { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    }
}