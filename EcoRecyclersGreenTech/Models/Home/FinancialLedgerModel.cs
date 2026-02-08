namespace EcoRecyclersGreenTech.Models.Home
{
    public enum LedgerSource
    {
        Wallet = 1,
        Payment = 2
    }

    public class FinancialLedgerModel
    {
        public string? Query { get; set; }     // search text
        public int? UserId { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }

        public List<LedgerModel> Rows { get; set; } = new();
    }
}
