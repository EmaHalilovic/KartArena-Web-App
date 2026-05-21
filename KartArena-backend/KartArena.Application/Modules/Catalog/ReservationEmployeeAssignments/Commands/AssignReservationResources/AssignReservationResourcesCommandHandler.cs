using KartArena.Domain.Entities.Equipment;
using KartArena.Domain.Entities.Reservations;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KartArena.Application.Features.ReservationEmployeeAssignments.Commands.AssignReservationResources;

public class AssignReservationResourcesCommandHandler : IRequestHandler<AssignReservationResourcesCommand, int>
{
    private readonly IAppDbContext _context;

    public AssignReservationResourcesCommandHandler(IAppDbContext context)
    {
        _context = context;
    }
    
    public async Task<int> Handle(
        AssignReservationResourcesCommand request,
        CancellationToken cancellationToken)
    {
       
            var now = DateTime.UtcNow;

            var reservation = await _context.Reservations
                .FirstOrDefaultAsync(x => x.Id == request.ReservationId && !x.IsDeleted, cancellationToken);

            if (reservation is null)
                throw new KeyNotFoundException("Reservation not found.");

            if (reservation.Status != ReservationStatus.Confirmed)
                throw new InvalidOperationException("Only confirmed reservations can be assigned.");

            var employeeId = await ResolveEmployeeIdAsync(request, cancellationToken);

            var requestedEquipmentIds = request.EquipmentItemIds
                .Distinct()
                .ToList();

            var existingAssignments = await _context.ReservationEmployees
                .Where(x => x.ReservationId == request.ReservationId && !x.IsDeleted)
                .ToListAsync(cancellationToken);

            var existingEquipmentIds = existingAssignments
                .Where(x => x.EquipmentItemId.HasValue)
                .Select(x => x.EquipmentItemId!.Value)
                .Distinct()
                .ToList();

            var validEquipmentIds = await _context.EquipmentItemEntity
                .Where(x =>
                    !x.IsDeleted &&
                    requestedEquipmentIds.Contains(x.Id) &&
                    (
                        x.Status == EquipmentItemStatus.Available ||
                        existingEquipmentIds.Contains(x.Id)
                    ))
                .Select(x => x.Id)
                .ToListAsync(cancellationToken);

            if (validEquipmentIds.Count != requestedEquipmentIds.Count)
                throw new InvalidOperationException("One or more selected equipment items do not exist or are not available.");

            var conflictingEquipmentIds = await _context.ReservationEmployees
                .Where(x =>
                    !x.IsDeleted &&
                    x.EquipmentItemId.HasValue &&
                    requestedEquipmentIds.Contains(x.EquipmentItemId.Value) &&
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

            if (conflictingEquipmentIds.Count > 0)
            {
                throw new InvalidOperationException(
                    $"Some equipment items are already assigned in this time slot: {string.Join(", ", conflictingEquipmentIds)}");
            }

            if (request.ReplaceExistingAssignments)
            {
                var removedEquipmentIds = existingEquipmentIds
                    .Except(requestedEquipmentIds)
                    .ToList();

                if (removedEquipmentIds.Count > 0)
                {
                    var equipmentToFree = await _context.EquipmentItemEntity
                        .Where(x => removedEquipmentIds.Contains(x.Id))
                        .ToListAsync(cancellationToken);

                    foreach (var item in equipmentToFree)
                    {
                        item.Status = EquipmentItemStatus.Available;
                        item.ModifiedAtUtc = now;
                    }
                }

                foreach (var assignment in existingAssignments)
                {
                    assignment.IsDeleted = true;
                    assignment.ModifiedAtUtc = now;
                }
            }

            var equipmentIdsToInsert = request.ReplaceExistingAssignments
                ? requestedEquipmentIds
                : requestedEquipmentIds.Except(existingEquipmentIds).ToList();

            var newAssignments = equipmentIdsToInsert
                .Select(equipmentItemId => new ReservationEmployeeEntity
                {
                    ReservationId = request.ReservationId,
                    EmployeeId = employeeId,
                    EquipmentItemId = equipmentItemId,
                    CreatedAtUtc = now,
                    ModifiedAtUtc = null,
                    IsDeleted = false
                })
                .ToList();

            var equipmentToUse = await _context.EquipmentItemEntity
                .Where(x => requestedEquipmentIds.Contains(x.Id))
                .ToListAsync(cancellationToken);

            foreach (var item in equipmentToUse)
            {
                item.Status = EquipmentItemStatus.InUse;
                item.ModifiedAtUtc = now;
            }

            _context.ReservationEmployees.AddRange(newAssignments);

            await _context.SaveChangesAsync(cancellationToken);

            return newAssignments.Count;
        }
    

    private async Task<int> ResolveEmployeeIdAsync(
        AssignReservationResourcesCommand request,
        CancellationToken cancellationToken)
    {
        if (request.EmployeeId.HasValue)
        {
            var employeeExists = await _context.Users
                .AnyAsync(x => x.Id == request.EmployeeId.Value && !x.IsDeleted, cancellationToken);

            if (!employeeExists)
                throw new KeyNotFoundException("Employee not found.");

            return request.EmployeeId.Value;
        }

        var employeeName = request.EmployeeName?.Trim();

        if (string.IsNullOrWhiteSpace(employeeName))
            throw new InvalidOperationException("Employee is required.");

        var matches = await _context.Users
            .Where(x =>
                !x.IsDeleted &&
                (
                    ((x.FirstName ?? string.Empty) + " " + (x.LastName ?? string.Empty)).Trim() == employeeName ||
                    x.Username == employeeName ||
                    x.Email == employeeName
                ))
            .Select(x => new { x.Id })
            .ToListAsync(cancellationToken);

        if (matches.Count == 0)
            throw new KeyNotFoundException("Employee not found.");

        if (matches.Count > 1)
            throw new InvalidOperationException(
                "Multiple employees match the provided value. Please enter a unique employee name, username, or email.");

        return matches[0].Id;

    }

}
