using KartArena.Domain.Entities.Payments;
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
            var trackExists = await ctx.Tracks.AnyAsync(x => x.Id == request.TrackId, ct);
            if (!trackExists)
                throw new Exception("Track does not exist.");

            var kartExists = await ctx.Karts.AnyAsync(x => x.Id == request.KartId, ct);
            if (!kartExists)
                throw new Exception("Kart does not exist.");

            var userExists = await ctx.Users.AnyAsync(x => x.Id == request.UserId, ct);
            if (!userExists)
                throw new Exception("User does not exist.");

            var paymentType = await ctx.PaymentTypes
                .FirstOrDefaultAsync(x => x.Id == request.PaymentTypeId, ct);

            if (paymentType is null)
                throw new Exception("Selected payment type does not exist.");

            if (!paymentType.isEnabled)
                throw new Exception("Selected payment type is not active.");

            if (!paymentType.AllowedOnline)
                throw new Exception("Selected payment type is not available for online payment.");

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
                IsDeleted = false
            };

            var payment = new PaymentEntity
            {
                Amount = request.Amount,
                PaymentTypeId = request.PaymentTypeId,
                Status = PaymentStatus.Pending,
                PaymentDate = null,
                Note = request.PaymentNote,
                Reservation = reservation
            };

            reservation.Payment = payment;

            await ctx.Reservations.AddAsync(reservation, ct);
            await ctx.SaveChangesAsync(ct);

            return reservation.Id;
        }
    }
}