using MediatR;

namespace KartArena.Application.Features.ReservationEmployeeAssignments.Commands.AssignReservationResources;

public class AssignReservationResourcesCommand : IRequest<int>
{
    public int ReservationId { get; set; }

    // Manual assignment for now; auth-based current-user resolution is intentionally not used here.
    public int? EmployeeId { get; set; }
    public string? EmployeeName { get; set; }

    public List<int> EquipmentItemIds { get; set; } = new();

    public bool ReplaceExistingAssignments { get; set; } = true;
}
