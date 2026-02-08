using EcoRecyclersGreenTech.Models.Payment;
using Stripe;
using Stripe.Checkout;

namespace EcoRecyclersGreenTech.Services
{
    public class StripePayResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public bool RequiresRedirect { get; set; }
        public string? RedirectUrl { get; set; }

        public string Provider { get; set; } = "Stripe";
        public string? ProviderPaymentId { get; set; } // PaymentIntentId or SessionId
        public string? SessionId { get; set; }
    }

    public class StripePaymentService
    {
        public StripePaymentService(IConfiguration config)
        {
            StripeConfiguration.ApiKey = config["Stripe:SecretKey"];
        }

        public async Task<PaymentResultModel> PayAsync(bool confirmPayment, string? sessionId, string successUrl, string cancelUrl, string productName, decimal amount, string currency = "egp")
        {
            // ==========================
            // CONFIRM PAYMENT (after redirect)
            // ==========================
            if (confirmPayment)
            {
                if (string.IsNullOrWhiteSpace(sessionId))
                {
                    return Fail("Stripe session id is missing.");
                }

                try
                {
                    var service = new SessionService();
                    var session = await service.GetAsync(sessionId);

                    if (session.PaymentStatus != "paid")
                    {
                        return Fail("Payment not completed.");
                    }

                    return new PaymentResultModel
                    {
                        Success = true,
                        Provider = "Stripe",
                        ProviderPaymentId = session.PaymentIntentId ?? session.Id,
                        AmountPaid = session.AmountTotal.HasValue
                            ? session.AmountTotal.Value / 100m
                            : amount,
                        Currency = currency.ToUpper()
                    };
                }
                catch (Exception ex)
                {
                    return Fail("Failed to confirm payment: " + ex.Message);
                }
            }

            // ==========================
            // START PAYMENT
            // ==========================
            try
            {
                long amountCents = (long)Math.Round(
                    amount * 100m,
                    0,
                    MidpointRounding.AwayFromZero
                );

                var options = new SessionCreateOptions
                {
                    Mode = "payment",
                    SuccessUrl = successUrl + "&session_id={CHECKOUT_SESSION_ID}",
                    CancelUrl = cancelUrl,
                    LineItems = new List<SessionLineItemOptions>
                    {
                        new SessionLineItemOptions
                        {
                            Quantity = 1,
                            PriceData = new SessionLineItemPriceDataOptions
                            {
                                Currency = currency,
                                UnitAmount = amountCents,
                                ProductData =
                                    new SessionLineItemPriceDataProductDataOptions
                                    {
                                        Name = productName
                                    }
                            }
                        }
                    }
                };

                var service = new SessionService();
                var session = await service.CreateAsync(options);

                return new PaymentResultModel
                {
                    Success = true,
                    RequiresRedirect = true,
                    RedirectUrl = session.Url,
                    Provider = "Stripe"
                };
            }
            catch (Exception ex)
            {
                return Fail("Failed to start Stripe payment: " + ex.Message);
            }
        }

        private static PaymentResultModel Fail(string msg)
        {
            return new PaymentResultModel
            {
                Success = false,
                Message = msg
            };
        }

        public async Task<StripePayResult> CreateCheckoutSessionAsync(string successUrl, string cancelUrl, string name, long amountCents, string currency, Dictionary<string, string> metadata)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(successUrl))
                    return new StripePayResult { Success = false, Message = "Missing successUrl." };

                if (string.IsNullOrWhiteSpace(cancelUrl))
                    return new StripePayResult { Success = false, Message = "Missing cancelUrl." };

                if (string.IsNullOrWhiteSpace(name))
                    name = "Payment";

                if (amountCents <= 0)
                    return new StripePayResult { Success = false, Message = "Invalid amount." };

                currency = (currency ?? "usd").Trim().ToLowerInvariant();

                var options = new SessionCreateOptions
                {
                    Mode = "payment",
                    SuccessUrl = successUrl + (successUrl.Contains("?") ? "&" : "?") + "session_id={CHECKOUT_SESSION_ID}",
                    CancelUrl = cancelUrl,

                    // مهم: metadata هنا هتوصل للـ session (ممتاز للويبهوك)
                    Metadata = metadata,

                    LineItems = new List<SessionLineItemOptions>
                    {
                        new SessionLineItemOptions
                        {
                            Quantity = 1,
                            PriceData = new SessionLineItemPriceDataOptions
                            {
                                Currency = currency,
                                UnitAmount = amountCents,
                                ProductData = new SessionLineItemPriceDataProductDataOptions
                                {
                                    Name = name
                                }
                            }
                        }
                    },

                    // (اختياري) خلي Stripe يجمع billing address
                    // BillingAddressCollection = "auto",
                };

                var service = new SessionService();
                var session = await service.CreateAsync(options);

                // session.Url = صفحة الدفع
                // session.PaymentIntentId = id بتاع الدفع (مفيد للحفظ)
                return new StripePayResult
                {
                    Success = true,
                    RequiresRedirect = true,
                    RedirectUrl = session.Url,
                    SessionId = session.Id,
                    ProviderPaymentId = !string.IsNullOrWhiteSpace(session.PaymentIntentId) ? session.PaymentIntentId : session.Id,
                    Provider = "Stripe"
                };
            }
            catch (StripeException ex)
            {
                return new StripePayResult
                {
                    Success = false,
                    Message = ex.StripeError?.Message ?? ex.Message
                };
            }
            catch (Exception ex)
            {
                return new StripePayResult
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }
    }
}