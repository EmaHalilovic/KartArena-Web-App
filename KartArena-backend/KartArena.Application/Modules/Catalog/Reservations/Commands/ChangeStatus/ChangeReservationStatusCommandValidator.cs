using KartArena.Domain.Entities.Reservations;

namespace KartArena.Application.Modules.Catalog.Reservations.Commands.ChangeStatus;
public sealed class ChangeReservationStatusCommandValidator
    : AbstractValidator<ChangeReservationStatusCommand>
{
    public ChangeReservationStatusCommandValidator()
    {
        RuleFor(x => x.ReservationId).GreaterThan(0);
        RuleFor(x => x.Status).IsInEnum();
    }
}