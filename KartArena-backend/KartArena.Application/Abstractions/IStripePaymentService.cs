using KartArena.Application.Modules.Catalog.Payments.Stripe.Models;

namespace KartArena.Application.Abstractions
{
    public interface IStripePaymentService
    {
        Task<CreateCheckoutSessionResponse>
             CreateCheckoutSessionAsync(
                 int paymentId,
                 CancellationToken cancellationToken);

        Task<bool> ExpireCheckoutSessionAsync(
            string stripeCheckoutSessionId,
            CancellationToken cancellationToken);
    }
}
