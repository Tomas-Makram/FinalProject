namespace EcoRecyclersGreenTech.Models.Home
{
    public class LedgerModel
    {
        public LedgerSource Source { get; set; }
        public DateTime CreatedAt { get; set; }

        // Common
        public long Id { get; set; }
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public string Currency { get; set; } = "EGP";
        public decimal Amount { get; set; }

        // Wallet specific
        public int? WalletId { get; set; }
        public string? WalletType { get; set; }        // Bonus/Fine/...
        public string? WalletStatus { get; set; }      // Succeeded/...
        public decimal? BalanceAfter { get; set; }
        public string? IdempotencyKey { get; set; }
        public string? Note { get; set; }

        // Payment specific
        public string? Provider { get; set; }
        public string? ProviderPaymentId { get; set; }
        public string? PaymentStatus { get; set; }     // Pending/Succeeded/...
        public long? WalletTransactionId { get; set; } // link if exists
    }
}
