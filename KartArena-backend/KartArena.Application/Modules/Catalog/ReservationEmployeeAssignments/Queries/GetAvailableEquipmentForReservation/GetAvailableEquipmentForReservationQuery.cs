using KartArena.Application.Features.ReservationEmployeeAssignments.DTOs;
using MediatR;

namespace KartArena.Application.Features.ReservationEmployeeAssignments.Queries.GetAvailableEquipmentForReservation;

public class GetAvailableEquipmentForReservationQuery : IRequest<List<AvailableEquipmentItemDto>>
{
    public int ReservationId { get; set; }
    public int EquipmentTypeId { get; set; }
}
