using KartArena.Application.Modules.Catalog.Payments.Stripe.Models;

namespace KartArena.Application.Abstractions
{
    public interface IStripePaymentService
    {
        Task<CreateCheckoutSessionResponse>
            CreateCheckoutSessionAsync(
                int reservationId,
                CancellationToken cancellationToken);
    }
}