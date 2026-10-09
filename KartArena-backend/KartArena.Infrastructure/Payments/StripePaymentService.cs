using KartArena.Application.Abstractions;
using KartArena.Application.Common.Exceptions;
using KartArena.Application.Modules.Catalog.Payments.Stripe.Models;
using KartArena.Domain.Entities.Payments;
using KartArena.Domain.Entities.Reservations;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Stripe;
using Stripe.Checkout;

namespace KartArena.Infrastructure.Payments;

public sealed class StripePaymentService : IStripePaymentService
{
    private const string Currency = "bam";
    private readonly IAppDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<StripePaymentService> _logger;

    public StripePaymentService(IAppDbContext context, IConfiguration configuration, ILogger<StripePaymentService> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<CreateCheckoutSessionResponse> CreateCheckoutSessionAsync(
        int paymentId,
        CancellationToken cancellationToken)
    {
        var payment = await _context.Payments
            .Include(x => x.PaymentType)
            .Include(x => x.PaymentAttempts)
            .Include(x => x.PaymentReservations).ThenInclude(x => x.Reservation).ThenInclude(x => x.Track)
            .Include(x => x.PaymentReservations).ThenInclude(x => x.Reservation).ThenInclude(x => x.Kart)
            .SingleOrDefaultAsync(x => x.Id == paymentId && !x.IsDeleted, cancellationToken);

        if (payment is null)
            throw new MarketNotFoundException("Payment was not found.");
        if (payment.PaymentReservations.Count == 0)
            throw new MarketBusinessRuleException("PAYMENT_NO_RESERVATIONS","Payment does not contain reservations.");
        if (payment.Status == PaymentStatus.Paid)
            throw new MarketBusinessRuleException("PAYMENT_ALREADY_COMPLETED","Payment is already completed.");

        var reservations = payment.PaymentReservations.Select(x => x.Reservation).ToList();
        if (reservations.Any(x => x.Status == ReservationStatus.Cancelled))
            throw new MarketBusinessRuleException("PAYMENT_CANCELLED_RESERVATIONS","Cancelled reservations cannot be paid.");
        if (reservations.Any(x => x.PaymentStatus == PaymentStatus.Paid))
            throw new MarketBusinessRuleException("PAYMENT_ALREADY_PAID","One or more reservations are already paid.");

        if (payment.PaymentAttempts.Count == 0 &&
            !string.IsNullOrWhiteSpace(payment.StripeCheckoutSessionId))
        {
            var legacyResponse = await RestoreLegacySessionAsync(
                payment, reservations, cancellationToken);
            if (legacyResponse is not null)
                return legacyResponse;
        }

        var activeAttempt = payment.PaymentAttempts.SingleOrDefault(x => x.IsActive);
        if (activeAttempt is not null)
        {
            if (activeAttempt.Status == PaymentAttemptStatus.Pending &&
                !string.IsNullOrWhiteSpace(activeAttempt.StripeCheckoutSessionId) &&
                !string.IsNullOrWhiteSpace(activeAttempt.CheckoutUrl))
            {
                return BuildResponse(payment, reservations,
                    activeAttempt.StripeCheckoutSessionId, activeAttempt.CheckoutUrl);
            }

            throw new MarketBusinessRuleException("PAYMENT_ATTEMPT_ALREADY_CREATING",
                "A Stripe checkout attempt is already being created. Please try again shortly.");
        }

        if (payment.Amount <= 0)
            throw new MarketBusinessRuleException("PAYMENT_AMOUNT_INVALID","Payment amount must be greater than zero.");

        var stripePaymentType = await _context.PaymentTypes.SingleOrDefaultAsync(
            x => x.Code == "STRIPE" && x.AllowedOnline && !x.IsDeleted,
            cancellationToken);
        if (stripePaymentType is null)
            throw new MarketConfigurationException(
         "stripe.configuration.payment_type_missing",
         "Online payment is currently unavailable.");

        var frontendUrl = _configuration["FrontendUrl"];
        if (string.IsNullOrWhiteSpace(frontendUrl))
        {
            throw new MarketConfigurationException(
                "stripe.configuration.frontend_url_missing",
                "Online payment is currently unavailable.");
        }

        var attempt = new PaymentAttemptEntity
        {
            PaymentId = payment.Id,
            IdempotencyKey = $"payment-{payment.Id}-attempt-{Guid.NewGuid():N}",
            Status = PaymentAttemptStatus.Creating,
            IsActive = true
        };
        _context.PaymentAttempts.Add(attempt);

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception)
        when (IsUniqueConstraintViolation(exception))
        {
            _logger.LogWarning(
                exception,
                "A concurrent Stripe checkout attempt already exists for payment {PaymentId}.",
                payment.Id);

            throw new MarketBusinessRuleException(
                "PAYMENT_ATTEMPT_ALREADY_EXISTS",
                "A checkout attempt already exists for this payment.");
        }
        catch (DbUpdateException exception)
        {
            _logger.LogError(
                exception,
                "Failed to save Stripe checkout attempt for payment {PaymentId}.",
                payment.Id);

            throw;
        }

        var amountInMinorUnits = checked((long)Math.Round(
            payment.Amount * 100m, MidpointRounding.AwayFromZero));
        var reservationIds = reservations.Select(x => x.Id).ToList();
        var options = new SessionCreateOptions
        {
            Mode = "payment",
            SuccessUrl = $"{frontendUrl}/payment/success?session_id={{CHECKOUT_SESSION_ID}}",
            CancelUrl = $"{frontendUrl}/payment/cancelled?paymentId={payment.Id}",
            ClientReferenceId = payment.Id.ToString(),
            CustomerEmail = reservations.Select(x => x.CustomerEmail)
                .FirstOrDefault(x => !string.IsNullOrWhiteSpace(x)),
            Metadata = new Dictionary<string, string>
            {
                ["paymentId"] = payment.Id.ToString(),
                ["paymentAttemptId"] = attempt.Id.ToString(),
                ["reservationIds"] = string.Join(",", reservationIds)
            },
            PaymentIntentData = new SessionPaymentIntentDataOptions
            {
                Metadata = new Dictionary<string, string>
                {
                    ["paymentId"] = payment.Id.ToString(),
                    ["paymentAttemptId"] = attempt.Id.ToString()
                }
            },
            LineItems =
            [
                new SessionLineItemOptions
                {
                    Quantity = 1,
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = Currency,
                        UnitAmount = amountInMinorUnits,
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = "Kart Arena reservation checkout",
                            Description = CreateDescription(reservations)
                        }
                    }
                }
            ]
        };

        Session? createdSession = null;

        try
        {
            createdSession = await new SessionService().CreateAsync(
                options,
                new RequestOptions { IdempotencyKey = attempt.IdempotencyKey },
                cancellationToken);

            if (string.IsNullOrWhiteSpace(createdSession.Url))
            {
                throw new MarketExternalServiceException(
                    "stripe.checkout.invalid_response",
                    "The payment session could not be created.");
            }

            attempt.StripeCheckoutSessionId = createdSession.Id;
            attempt.CheckoutUrl = createdSession.Url;
            attempt.Status = PaymentAttemptStatus.Pending;

            payment.Currency = Currency;
            payment.Status = PaymentStatus.Pending;
            payment.PaymentDate = null;
            payment.StripeCheckoutSessionId = createdSession.Id;
            payment.StripePaymentIntentId = null;
            payment.StripeEventId = null;
            payment.TransactionReference = createdSession.Id;
            payment.PaymentTypeId = stripePaymentType.Id;
            payment.Note = "Stripe Checkout Session created.";

            foreach (var reservation in reservations)
                reservation.PaymentStatus = PaymentStatus.Pending;

            await _context.SaveChangesAsync(cancellationToken);
            return BuildResponse(payment, reservations, createdSession.Id, createdSession.Url);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (MarketExternalServiceException)
        {
            await MarkCheckoutCreationAsFailedAsync(
                createdSession,
                attempt,
                payment,
                reservations,
                cancellationToken);

            throw;
        }
        catch (StripeException exception)
        {
            _logger.LogError(
                exception,
                "Stripe failed to create checkout session for payment {PaymentId}.",
                payment.Id);

            await MarkCheckoutCreationAsFailedAsync(
                createdSession,
                attempt,
                payment,
                reservations,
                cancellationToken);

            throw new MarketExternalServiceException(
                "stripe.checkout.creation_failed",
                "The payment provider is currently unavailable. Please try again.",
                exception);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Unexpected error while creating Stripe checkout session for payment {PaymentId}.",
                payment.Id);

            await MarkCheckoutCreationAsFailedAsync(
                createdSession,
                attempt,
                payment,
                reservations,
                cancellationToken);

            throw;
        }
    }

    public async Task<bool> ExpireCheckoutSessionAsync(
        string stripeCheckoutSessionId,
        CancellationToken cancellationToken)
    {
        try
        {
            await new SessionService().ExpireAsync(
                stripeCheckoutSessionId, cancellationToken: cancellationToken);
            return true;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (StripeException exception)
        {
            _logger.LogWarning(
                exception,
                "Failed to expire Stripe checkout session {StripeCheckoutSessionId}.",
                stripeCheckoutSessionId);

            return false;
        }
    }

    private async Task<CreateCheckoutSessionResponse?> RestoreLegacySessionAsync(
        PaymentEntity payment,
        IReadOnlyCollection<ReservationEntity> reservations,
        CancellationToken cancellationToken)
    {
        Session session;
        try
        {
            session = await new SessionService().GetAsync(
                payment.StripeCheckoutSessionId, cancellationToken: cancellationToken);
        }
        catch (StripeException exception)
        {
            throw new MarketExternalServiceException(
                "stripe.checkout.verification_failed",
                "The payment session could not be verified. Please try again.",
                exception);
        }

        var sessionIsOpen = string.Equals(
            session.Status, "open", StringComparison.OrdinalIgnoreCase);
        var legacyAttempt = new PaymentAttemptEntity
        {
            PaymentId = payment.Id,
            IdempotencyKey = $"legacy-session-{session.Id}",
            StripeCheckoutSessionId = session.Id,
            CheckoutUrl = session.Url,
            Status = sessionIsOpen
                ? PaymentAttemptStatus.Pending
                : PaymentAttemptStatus.Expired,
            IsActive = sessionIsOpen
        };

        _context.PaymentAttempts.Add(legacyAttempt);
        await _context.SaveChangesAsync(cancellationToken);

        if (sessionIsOpen && !string.IsNullOrWhiteSpace(session.Url))
            return BuildResponse(payment, reservations, session.Id, session.Url);

        if (string.Equals(session.Status, "complete", StringComparison.OrdinalIgnoreCase))
            throw new MarketBusinessRuleException(
                 "PAYMENT_AWAITING_WEBHOOK",
                 "Payment is being processed. Please try again shortly.");

        payment.StripeCheckoutSessionId = null;
        payment.TransactionReference = null;
        return null;
    }

    private static CreateCheckoutSessionResponse BuildResponse(
        PaymentEntity payment,
        IReadOnlyCollection<ReservationEntity> reservations,
        string sessionId,
        string checkoutUrl) => new()
    {
        CheckoutUrl = checkoutUrl,
        SessionId = sessionId,
        PaymentId = payment.Id,
        ReservationIds = reservations.Select(x => x.Id).ToList()
    };

    private static string CreateDescription(IReadOnlyCollection<ReservationEntity> reservations)
    {
        if (reservations.Count == 1)
        {
            var reservation = reservations.First();
            var trackName = reservation.Track?.Name ?? "Track";
            var kartName = reservation.Kart?.Name ?? "Kart";
            return $"{trackName}, {kartName}, {reservation.Date:dd.MM.yyyy}, " +
                   $"{reservation.StartTime:HH:mm} - {reservation.EndTime:HH:mm}";
        }

        return $"{reservations.Count} Kart Arena reservations";
    }

    private async Task MarkCheckoutCreationAsFailedAsync(
    Session? createdSession,
    PaymentAttemptEntity attempt,
    PaymentEntity payment,
    IReadOnlyCollection<ReservationEntity> reservations,
    CancellationToken cancellationToken)
    {
        if (createdSession is not null)
        {
            try
            {
                await new SessionService().ExpireAsync(
                    createdSession.Id,
                    cancellationToken: cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (StripeException exception)
            {
                _logger.LogWarning(
                    exception,
                    "Failed to expire Stripe checkout session {StripeCheckoutSessionId} after checkout creation failure.",
                    createdSession.Id);

                // A late successful payment will be handled
                // and refunded by the webhook.
            }
        }

        attempt.Status = PaymentAttemptStatus.Failed;
        attempt.IsActive = false;

        payment.Status = PaymentStatus.Failed;
        payment.PaymentDate = null;
        payment.Note = "Stripe Checkout Session could not be created.";

        foreach (var reservation in reservations)
        {
            if (reservation.PaymentStatus == PaymentStatus.Paid)
            {
                continue;
            }

            reservation.PaymentStatus = PaymentStatus.Failed;
            reservation.Status = ReservationStatus.Cancelled;
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    private static bool IsUniqueConstraintViolation(
    DbUpdateException exception)
    {
        return exception.InnerException is SqlException sqlException &&
               sqlException.Number is 2601 or 2627;
    }
}
