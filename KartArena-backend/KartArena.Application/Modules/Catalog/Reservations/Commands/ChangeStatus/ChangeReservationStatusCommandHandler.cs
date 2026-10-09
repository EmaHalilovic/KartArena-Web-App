using KartArena.Domain.Entities.Reservations;
using Microsoft.EntityFrameworkCore;

namespace KartArena.Application.Modules.Catalog.Reservations.Commands.ChangeStatus;

public sealed class ChangeReservationStatusCommandHandler(
    IAppDbContext context,
    IBusinessClock businessClock)
    : IRequestHandler<ChangeReservationStatusCommand, int>
{
    public async Task<int> Handle(ChangeReservationStatusCommand request, CancellationToken cancellationToken)
    {
        var reservation = await context.Reservations
            .FirstOrDefaultAsync(x => x.Id == request.ReservationId && !x.IsDeleted, cancellationToken);

        if (reservation is null)
            throw new MarketNotFoundException("Reservation not found.");

        if (reservation.Status != ReservationStatus.Confirmed)
            throw new MarketBusinessRuleException(
                "RESERVATION_NOT_CONFIRMED",
                "Only confirmed reservations can be completed or cancelled.");

        if (request.Status is not ReservationStatus.Completed and not ReservationStatus.Cancelled)
            throw new MarketBusinessRuleException(
                "INVALID_RESERVATION_STATUS",
                "The new status must be Completed or Cancelled.");

        if (reservation.Date.Date != businessClock.Today)
            throw new MarketBusinessRuleException(
                "INVALID_RESERVATION_DATE",
                "A reservation can only be completed or cancelled on its reservation date.");

        reservation.Status = request.Status;
        await context.SaveChangesAsync(cancellationToken);
        return reservation.Id;
    }
}
