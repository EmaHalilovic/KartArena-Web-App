using KartArena.Application.Abstractions;
using KartArena.Application.Modules.Catalog.Payments.Stripe;
using KartArena.Domain.Entities.Payments;
using KartArena.Domain.Entities.Reservations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;

namespace KartArena.Infrastructure.Payments
{
    public sealed class StripeWebhookService
        : IStripeWebhookService
    {
        private readonly IAppDbContext _context;
        private readonly StripeSettings _settings;

        public StripeWebhookService(
            IAppDbContext context,
            IOptions<StripeSettings> settings)
        {
            _context = context;
            _settings = settings.Value;
        }

        public async Task ProcessWebhookAsync(
            string json,
            string stripeSignature,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(
                    _settings.WebhookSecret))
            {
                throw new InvalidOperationException(
                    "Stripe webhook secret is not configured.");
            }

            Event stripeEvent;

            try
            {
                stripeEvent =
                    EventUtility.ConstructEvent(
                        json,
                        stripeSignature,
                        _settings.WebhookSecret);
            }
            catch (StripeException exception)
            {
                throw new InvalidOperationException(
                    "Stripe webhook signature is invalid.",
                    exception);
            }

            switch (stripeEvent.Type)
            {
                case EventTypes.CheckoutSessionCompleted:
                case EventTypes.CheckoutSessionAsyncPaymentSucceeded:
                    {
                        if (stripeEvent.Data.Object is Session session)
                        {
                            await ProcessCompletedSessionAsync(
                                session,
                                stripeEvent.Id,
                                cancellationToken);
                        }

                        break;
                    }

                case EventTypes.CheckoutSessionExpired:
                    {
                        if (stripeEvent.Data.Object is Session session)
                        {
                            await ProcessExpiredSessionAsync(
                                session,
                                stripeEvent.Id,
                                cancellationToken);
                        }

                        break;
                    }

                case EventTypes.CheckoutSessionAsyncPaymentFailed:
                    {
                        if (stripeEvent.Data.Object is Session session)
                        {
                            await ProcessFailedSessionAsync(
                                session,
                                stripeEvent.Id,
                                cancellationToken);
                        }

                        break;
                    }
            }
        }

        private async Task ProcessCompletedSessionAsync(
            Session session,
            string stripeEventId,
            CancellationToken cancellationToken)
        {
            var payment = await GetPaymentWithReservationsAsync(
                session.Id,
                cancellationToken);

            if (payment is null)
            {
                throw new KeyNotFoundException(
                    $"Payment for Stripe session {session.Id} was not found.");
            }

            // Stripe može više puta poslati isti događaj.
            if (payment.Status == PaymentStatus.Paid)
            {
                return;
            }

            if (!string.Equals(
                    session.PaymentStatus,
                    "paid",
                    StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var expectedAmount = checked(
                (long)Math.Round(
                    payment.Amount * 100m,
                    MidpointRounding.AwayFromZero));

            if (session.AmountTotal != expectedAmount)
            {
                payment.Status = PaymentStatus.Failed;
                payment.StripeEventId = stripeEventId;
                payment.Note =
                    "Stripe amount does not match the expected payment amount.";
                payment.ModifiedAtUtc = DateTime.UtcNow;

                SetReservationPaymentStatuses(
                    payment,
                    PaymentStatus.Failed);

                await _context.SaveChangesAsync(
                    cancellationToken);

                return;
            }

            if (!string.Equals(
                    session.Currency,
                    payment.Currency,
                    StringComparison.OrdinalIgnoreCase))
            {
                payment.Status = PaymentStatus.Failed;
                payment.StripeEventId = stripeEventId;
                payment.Note =
                    "Stripe currency does not match the expected currency.";
                payment.ModifiedAtUtc = DateTime.UtcNow;

                SetReservationPaymentStatuses(
                    payment,
                    PaymentStatus.Failed);

                await _context.SaveChangesAsync(
                    cancellationToken);

                return;
            }

            var now = DateTime.UtcNow;

            payment.Status = PaymentStatus.Paid;
            payment.PaymentDate = now;
            payment.StripePaymentIntentId =
                session.PaymentIntentId;
            payment.StripeEventId = stripeEventId;
            payment.TransactionReference =
                session.PaymentIntentId;
            payment.Note =
                "Stripe payment successfully completed.";
            payment.ModifiedAtUtc = now;

            foreach (var link in payment.PaymentReservations)
            {
                var reservation = link.Reservation;

                reservation.PaymentStatus =
                    PaymentStatus.Paid;

                reservation.Status =
                    ReservationStatus.Confirmed;

                reservation.ModifiedAtUtc = now;
            }

            await _context.SaveChangesAsync(
                cancellationToken);
        }

        private async Task ProcessExpiredSessionAsync(
            Session session,
            string stripeEventId,
            CancellationToken cancellationToken)
        {
            var payment = await GetPaymentWithReservationsAsync(
                session.Id,
                cancellationToken);

            if (payment is null ||
                payment.Status == PaymentStatus.Paid)
            {
                return;
            }

            var now = DateTime.UtcNow;

            payment.Status =
                PaymentStatus.Cancelled;

            payment.StripeEventId =
                stripeEventId;

            payment.Note =
                "Stripe Checkout Session expired.";

            payment.ModifiedAtUtc =
                now;

            foreach (var link in payment.PaymentReservations)
            {
                var reservation = link.Reservation;

                reservation.PaymentStatus =
                    PaymentStatus.Cancelled;

                reservation.Status =
                    ReservationStatus.Cancelled;

                reservation.ModifiedAtUtc =
                    now;
            }

            await _context.SaveChangesAsync(
                cancellationToken);
        }

        private async Task ProcessFailedSessionAsync(
            Session session,
            string stripeEventId,
            CancellationToken cancellationToken)
        {
            var payment = await GetPaymentWithReservationsAsync(
                session.Id,
                cancellationToken);

            if (payment is null ||
                payment.Status == PaymentStatus.Paid)
            {
                return;
            }

            var now = DateTime.UtcNow;

            payment.Status =
                PaymentStatus.Failed;

            payment.StripeEventId =
                stripeEventId;

            payment.Note =
                "Stripe payment failed.";

            payment.ModifiedAtUtc =
                now;

            /*
             * Rezervacije ostaju Pending kako bi korisnik
             * mogao ponovo pokušati platiti prije isteka
             * dozvoljenog vremena.
             */
            foreach (var link in payment.PaymentReservations)
            {
                link.Reservation.PaymentStatus =
                    PaymentStatus.Failed;

                link.Reservation.ModifiedAtUtc =
                    now;
            }

            await _context.SaveChangesAsync(
                cancellationToken);
        }

        private async Task<PaymentEntity?>
            GetPaymentWithReservationsAsync(
                string stripeCheckoutSessionId,
                CancellationToken cancellationToken)
        {
            return await _context.Payments
                .Include(x => x.PaymentReservations)
                    .ThenInclude(x => x.Reservation)
                .SingleOrDefaultAsync(
                    x =>
                        x.StripeCheckoutSessionId ==
                            stripeCheckoutSessionId &&
                        !x.IsDeleted,
                    cancellationToken);
        }

        private static void SetReservationPaymentStatuses(
            PaymentEntity payment,
            PaymentStatus status)
        {
            var now = DateTime.UtcNow;

            foreach (var link in payment.PaymentReservations)
            {
                link.Reservation.PaymentStatus =
                    status;

                link.Reservation.ModifiedAtUtc =
                    now;
            }
        }
    }
}