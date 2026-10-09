using KartArena.Application.Features.ReservationEmployeeAssignments.Commands.RemoveReservationEmployeeAssignment;
using KartArena.Application.Modules.Catalog.Equipment.Commands.Delete;

namespace KartArena.Application.Modules.Catalog.Equipment.Commands.Update;

public sealed class RemoveReservationEmployeeAssignmentCommandValidator : AbstractValidator<RemoveReservationEmployeeAssignmentCommand>
{
    public RemoveReservationEmployeeAssignmentCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);

        
    }
}
