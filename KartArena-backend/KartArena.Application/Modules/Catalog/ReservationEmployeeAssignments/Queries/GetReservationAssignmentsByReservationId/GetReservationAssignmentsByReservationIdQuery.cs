using KartArena.Application.Features.ReservationEmployeeAssignments.DTOs;
using MediatR;

namespace KartArena.Application.Features.ReservationEmployeeAssignments.Queries.GetReservationAssignmentsByReservationId;

public class GetReservationAssignmentsByReservationIdQuery : IRequest<List<ReservationEmployeeAssignmentDto>>
{
    public int ReservationId { get; set; }
}
