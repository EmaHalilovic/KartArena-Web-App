using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace KartArena.Application.Modules.Catalog.Reservations.Commands.Create
{
    public sealed class CreateReservationCommandHandler(IAppDbContext ctx)
        : IRequestHandler<CreateReservationCommand, int>
    {
        public async Task<int> Handle(CreateReservationCommand request, CancellationToken ct)
        {
            // 1) FK checks
            var trackExists = await ctx.Tracks.AnyAsync(x => x.Id == request.TrackId, ct);
            if (!trackExists)
                throw new Exception("Track does not exist");

            var kartExists = await ctx.Karts.AnyAsync(x => x.Id == request.KartId, ct);
            if (!kartExists)
                throw new Exception("Kart does not exist");

            var userExists = await ctx.Users.AnyAsync(x => x.Id == request.UserId, ct);
            if (!userExists)
                throw new Exception("User does not exist");

            var date = request.ReservationDate.Date;

            // 2) Kart availability (overlapping time window)
            var kartBusy = await ctx.Reservations.AnyAsync(r =>
                !r.IsDeleted &&
                r.KartId == request.KartId &&
                r.Date.Date == date &&
                r.StartTime < request.EndTime &&
                r.EndTime > request.StartTime,
                ct);

            if (kartBusy)
                throw new Exception("Selected kart is not available for the chosen time.");

            // 3) Track capacity (max 6 drivers overlapping)
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

            // 4) Create reservation
            var reservation = new ReservationEntity
            {
                UserId = request.UserId,
                TrackId = request.TrackId,
                KartId = request.KartId,
                Date = date,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                IsDeleted = false
            };

            await ctx.Reservations.AddAsync(reservation, ct);
            await ctx.SaveChangesAsync(ct);

            return reservation.Id;
        }
    }
}
