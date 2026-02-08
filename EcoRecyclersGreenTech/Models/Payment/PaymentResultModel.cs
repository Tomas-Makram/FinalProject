namespace EcoRecyclersGreenTech.Models.Payment
{
    public class PaymentResultModel
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";

        // Stripe flow
        public bool RequiresRedirect { get; set; }
        public string? RedirectUrl { get; set; }

        // Payment info
        public string Provider { get; set; } = "Stripe";
        public string? ProviderPaymentId { get; set; }

        public decimal AmountPaid { get; set; }
        public string Currency { get; set; } = "EGP";
    }

}
