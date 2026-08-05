using KartArena.Domain.Entities.Payments;
using KartArena.Domain.Entities.Catalog;
using KartArena.Domain.Entities.Reservations;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KartArena.Application.Modules.Catalog.Reservations.Commands.Create
{
    public sealed class CreateReservationCommandHandler(IAppDbContext ctx)
        : IRequestHandler<CreateReservationCommand, int>
    {
        public async Task<int> Handle(CreateReservationCommand request, CancellationToken ct)
        {
            var track = await ctx.Tracks
    .FirstOrDefaultAsync(x => x.Id == request.TrackId, ct);

            if (track is null)
                throw new Exception("Track does not exist.");

            var kart = await ctx.Karts
     .FirstOrDefaultAsync(x => x.Id == request.KartId, ct);

            if (kart is null)
                throw new Exception("Kart does not exist.");

            if (request.UserId.HasValue)
            {
                var userExists = await ctx.Users.AnyAsync(x => x.Id == request.UserId.Value, ct);
                if (!userExists)
                    throw new Exception("User does not exist.");
            }

            PaymentTypeEntity? paymentType = null;

            if (request.PaymentTypeId.HasValue)
            {
                paymentType = await ctx.PaymentTypes
                    .FirstOrDefaultAsync(x => x.Id == request.PaymentTypeId.Value, ct);

                if (paymentType is null)
                    throw new Exception("Selected payment type does not exist.");

                if (!paymentType.isEnabled)
                    throw new Exception("Selected payment type is not active.");

                if (!paymentType.AllowedOnline)
                    throw new Exception("Selected payment type is not available for online payment.");
            }

            var date = request.ReservationDate.Date;

            var kartBusy = await ctx.Reservations.AnyAsync(r =>
                !r.IsDeleted &&
                r.KartId == request.KartId &&
                r.Date.Date == date &&
                r.StartTime < request.EndTime &&
                r.EndTime > request.StartTime,
                ct);

            if (kartBusy)
                throw new Exception("Selected kart is not available for the chosen time.");

            var trackReservationsCount = await ctx.Reservations
                .Where(r =>
                    !r.IsDeleted &&
                    r.TrackId == request.TrackId &&
                    r.Date.Date == date &&
                    r.StartTime < request.EndTime &&
                    r.EndTime > request.StartTime)
                .CountAsync(ct);

            if (trackReservationsCount >= 6)
                throw new Exception("Track is not available (maximum 6 drivers for this time slot).");

            if (request.EndTime <= request.StartTime)
                throw new Exception(
                    "Reservation end time must be after the start time.");

            var durationMinutes =
                (request.EndTime - request.StartTime).TotalMinutes;

            decimal totalPrice = (track.Outdoors, durationMinutes) switch
            {
                (false, 10) => 15m,
                (false, 15) => 20m,

                (true, 10) => 20m,
                (true, 15) => 25m,

                _ => throw new Exception(
                    "Only 10-minute and 15-minute sessions are allowed.")
            };


            var reservation = new ReservationEntity
            {
                UserId = request.UserId,
                TrackId = request.TrackId,
                KartId = request.KartId,
                Date = date,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                Status = ReservationStatus.Pending,
                PaymentStatus = PaymentStatus.Pending,
                IsDeleted = false,
                TotalPrice = totalPrice,
            };

            if (paymentType is not null)
            {
                var payment = new PaymentEntity
                {
                    PaymentTypeId = request.PaymentTypeId,
                    Status = PaymentStatus.Pending,
                    PaymentDate = null,
                    Note = request.PaymentNote,
                    Reservation = reservation
                };

                reservation.Payment = payment;
            }

            await ctx.Reservations.AddAsync(reservation, ct);
            await ctx.SaveChangesAsync(ct);

            return reservation.Id;
        }
    }
}
