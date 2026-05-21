using KartArena.Application.Features.ReservationEmployeeAssignments.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KartArena.Application.Features.ReservationEmployeeAssignments.Queries.GetReservationAssignmentsByReservationId;

public class GetReservationAssignmentsByReservationIdQueryHandler : IRequestHandler<GetReservationAssignmentsByReservationIdQuery, List<ReservationEmployeeAssignmentDto>>
{
    private readonly IAppDbContext _context;

    public GetReservationAssignmentsByReservationIdQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<List<ReservationEmployeeAssignmentDto>> Handle(GetReservationAssignmentsByReservationIdQuery request, CancellationToken cancellationToken)
    {
        return await _context.ReservationEmployees
            .Where(x => x.ReservationId == request.ReservationId && !x.IsDeleted)
            .Select(x => new ReservationEmployeeAssignmentDto
            {
                Id = x.Id,
                ReservationId = x.ReservationId,
                EmployeeId = x.EmployeeId,
                EmployeeName = x.Employee != null
                    ? ((x.Employee.FirstName ?? string.Empty) + " " + (x.Employee.LastName ?? string.Empty)).Trim()
                    : string.Empty,
                EquipmentItemId = x.EquipmentItemId,
                EquipmentItemName = x.EquipmentItem != null ? x.EquipmentItem.ItemCode : null,
                EquipmentTypeId = x.EquipmentItem != null ? x.EquipmentItem.EquipmentTypeId : null,
                EquipmentCategoryName = x.EquipmentItem != null && x.EquipmentItem.EquipmentType != null
                    ? x.EquipmentItem.EquipmentType.Name
                    : null,
                CreatedAtUtc = x.CreatedAtUtc
            })
            .OrderBy(x => x.EquipmentCategoryName)
            .ThenBy(x => x.EquipmentItemName)
            .ToListAsync(cancellationToken);
    }
}
