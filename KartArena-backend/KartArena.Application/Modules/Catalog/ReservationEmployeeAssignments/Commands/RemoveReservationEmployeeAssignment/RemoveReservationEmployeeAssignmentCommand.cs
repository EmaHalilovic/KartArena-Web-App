using MediatR;

namespace KartArena.Application.Features.ReservationEmployeeAssignments.Commands.RemoveReservationEmployeeAssignment;

public class RemoveReservationEmployeeAssignmentCommand : IRequest
{
    public int Id { get; set; }
}
