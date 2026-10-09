using KartArena.Domain.Entities.Reservations;
using MediatR;
using Microsoft.EntityFrameworkCore;

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
                throw new MarketNotFoundException("Reservation not found.");

            if (reservation.Status is not ReservationStatus.Completed and not ReservationStatus.Cancelled)
                throw new MarketBusinessRuleException(
                    "RESERVATION_NOT_COMPLETED_OR_CANCELLED",
                    "Only completed or cancelled reservations can be deleted.");

            reservation.IsDeleted = true;
            await ctx.SaveChangesAsync(ct);
        }
    }
}
