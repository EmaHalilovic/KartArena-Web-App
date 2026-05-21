using KartArena.Application.Features.ReservationEmployeeAssignments.DTOs;
using KartArena.Domain.Entities.Equipment;
using KartArena.Domain.Entities.Reservations;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KartArena.Application.Features.ReservationEmployeeAssignments.Queries.GetAvailableEquipmentForReservation;

public class GetAvailableEquipmentForReservationQueryHandler : IRequestHandler<GetAvailableEquipmentForReservationQuery, List<AvailableEquipmentItemDto>>
{
    private readonly IAppDbContext _context;

    public GetAvailableEquipmentForReservationQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<List<AvailableEquipmentItemDto>> Handle(GetAvailableEquipmentForReservationQuery request, CancellationToken cancellationToken)
    {
        var reservation = await _context.Reservations
            .FirstOrDefaultAsync(x => x.Id == request.ReservationId && !x.IsDeleted, cancellationToken);

        if (reservation is null)
            throw new KeyNotFoundException("Reservation not found.");

        if (reservation.Status != ReservationStatus.Confirmed)
            throw new InvalidOperationException("Equipment availability can only be checked for confirmed reservations.");

        var blockedEquipmentIds = await _context.ReservationEmployees
            .Where(x =>
                !x.IsDeleted &&
                x.EquipmentItemId != null &&
                x.ReservationId != request.ReservationId &&
                x.Reservation != null &&
                !x.Reservation.IsDeleted &&
                x.Reservation.Status != ReservationStatus.Cancelled &&
                x.Reservation.Status != ReservationStatus.Completed &&
                reservation.StartTime < x.Reservation.EndTime &&
                reservation.EndTime > x.Reservation.StartTime)
            .Select(x => x.EquipmentItemId!.Value)
            .Distinct()
            .ToListAsync(cancellationToken);

        return await _context.EquipmentItemEntity
            .Where(x =>
                !x.IsDeleted &&
                x.Status == EquipmentItemStatus.Available &&
                x.EquipmentTypeId == request.EquipmentTypeId &&
                !blockedEquipmentIds.Contains(x.Id))
            .Select(x => new AvailableEquipmentItemDto
            {
                Id = x.Id,
                ItemCode = x.ItemCode,
                EquipmentTypeId = x.EquipmentTypeId,
                EquipmentTypeName = x.EquipmentType != null ? x.EquipmentType.Name : string.Empty
            })
            .OrderBy(x => x.ItemCode)
            .ToListAsync(cancellationToken);
    }
}
