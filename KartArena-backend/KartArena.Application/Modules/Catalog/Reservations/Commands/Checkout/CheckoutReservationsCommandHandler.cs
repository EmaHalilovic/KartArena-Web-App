using KartArena.Application.Abstractions;
using KartArena.Application.Modules.Catalog.Payments.Stripe;
using KartArena.Domain.Entities.Payments;
using KartArena.Domain.Entities.Reservations;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KartArena.Application.Modules.Catalog.Reservations.Commands.Checkout;

public sealed class CheckoutReservationsCommandHandler(
    IAppDbContext ctx,
    IStripePaymentService stripePaymentService)
    : IRequestHandler<
        CheckoutReservationsCommand,
        CheckoutReservationsResponse>
{
    private const int MaxDriversPerTrackSlot = 6;
    private const string StripePaymentCode = "STRIPE";
    private const string DeskCashPaymentCode = "DESK_CASH";
    private const string DefaultCurrency = "bam";

    public async Task<CheckoutReservationsResponse> Handle(
        CheckoutReservationsCommand request,
        CancellationToken ct)
    {
        if (request.Items.Count == 0)
        {
            throw new InvalidOperationException(
                "Cart is empty.");
        }

        ValidateCustomerInfo(request);

        if (request.UserId.HasValue)
        {
            var userExists = await ctx.Users.AnyAsync(
                x => x.Id == request.UserId.Value &&
                     !x.IsDeleted,
                ct);

            if (!userExists)
            {
                throw new InvalidOperationException(
                    "User does not exist.");
            }
        }

        if (!request.PaymentTypeId.HasValue)
        {
            throw new InvalidOperationException(
                "Payment type is required.");
        }

        var paymentType = await ctx.PaymentTypes
            .SingleOrDefaultAsync(
                x => x.Id == request.PaymentTypeId.Value &&
                     !x.IsDeleted,
                ct);

        if (paymentType is null)
        {
            throw new InvalidOperationException(
                "Selected payment type does not exist.");
        }

        if (!paymentType.isEnabled)
        {
            throw new InvalidOperationException(
                "Selected payment type is not active.");
        }

        if (!paymentType.AllowedOnline)
        {
            throw new InvalidOperationException(
                "Selected payment type is not available for online payment.");
        }

        var isDeskCashPayment = string.Equals(
    paymentType.Code,
    DeskCashPaymentCode,
    StringComparison.OrdinalIgnoreCase);

        var kartIds = request.Items
            .Select(x => x.KartId)
            .Distinct()
            .ToList();

        var selectedKarts = await ctx.Karts
            .Where(x =>
                kartIds.Contains(x.Id) &&
                !x.IsDeleted)
            .ToListAsync(ct);

        if (selectedKarts.Count != kartIds.Count)
        {
            throw new InvalidOperationException(
                "One or more selected karts do not exist.");
        }

        

        var calculatedTotalAmount = request.Items.Sum(item =>
        {
            var kart = selectedKarts
                .Single(x => x.Id == item.KartId);

            if (!kart.PricePerSession.HasValue)
            {
                throw new InvalidOperationException(
                    $"Kart {kart.Name ?? kart.Id.ToString()} " +
                    "does not have a price.");
            }

            return kart.PricePerSession.Value;
        });

        if (request.Amount.HasValue &&
            request.Amount.Value != calculatedTotalAmount)
        {
            throw new InvalidOperationException(
                "Submitted amount does not match calculated amount.");
        }

        await using var transaction =
            await ctx.Database.BeginTransactionAsync(ct);

        try
        {
            var reservationsToCreate =
                new List<ReservationEntity>();

            foreach (var item in request.Items)
            {
                ValidateReservationItem(item);

                var date = item.ReservationDate.Date;

                var trackExists = await ctx.Tracks.AnyAsync(
                    x => x.Id == item.TrackId &&
                         !x.IsDeleted,
                    ct);

                if (!trackExists)
                {
                    throw new InvalidOperationException(
                        $"Track with ID {item.TrackId} does not exist.");
                }

                var kart = selectedKarts
                    .Single(x => x.Id == item.KartId);

                var itemAmount = kart.PricePerSession!.Value;

                var kartBusyInDatabase =
                    await ctx.Reservations.AnyAsync(
                        reservation =>
                            !reservation.IsDeleted &&
                            reservation.KartId == item.KartId &&
                            reservation.Date.Date == date &&
                            reservation.StartTime < item.EndTime &&
                            reservation.EndTime > item.StartTime &&
                            reservation.Status !=
                                ReservationStatus.Cancelled,
                        ct);

                if (kartBusyInDatabase)
                {
                    throw new InvalidOperationException(
                        $"Kart {item.KartId} is not available " +
                        "for the chosen time.");
                }

                var kartBusyInsideCurrentCart =
                    reservationsToCreate.Any(
                        reservation =>
                            reservation.KartId == item.KartId &&
                            reservation.Date.Date == date &&
                            reservation.StartTime < item.EndTime &&
                            reservation.EndTime > item.StartTime);

                if (kartBusyInsideCurrentCart)
                {
                    throw new InvalidOperationException(
                        $"Kart {item.KartId} is already selected " +
                        "in this cart for the same time.");
                }

                var existingTrackReservationsCount =
                    await ctx.Reservations
                        .Where(reservation =>
                            !reservation.IsDeleted &&
                            reservation.TrackId == item.TrackId &&
                            reservation.Date.Date == date &&
                            reservation.StartTime < item.EndTime &&
                            reservation.EndTime > item.StartTime &&
                            reservation.Status !=
                                ReservationStatus.Cancelled)
                        .CountAsync(ct);

                var cartTrackReservationsCount =
                    reservationsToCreate.Count(
                        reservation =>
                            reservation.TrackId == item.TrackId &&
                            reservation.Date.Date == date &&
                            reservation.StartTime < item.EndTime &&
                            reservation.EndTime > item.StartTime);

                if (existingTrackReservationsCount +
                    cartTrackReservationsCount >=
                    MaxDriversPerTrackSlot)
                {
                    throw new InvalidOperationException(
                        "Track is not available. Maximum 6 drivers " +
                        "are allowed for this time slot.");
                }

                var reservation = new ReservationEntity
                {
                    UserId = request.UserId,

                    CustomerFirstName =
                        request.CustomerFirstName.Trim(),

                    CustomerLastName =
                        request.CustomerLastName.Trim(),

                    CustomerEmail =
                        request.CustomerEmail.Trim(),

                    CustomerPhone =
                        request.CustomerPhone.Trim(),

                    CustomerNote =
                        string.IsNullOrWhiteSpace(
                            request.CustomerNote)
                            ? null
                            : request.CustomerNote.Trim(),

                    TrackId = item.TrackId,
                    KartId = item.KartId,

                    Date = date,
                    StartTime = item.StartTime,
                    EndTime = item.EndTime,

                    TotalPrice = itemAmount,


                    Status = isDeskCashPayment
                        ? ReservationStatus.Confirmed
                        : ReservationStatus.Pending,
                    PaymentStatus = PaymentStatus.Pending,

                    IsDeleted = false
                };

                reservationsToCreate.Add(reservation);
            }

            await ctx.Reservations.AddRangeAsync(
                reservationsToCreate,
                ct);

            var payment = new PaymentEntity
            {
                Amount = calculatedTotalAmount,
                Currency = DefaultCurrency,

                Status = PaymentStatus.Pending,
                PaymentDate = null,

                PaymentTypeId = paymentType.Id,

                Note = string.IsNullOrWhiteSpace(
                    request.PaymentNote)
                    ? null
                    : request.PaymentNote.Trim(),

                IsDeleted = false
            };

            await ctx.Payments.AddAsync(payment, ct);

            
            foreach (var reservation in reservationsToCreate)
            {
                var paymentReservation =
                    new PaymentReservationEntity
                    {
                        Payment = payment,
                        Reservation = reservation
                    };

                ctx.PaymentReservations.Add(
                    paymentReservation);
            }

            await ctx.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

         
            if (string.Equals(
                    paymentType.Code,
                    StripePaymentCode,
                    StringComparison.OrdinalIgnoreCase))
            {
                var stripeResponse =
                    await stripePaymentService
                        .CreateCheckoutSessionAsync(
                            payment.Id,
                            ct);

                return new CheckoutReservationsResponse
                {
                    ReservationIds = reservationsToCreate
                        .Select(x => x.Id)
                        .ToList(),

                    PaymentId = payment.Id,

                    PaymentStatus =
                        payment.Status.ToString(),

                    CheckoutUrl =
                        stripeResponse.CheckoutUrl,

                    StripeCheckoutSessionId =
                        stripeResponse.SessionId
                };
            }

          
            return new CheckoutReservationsResponse
            {
                ReservationIds = reservationsToCreate
                    .Select(x => x.Id)
                    .ToList(),

                PaymentId = payment.Id,

                PaymentStatus =
                    payment.Status.ToString(),

                CheckoutUrl = null,

                StripeCheckoutSessionId = null
            };
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }

    private static void ValidateCustomerInfo(
        CheckoutReservationsCommand request)
    {
        if (string.IsNullOrWhiteSpace(
                request.CustomerFirstName))
        {
            throw new InvalidOperationException(
                "First name is required.");
        }

        if (string.IsNullOrWhiteSpace(
                request.CustomerLastName))
        {
            throw new InvalidOperationException(
                "Last name is required.");
        }

        if (string.IsNullOrWhiteSpace(
                request.CustomerEmail))
        {
            throw new InvalidOperationException(
                "Email is required.");
        }

        if (string.IsNullOrWhiteSpace(
                request.CustomerPhone))
        {
            throw new InvalidOperationException(
                "Phone number is required.");
        }
    }

    private static void ValidateReservationItem(
        CheckoutReservationItemDto item)
    {
        if (item.TrackId <= 0)
        {
            throw new InvalidOperationException(
                "Track is required.");
        }

        if (item.KartId <= 0)
        {
            throw new InvalidOperationException(
                "Kart is required.");
        }

        if (item.StartTime >= item.EndTime)
        {
            throw new InvalidOperationException(
                "End time must be after start time.");
        }

        if (item.ReservationDate.Date !=
                item.StartTime.Date ||
            item.ReservationDate.Date !=
                item.EndTime.Date)
        {
            throw new InvalidOperationException(
                "Reservation date, start time and end time " +
                "must be on the same day.");
        }

        if (item.StartTime <= DateTime.UtcNow)
        {
            throw new InvalidOperationException(
                "Reservation time must be in the future.");
        }
    }
}