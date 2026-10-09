using KartArena.Application.Modules.Catalog.Equipment.Commands.Delete;
using KartArena.Application.Modules.Catalog.Reservations.Commands.Delete;

namespace KartArena.Application.Modules.Catalog.Equipment.Commands.Update;

public sealed class DeleteReservationCommandValidator : AbstractValidator<DeleteReservationCommand>
{
    public DeleteReservationCommandValidator()
    {
        RuleFor(x => x.ReservationId).GreaterThan(0);

        
    }
}
