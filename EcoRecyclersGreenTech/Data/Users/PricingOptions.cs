namespace EcoRecyclersGreenTech.Data.Users
{
    public class PricingOptions
    {
        public decimal PlatformFeePercent { get; set; } = 0.02m;
        
        public decimal DepositPercent { get; set; } = 0.10m;

        public decimal AuctionDepositPercent { get; set; } = 0.20m;

        public int RentalMonthsUpfront { get; set; } = 3;
    }
}