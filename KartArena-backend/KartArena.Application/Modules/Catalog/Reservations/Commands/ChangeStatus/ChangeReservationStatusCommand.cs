using KartArena.Domain.Entities.Reservations;

namespace KartArena.Application.Modules.Catalog.Reservations.Commands.ChangeStatus;

public sealed class ChangeReservationStatusCommand : IRequest<int>
{
    public int ReservationId { get; set; }
    public ReservationStatus Status { get; set; }
}
