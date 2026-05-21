using FluentValidation;

namespace KartArena.Application.Features.ReservationEmployeeAssignments.Commands.AssignReservationResources;

public class AssignReservationResourcesCommandValidator : AbstractValidator<AssignReservationResourcesCommand>
{
    public AssignReservationResourcesCommandValidator()
    {
        RuleFor(x => x.ReservationId)
            .GreaterThan(0);

        RuleFor(x => x)
            .Must(x => x.EmployeeId.HasValue || !string.IsNullOrWhiteSpace(x.EmployeeName))
            .WithMessage("An employee must be provided.");

        RuleForEach(x => x.EquipmentItemIds)
            .GreaterThan(0);

        RuleFor(x => x.EquipmentItemIds)
            .Must(x => x.Distinct().Count() == x.Count)
            .WithMessage("Duplicate equipment items are not allowed.");
    }
}
