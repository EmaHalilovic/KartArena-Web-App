using KartArena.Application.Abstractions;
using KartArena.Application.Modules.Catalog.Payments;
using Microsoft.AspNetCore.Mvc;

namespace KartArena.API.Controllers
{
    [ApiController]
    [Route("api/stripe")]
    public class StripeWebhookController : ControllerBase
    {
        private readonly IStripeWebhookService
            _stripeWebhookService;

        private readonly ILogger<StripeWebhookController>
            _logger;

        public StripeWebhookController(
            IStripeWebhookService stripeWebhookService,
            ILogger<StripeWebhookController> logger)
        {
            _stripeWebhookService =
                stripeWebhookService;

            _logger = logger;
        }

        [HttpPost("webhook")]
        public async Task<IActionResult> HandleWebhook(
            CancellationToken cancellationToken)
        {
            string json;

            using (var reader =
                new StreamReader(Request.Body))
            {
                json =
                    await reader.ReadToEndAsync(
                        cancellationToken);
            }

            var stripeSignature =
                Request.Headers["Stripe-Signature"]
                    .ToString();

            if (string.IsNullOrWhiteSpace(
                    stripeSignature))
            {
                return BadRequest(
                    "Stripe-Signature header is missing.");
            }

            try
            {
                await _stripeWebhookService
                    .ProcessWebhookAsync(
                        json,
                        stripeSignature,
                        cancellationToken);

                return Ok();
            }
            catch (InvalidOperationException exception)
            {
                _logger.LogWarning(
                    exception,
                    "Invalid Stripe webhook.");

                return BadRequest();
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Stripe webhook processing failed.");

                return StatusCode(
                    StatusCodes
                        .Status500InternalServerError);
            }
        }
    }
}