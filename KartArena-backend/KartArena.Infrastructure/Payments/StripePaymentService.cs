using KartArena.Application.Abstractions;
using KartArena.Application.Modules.Catalog.Payments.Stripe.Models;
using KartArena.Application.Modules.Catalog.Payments.Stripe.Models;
using KartArena.Domain.Entities.Payments;
using KartArena.Domain.Entities.Reservations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Stripe;
using Stripe.Checkout;

namespace KartArena.Infrastructure.Payments
{
    public sealed class StripePaymentService
        : IStripePaymentService
    {
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
                int reservationId,
                CancellationToken cancellationToken)
        {
            var reservation = await _context.Reservations
                .Include(x => x.Track)
                .Include(x => x.Kart)
                .Include(x => x.Payment)
                .SingleOrDefaultAsync(
                    x => x.Id == reservationId &&
                         !x.IsDeleted,
                    cancellationToken);

            if (reservation is null)
            {
                throw new KeyNotFoundException(
                    "Reservation was not found.");
            }

            if (reservation.Status ==
                ReservationStatus.Cancelled)
            {
                throw new InvalidOperationException(
                    "Cancelled reservation cannot be paid.");
            }

            if (reservation.PaymentStatus ==
                PaymentStatus.Paid)
            {
                throw new InvalidOperationException(
                    "Reservation is already paid.");
            }

            if (reservation.TotalPrice <= 0)
            {
                throw new InvalidOperationException(
                    "Reservation price must be greater than zero.");
            }

            var stripePaymentType =
                await _context.PaymentTypes
                    .SingleOrDefaultAsync(
                        x => x.Code == "STRIPE" &&
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
                    reservation.TotalPrice * 100m,
                    MidpointRounding.AwayFromZero));

            var options = new SessionCreateOptions
            {
                Mode = "payment",

                SuccessUrl =
                    $"{frontendUrl}/payment/success" +
                    "?session_id={{CHECKOUT_SESSION_ID}}",

                CancelUrl =
                    $"{frontendUrl}/payment/cancelled" +
                    $"?reservationId={reservation.Id}",

                ClientReferenceId =
                    reservation.Id.ToString(),

                CustomerEmail =
                    reservation.CustomerEmail,

                Metadata = new Dictionary<string, string>
                {
                    ["reservationId"] =
                        reservation.Id.ToString(),

                    ["customerEmail"] =
                        reservation.CustomerEmail
                },

                PaymentIntentData =
                    new SessionPaymentIntentDataOptions
                    {
                        Metadata =
                            new Dictionary<string, string>
                            {
                                ["reservationId"] =
                                    reservation.Id.ToString()
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
                                Currency = "eur",

                                UnitAmount =
                                    amountInMinorUnits,

                                ProductData =
                                    new SessionLineItemPriceDataProductDataOptions
                                    {
                                        Name =
                                            "Kart Arena reservation",

                                        Description =
                                            CreateDescription(
                                                reservation)
                                    }
                            }
                    }
                ]
            };

            var sessionService = new SessionService();

            var requestOptions = new RequestOptions
            {
                IdempotencyKey =
                    $"reservation-{reservation.Id}-checkout"
            };

            Session session;

            try
            {
                session = await sessionService.CreateAsync(
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

            if (string.IsNullOrWhiteSpace(session.Url))
            {
                throw new InvalidOperationException(
                    "Stripe did not return a checkout URL.");
            }

            if (reservation.Payment is null)
            {
                reservation.Payment =
                    new PaymentEntity
                    {
                        Amount =
                            reservation.TotalPrice,

                        Currency = "EUR",

                        Status =
                            PaymentStatus.Pending,

                        StripeCheckoutSessionId =
                            session.Id,

                        TransactionReference =
                            session.Id,

                        PaymentTypeId =
                            stripePaymentType.Id,

                        ReservationId =
                            reservation.Id,

                        Note =
                            "Stripe Checkout Session created."
                    };

                _context.Payments.Add(
                    reservation.Payment);
            }
            else
            {
                // Retrying the payment uses the same Payment record
                reservation.Payment.Amount =
                    reservation.TotalPrice;

                reservation.Payment.Currency =
                    "BAM";

                reservation.Payment.Status =
                    PaymentStatus.Pending;

                reservation.Payment.PaymentDate =
                    null;

                reservation.Payment.StripeCheckoutSessionId =
                    session.Id;

                reservation.Payment.StripePaymentIntentId =
                    null;

                reservation.Payment.StripeEventId =
                    null;

                reservation.Payment.TransactionReference =
                    session.Id;

                reservation.Payment.PaymentTypeId =
                    stripePaymentType.Id;

                reservation.Payment.Note =
                    "New Stripe Checkout Session created.";
            }

            reservation.PaymentStatus =
                PaymentStatus.Pending;

            await _context.SaveChangesAsync(
                cancellationToken);

            return new CreateCheckoutSessionResponse
            {
                CheckoutUrl = session.Url,
                SessionId = session.Id,
                ReservationId = reservation.Id
            };
        }

        private static string CreateDescription(
            ReservationEntity reservation)
        {
            var trackName =
                reservation.Track?.Name ?? "Track";

            var kartName =
                reservation.Kart?.Name ?? "Kart";

            return
                $"{trackName}, {kartName}, " +
                $"{reservation.Date:dd.MM.yyyy}, " +
                $"{reservation.StartTime:HH:mm} - " +
                $"{reservation.EndTime:HH:mm}";
        }
    }
}