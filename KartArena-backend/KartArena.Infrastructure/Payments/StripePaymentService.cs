using KartArena.Application.Abstractions;
using KartArena.Application.Modules.Catalog.Payments.Stripe.Models;
using KartArena.Domain.Entities.Payments;
using KartArena.Domain.Entities.Reservations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Stripe;
using Stripe.Checkout;

namespace KartArena.Infrastructure.Payments;

public sealed class StripePaymentService : IStripePaymentService
{
    private const string Currency = "bam";

    private readonly IAppDbContext _context;
    private readonly IConfiguration _configuration;

    public StripePaymentService(
        IAppDbContext context,
        IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<CreateCheckoutSessionResponse>
        CreateCheckoutSessionAsync(
            int paymentId,
            CancellationToken cancellationToken)
    {
        var payment = await _context.Payments
            .Include(x => x.PaymentType)
            .Include(x => x.PaymentReservations)
                .ThenInclude(x => x.Reservation)
                    .ThenInclude(x => x.Track)
            .Include(x => x.PaymentReservations)
                .ThenInclude(x => x.Reservation)
                    .ThenInclude(x => x.Kart)
            .SingleOrDefaultAsync(
                x => x.Id == paymentId &&
                     !x.IsDeleted,
                cancellationToken);

        if (payment is null)
        {
            throw new KeyNotFoundException(
                "Payment was not found.");
        }

        if (payment.PaymentReservations.Count == 0)
        {
            throw new InvalidOperationException(
                "Payment does not contain reservations.");
        }

        if (payment.Status == PaymentStatus.Paid)
        {
            throw new InvalidOperationException(
                "Payment is already completed.");
        }

        var reservations = payment.PaymentReservations
            .Select(x => x.Reservation)
            .ToList();

        if (reservations.Any(x =>
                x.Status == ReservationStatus.Cancelled))
        {
            throw new InvalidOperationException(
                "Cancelled reservations cannot be paid.");
        }

        if (reservations.Any(x =>
                x.PaymentStatus == PaymentStatus.Paid))
        {
            throw new InvalidOperationException(
                "One or more reservations are already paid.");
        }

        if (payment.Amount <= 0)
        {
            throw new InvalidOperationException(
                "Payment amount must be greater than zero.");
        }

        var stripePaymentType = await _context.PaymentTypes
            .SingleOrDefaultAsync(
                x => x.Code == "STRIPE" &&
                     x.AllowedOnline &&
                     !x.IsDeleted,
                cancellationToken);

        if (stripePaymentType is null)
        {
            throw new InvalidOperationException(
                "Stripe payment type is not configured.");
        }

        var frontendUrl =
            _configuration["FrontendUrl"];

        if (string.IsNullOrWhiteSpace(frontendUrl))
        {
            throw new InvalidOperationException(
                "Frontend URL is not configured.");
        }

        var amountInMinorUnits = checked(
            (long)Math.Round(
                payment.Amount * 100m,
                MidpointRounding.AwayFromZero));

        var reservationIds = reservations
            .Select(x => x.Id)
            .ToList();

        var options = new SessionCreateOptions
        {
            Mode = "payment",

            SuccessUrl =
                $"{frontendUrl}/payment/success" +
                "?session_id={{CHECKOUT_SESSION_ID}}",

            CancelUrl =
                $"{frontendUrl}/payment/cancelled" +
                $"?paymentId={payment.Id}",

            ClientReferenceId =
                payment.Id.ToString(),

            CustomerEmail =
                reservations
                    .Select(x => x.CustomerEmail)
                    .FirstOrDefault(x =>
                        !string.IsNullOrWhiteSpace(x)),

            Metadata = new Dictionary<string, string>
            {
                ["paymentId"] =
                    payment.Id.ToString(),

                ["reservationIds"] =
                    string.Join(",", reservationIds)
            },

            PaymentIntentData =
                new SessionPaymentIntentDataOptions
                {
                    Metadata =
                        new Dictionary<string, string>
                        {
                            ["paymentId"] =
                                payment.Id.ToString()
                        }
                },

            LineItems =
            [
                new SessionLineItemOptions
                {
                    Quantity = 1,

                    PriceData =
                        new SessionLineItemPriceDataOptions
                        {
                            Currency = Currency,

                            UnitAmount =
                                amountInMinorUnits,

                            ProductData =
                                new SessionLineItemPriceDataProductDataOptions
                                {
                                    Name =
                                        "Kart Arena reservation checkout",

                                    Description =
                                        CreateDescription(
                                            reservations)
                                }
                        }
                }
            ]
        };

        var sessionService =
            new SessionService();

        /*
         * Ako isti payment može imati više pokušaja,
         * nemoj koristiti uvijek isti idempotency key.
         *
         * Payment ID + trenutni broj session pokušaja
         * ili posebni attempt ID je bolja opcija.
         */
        var requestOptions =
            new RequestOptions
            {
                IdempotencyKey =
                    $"payment-{payment.Id}-checkout-" +
                    $"{Guid.NewGuid():N}"
            };

        Session session;

        try
        {
            session =
                await sessionService.CreateAsync(
                    options,
                    requestOptions,
                    cancellationToken);
        }
        catch (StripeException exception)
        {
            throw new InvalidOperationException(
                "Stripe Checkout Session could not be created.",
                exception);
        }

        if (string.IsNullOrWhiteSpace(
                session.Url))
        {
            throw new InvalidOperationException(
                "Stripe did not return a checkout URL.");
        }

        payment.Amount =
            payment.Amount;

        payment.Currency =
            Currency;

        payment.Status =
            PaymentStatus.Pending;

        payment.PaymentDate =
            null;

        payment.StripeCheckoutSessionId =
            session.Id;

        payment.StripePaymentIntentId =
            null;

        payment.StripeEventId =
            null;

        payment.TransactionReference =
            session.Id;

        payment.PaymentTypeId =
            stripePaymentType.Id;

        payment.Note =
            "Stripe Checkout Session created.";

        payment.ModifiedAtUtc =
            DateTime.UtcNow;

        foreach (var reservation in reservations)
        {
            reservation.PaymentStatus =
                PaymentStatus.Pending;

            reservation.ModifiedAtUtc =
                DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(
            cancellationToken);

        return new CreateCheckoutSessionResponse
        {
            CheckoutUrl = session.Url,
            SessionId = session.Id,
            PaymentId = payment.Id,
            ReservationIds = reservationIds
        };
    }

    private static string CreateDescription(
        IReadOnlyCollection<ReservationEntity> reservations)
    {
        if (reservations.Count == 1)
        {
            var reservation =
                reservations.First();

            var trackName =
                reservation.Track?.Name ??
                "Track";

            var kartName =
                reservation.Kart?.Name ??
                "Kart";

            return
                $"{trackName}, {kartName}, " +
                $"{reservation.Date:dd.MM.yyyy}, " +
                $"{reservation.StartTime:HH:mm} - " +
                $"{reservation.EndTime:HH:mm}";
        }

        return
            $"{reservations.Count} Kart Arena reservations";
    }
}