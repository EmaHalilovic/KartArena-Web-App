using KartArena.Application.Abstractions;
using KartArena.Application.Modules.Catalog.Payments.Stripe;
using KartArena.Domain.Entities.Payments;
using KartArena.Domain.Entities.Reservations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;

namespace KartArena.Infrastructure.Payments;

public sealed class StripeWebhookService : IStripeWebhookService
{
    private readonly IAppDbContext _context;
    private readonly StripeSettings _settings;
    private readonly IBusinessClock _businessClock;

    public StripeWebhookService(IAppDbContext context, IOptions<StripeSettings> settings, IBusinessClock businessClock)
    {
        _context = context;
        _settings = settings.Value;
        _businessClock =businessClock;
    }

    public async Task ProcessWebhookAsync(
        string json,
        string stripeSignature,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_settings.WebhookSecret))
            throw new InvalidOperationException("Stripe webhook secret is not configured.");

        Event stripeEvent;
        try
        {
            stripeEvent = EventUtility.ConstructEvent(
                json, stripeSignature, _settings.WebhookSecret);
        }
        catch (StripeException exception)
        {
            throw new InvalidOperationException(
                "Stripe webhook signature is invalid.", exception);
        }

        if (stripeEvent.Data.Object is not Session session)
            return;

        switch (stripeEvent.Type)
        {
            case EventTypes.CheckoutSessionCompleted:
            case EventTypes.CheckoutSessionAsyncPaymentSucceeded:
                await ProcessCompletedSessionAsync(
                    session, stripeEvent.Id, cancellationToken);
                break;

            case EventTypes.CheckoutSessionExpired:
                await ProcessExpiredSessionAsync(
                    session, stripeEvent.Id, cancellationToken);
                break;

            case EventTypes.CheckoutSessionAsyncPaymentFailed:
                await ProcessFailedSessionAsync(
                    session, stripeEvent.Id, cancellationToken);
                break;
        }
    }

    private async Task ProcessCompletedSessionAsync(
        Session session,
        string stripeEventId,
        CancellationToken cancellationToken)
    {
        var now = _businessClock.UtcNow;

        var attempt = await GetAttemptWithPaymentAsync(session.Id, cancellationToken);
        if (attempt is null || attempt.Status is PaymentAttemptStatus.Paid or PaymentAttemptStatus.Refunded)
            return;

        if (!string.Equals(session.PaymentStatus, "paid", StringComparison.OrdinalIgnoreCase))
            return;

        var payment = attempt.Payment;
        var expectedAmount = checked((long)Math.Round(
            payment.Amount * 100m, MidpointRounding.AwayFromZero));

        if (session.AmountTotal != expectedAmount ||
            !string.Equals(session.Currency, payment.Currency, StringComparison.OrdinalIgnoreCase))
        {
            attempt.Status = PaymentAttemptStatus.Failed;
            attempt.IsActive = false;
            attempt.StripeEventId = stripeEventId;

            payment.Status = PaymentStatus.Failed;
            payment.StripeEventId = stripeEventId;
            payment.PaymentDate = null;
            payment.Note = "Stripe amount or currency does not match the expected payment.";

            SetReservationStatuses(payment, PaymentStatus.Failed, ReservationStatus.Cancelled, now);
            await _context.SaveChangesAsync(cancellationToken);
            return;
        }

        var reservationWasCancelled = payment.Status == PaymentStatus.Cancelled ||
            payment.PaymentReservations.Any(x =>
                x.Reservation.Status == ReservationStatus.Cancelled);

        if (reservationWasCancelled)
        {
            await RefundLatePaymentAsync(
                attempt, session, stripeEventId, cancellationToken);
            return;
        }

       
        attempt.Status = PaymentAttemptStatus.Paid;
        attempt.IsActive = false;
        attempt.StripeEventId = stripeEventId;
        
        payment.Status = PaymentStatus.Paid;
        payment.PaymentDate = now;
        payment.StripePaymentIntentId = session.PaymentIntentId;
        payment.StripeEventId = stripeEventId;
        payment.TransactionReference = session.PaymentIntentId;
        payment.Note = "Stripe payment successfully completed.";

        SetReservationStatuses(payment, PaymentStatus.Paid, ReservationStatus.Confirmed, now);

        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task RefundLatePaymentAsync(
        PaymentAttemptEntity attempt,
        Session session,
        string stripeEventId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(session.PaymentIntentId))
            throw new InvalidOperationException(
                "A late Stripe payment cannot be refunded because its PaymentIntent is missing.");

        await new RefundService().CreateAsync(
            new RefundCreateOptions { PaymentIntent = session.PaymentIntentId },
            new RequestOptions { IdempotencyKey = $"late-payment-refund-{stripeEventId}" },
            cancellationToken);

        var now = _businessClock.UtcNow;

        attempt.Status = PaymentAttemptStatus.Refunded;
        attempt.IsActive = false;
        attempt.StripeEventId = stripeEventId;

        attempt.Payment.Status = PaymentStatus.Refunded;
        attempt.Payment.PaymentDate = null;
        attempt.Payment.StripePaymentIntentId = session.PaymentIntentId;
        attempt.Payment.StripeEventId = stripeEventId;
        attempt.Payment.TransactionReference = session.PaymentIntentId;
        attempt.Payment.Note =
            "Stripe payment arrived after reservation expiry and was automatically refunded.";

        SetReservationStatuses(
            attempt.Payment, PaymentStatus.Refunded, ReservationStatus.Cancelled, now);

        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task ProcessExpiredSessionAsync(
        Session session,
        string stripeEventId,
        CancellationToken cancellationToken)
    {
        var attempt = await GetAttemptWithPaymentAsync(session.Id, cancellationToken);
        if (attempt is null || attempt.Status == PaymentAttemptStatus.Paid)
            return;

        var now = _businessClock.UtcNow;
        var wasActive = attempt.IsActive;

        attempt.Status = PaymentAttemptStatus.Expired;
        attempt.IsActive = false;
        attempt.StripeEventId = stripeEventId;

        if (wasActive &&
            attempt.Payment.Status != PaymentStatus.Paid)
        {
            attempt.Payment.Status = PaymentStatus.Cancelled;
            attempt.Payment.PaymentDate = null;
            attempt.Payment.StripeEventId = stripeEventId;
            attempt.Payment.Note =
                "Stripe Checkout Session expired.";

            SetReservationStatuses(
                attempt.Payment,
                PaymentStatus.Cancelled,
                ReservationStatus.Cancelled,
                now);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task ProcessFailedSessionAsync(
        Session session,
        string stripeEventId,
        CancellationToken cancellationToken)
    {
        var attempt = await GetAttemptWithPaymentAsync(session.Id, cancellationToken);
        
        if (attempt is null ||
        attempt.Status is PaymentAttemptStatus.Paid
            or PaymentAttemptStatus.Failed
            or PaymentAttemptStatus.Expired)
        {
            return;
        }

        var now = _businessClock.UtcNow;

        attempt.Status = PaymentAttemptStatus.Failed;
        attempt.IsActive = false;
        attempt.StripeEventId = stripeEventId;

        if (attempt.Payment.Status == PaymentStatus.Pending)
        {
            attempt.Payment.Status = PaymentStatus.Failed;
            attempt.Payment.StripeEventId = stripeEventId;
            attempt.Payment.PaymentDate = null;
            attempt.Payment.Note = "Stripe payment failed.";

            foreach (var link in attempt.Payment.PaymentReservations)
            {
                var reservation = link.Reservation;

                if (reservation.PaymentStatus == PaymentStatus.Paid)
                    continue;

                reservation.PaymentStatus = PaymentStatus.Failed;
                reservation.Status = ReservationStatus.Cancelled;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task<PaymentAttemptEntity?> GetAttemptWithPaymentAsync(
        string stripeCheckoutSessionId,
        CancellationToken cancellationToken)
    {
        var attempt = await _context.PaymentAttempts
            .Include(x => x.Payment)
                .ThenInclude(x => x.PaymentReservations)
                    .ThenInclude(x => x.Reservation)
            .SingleOrDefaultAsync(
                x => x.StripeCheckoutSessionId == stripeCheckoutSessionId && !x.IsDeleted,
                cancellationToken);

        if (attempt is not null)
            return attempt;

        // Preserve webhook handling for sessions created before PaymentAttempts existed.
        var legacyPayment = await _context.Payments
            .Include(x => x.PaymentReservations)
                .ThenInclude(x => x.Reservation)
            .SingleOrDefaultAsync(
                x => x.StripeCheckoutSessionId == stripeCheckoutSessionId && !x.IsDeleted,
                cancellationToken);

        if (legacyPayment is null)
            return null;

        attempt = new PaymentAttemptEntity
        {
            PaymentId = legacyPayment.Id,
            Payment = legacyPayment,
            IdempotencyKey = $"legacy-session-{stripeCheckoutSessionId}",
            StripeCheckoutSessionId = stripeCheckoutSessionId,
            Status = PaymentAttemptStatus.Pending,
            IsActive = legacyPayment.Status == PaymentStatus.Pending
        };

        _context.PaymentAttempts.Add(attempt);
        return attempt;
    }

    private static void SetReservationStatuses(
    PaymentEntity payment,
    PaymentStatus paymentStatus,
    ReservationStatus reservationStatus,
    DateTime now)
    {
        foreach (var link in payment.PaymentReservations)
        {
            link.Reservation.PaymentStatus = paymentStatus;
            link.Reservation.Status = reservationStatus;
        }
    }
}
