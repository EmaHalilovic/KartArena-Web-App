using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KartArena.Application.Modules.Catalog.Reservations.Commands.Delete
{
    public sealed class DeleteReservationCommandHandler(IAppDbContext ctx)
        : IRequestHandler<DeleteReservationCommand>
    {
        public async Task Handle(DeleteReservationCommand request, CancellationToken ct)
        {
            var reservation = await ctx.Reservations
                .FirstOrDefaultAsync(r => r.Id == request.ReservationId && !r.IsDeleted, ct);

            if (reservation is null)
                throw new Exception("Reservation not found.");

            if (reservation.StartTime - DateTime.UtcNow < TimeSpan.FromHours(2))
            {
                throw new Exception(
                   "Reservation can only be deleted at least 2 hours before the start time.");
            }

            reservation.IsDeleted = true;
            await ctx.SaveChangesAsync(ct);
        }
    }
}