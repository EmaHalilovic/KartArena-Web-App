using KartArena.Domain.Entities.Payments;
using KartArena.Domain.Entities.Reservations;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KartArena.Application.Modules.Catalog.Reservations.Commands.Create;

public sealed class CreateReservationCommandHandler(IAppDbContext ctx)
    : IRequestHandler<CreateReservationCommand, int>
{
    private const int MaxDriversPerTrackSlot = 6;
    private const string DeskCashPaymentCode = "DESK_CASH";
    private const string DefaultCurrency = "bam";

    public async Task<int> Handle(
        CreateReservationCommand request,
        CancellationToken ct)
    {
        var track = await ctx.Tracks
            .FirstOrDefaultAsync(
                x => x.Id == request.TrackId &&
                     !x.IsDeleted,
                ct);

        if (track is null)
        {
            throw new InvalidOperationException(
                "Track does not exist.");
        }

        var kart = await ctx.Karts
            .FirstOrDefaultAsync(
                x => x.Id == request.KartId &&
                     !x.IsDeleted,
                ct);

        if (kart is null)
        {
            throw new InvalidOperationException(
                "Kart does not exist.");
        }

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

        PaymentTypeEntity? paymentType = null;

        if (request.PaymentTypeId.HasValue)
        {
            paymentType = await ctx.PaymentTypes
                .FirstOrDefaultAsync(
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
        }

        var isDeskCashPayment = string.Equals(
 paymentType.Code,
 DeskCashPaymentCode,
 StringComparison.OrdinalIgnoreCase);

        var date = request.ReservationDate.Date;

        if (request.EndTime <= request.StartTime)
        {
            throw new InvalidOperationException(
                "Reservation end time must be after the start time.");
        }

        if (date != request.StartTime.Date ||
            date != request.EndTime.Date)
        {
            throw new InvalidOperationException(
                "Reservation date, start time and end time must be on the same day.");
        }

        var durationMinutes =
            (request.EndTime - request.StartTime).TotalMinutes;

        var totalPrice = (track.Outdoors, durationMinutes) switch
        {
            (false, 10) => 15m,
            (false, 15) => 20m,

            (true, 10) => 20m,
            (true, 15) => 25m,

            _ => throw new InvalidOperationException(
                "Only 10-minute and 15-minute sessions are allowed.")
        };

        var kartBusy = await ctx.Reservations.AnyAsync(
            reservation =>
                !reservation.IsDeleted &&
                reservation.KartId == request.KartId &&
                reservation.Date.Date == date &&
                reservation.StartTime < request.EndTime &&
                reservation.EndTime > request.StartTime &&
                reservation.Status != ReservationStatus.Cancelled,
            ct);

        if (kartBusy)
        {
            throw new InvalidOperationException(
                "Selected kart is not available for the chosen time.");
        }

        var trackReservationsCount = await ctx.Reservations
            .Where(reservation =>
                !reservation.IsDeleted &&
                reservation.TrackId == request.TrackId &&
                reservation.Date.Date == date &&
                reservation.StartTime < request.EndTime &&
                reservation.EndTime > request.StartTime &&
                reservation.Status != ReservationStatus.Cancelled)
            .CountAsync(ct);

        if (trackReservationsCount >= MaxDriversPerTrackSlot)
        {
            throw new InvalidOperationException(
                "Track is not available. Maximum 6 drivers are allowed for this time slot.");
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


            TrackId = request.TrackId,
            KartId = request.KartId,

            Date = date,
            StartTime = request.StartTime,
            EndTime = request.EndTime,

            Status = isDeskCashPayment?ReservationStatus.Confirmed:ReservationStatus.Pending,
            PaymentStatus = PaymentStatus.Pending,

            TotalPrice = totalPrice,
            IsDeleted = false
        };

        await ctx.Reservations.AddAsync(
            reservation,
            ct);

        if (paymentType is not null)
        {
            var payment = new PaymentEntity
            {
                PaymentTypeId = paymentType.Id,

                Amount = totalPrice,
                Currency = DefaultCurrency,

                Status = PaymentStatus.Pending,
                PaymentDate = null,

                Note = string.IsNullOrWhiteSpace(
                    request.PaymentNote)
                    ? null
                    : request.PaymentNote.Trim(),

                IsDeleted = false,
                isEnabled = true
            };

            var paymentReservation =
                new PaymentReservationEntity
                {
                    Payment = payment,
                    Reservation = reservation
                };

            await ctx.Payments.AddAsync(
                payment,
                ct);

            await ctx.PaymentReservations.AddAsync(
                paymentReservation,
                ct);
        }

        await ctx.SaveChangesAsync(ct);

        return reservation.Id;
    }
}