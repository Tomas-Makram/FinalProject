using EcoRecyclersGreenTech.Services;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;

namespace EcoRecyclersGreenTech.Controllers
{
    [ApiController]
    [Route("api/payments")]
    public class PaymentsController : ControllerBase
    {
        private readonly IFactoryStoreService _factoryService;
        private readonly ILogger<PaymentsController> _logger;
        private readonly IConfiguration _config;

        public PaymentsController(IFactoryStoreService factoryService, ILogger<PaymentsController> logger, IConfiguration config)
        {
            _factoryService = factoryService;
            _logger = logger;
            _config = config;
        }

        [HttpPost("stripe-webhook")]
        public async Task<IActionResult> StripeWebhook()
        {
            var json = await new StreamReader(Request.Body).ReadToEndAsync();

            try
            {
                var webhookSecret = _config["Stripe:WebhookSecret"];
                if (string.IsNullOrWhiteSpace(webhookSecret))
                    return BadRequest("Missing Stripe webhook secret");

                var stripeEvent = EventUtility.ConstructEvent(
                    json,
                    Request.Headers["Stripe-Signature"],
                    webhookSecret
                );

                // ✅ safest: use string type (works across versions)
                if (stripeEvent.Type == "checkout.session.completed")
                {
                    var session = stripeEvent.Data.Object as Session;
                    if (session == null)
                        return Ok();

                    // Metadata اللي انت حاططها وقت إنشاء الـ Session
                    // لازم تكون موجودة
                    var userId = int.Parse(session.Metadata["UserId"]);
                    var materialId = int.Parse(session.Metadata["MaterialId"]);
                    var quantity = int.Parse(session.Metadata["Quantity"]);
                    var walletAmount = decimal.Parse(session.Metadata["WalletAmount"]);
                    var deposit = decimal.Parse(session.Metadata["Deposit"]);

                    // ✅ هنا نفّذ إنشاء الأوردر بعد ما الدفع أكد
                    await _factoryService.PlaceMaterialOrderAsync(
                        buyerId: userId,
                        materialId: materialId,
                        quantity: quantity,
                        depositPaid: deposit,            // هنا Stripe دفع deposit كامل
                        walletUsed: walletAmount,         // لو هتسحب من محفظة هنا/داخل السيرفيس
                        provider: "Stripe",
                        providerPaymentId: session.PaymentIntentId ?? session.Id
                    );
                }

                return Ok();
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe webhook error");
                return BadRequest();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Webhook failed");
                return StatusCode(500);
            }
        }

        [HttpGet("success")]
        public IActionResult Success()
        {
            return Ok("Payment succeeded. You can close this page.");
        }

        [HttpGet("cancel")]
        public IActionResult Cancel()
        {
            return Ok("Payment canceled.");
        }
    }
}