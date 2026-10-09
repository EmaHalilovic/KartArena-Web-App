using KartArena.Domain.Entities.Equipment;
using KartArena.Domain.Entities.Reservations;

namespace KartArena.Application.Modules.Catalog.EquipmentItems.Commands.Update;

public sealed class UpdateEquipmentItemCommandHandler(IAppDbContext ctx)
    : IRequestHandler<UpdateEquipmentItemCommand, Unit>
{
    public async Task<Unit> Handle(UpdateEquipmentItemCommand request, CancellationToken ct)
    {
        var entity = await ctx.EquipmentItemEntity
            .Where(x => x.Id == request.Id)
            .FirstOrDefaultAsync(ct);

        if (entity is null)
            throw new MarketNotFoundException($"Equipment item (ID={request.Id}) was not found.");

        if (request.Status == EquipmentItemStatus.Available)
        {
            var hasActiveAssignment = await ctx.ReservationEmployees
                .AnyAsync(x =>
                    !x.IsDeleted &&
                    x.EquipmentItemId == request.Id &&
                    x.Reservation != null &&
                    !x.Reservation.IsDeleted &&
                    x.Reservation.Status != ReservationStatus.Completed &&
                    x.Reservation.Status != ReservationStatus.Cancelled, ct);

            if (hasActiveAssignment)
                throw new MarketBusinessRuleException(
                    "EQUIPMENT_STILL_ASSIGNED",
                    "Equipment cannot be marked as available while assigned to an active reservation. Remove its assignment first.");
        }

        var itemCode = request.ItemCode?.Trim();

        if (!string.IsNullOrWhiteSpace(itemCode))
        {
            var exists = await ctx.EquipmentItemEntity
                .AnyAsync(x => x.Id != request.Id && x.ItemCode == itemCode, ct);

            if (exists)
                throw new MarketConflictException("Equipment item code already exists.");

            entity.ItemCode = itemCode;
        }

        if (request.EquipmentTypeId.HasValue)
        {
            var equipmentTypeExists = await ctx.EquipmentEntity
                .AnyAsync(x => x.Id == request.EquipmentTypeId.Value, ct);

            if (!equipmentTypeExists)
                throw new MarketNotFoundException("Equipment type was not found.");

            entity.EquipmentTypeId = request.EquipmentTypeId.Value;
        }

        entity.Status = request.Status ?? entity.Status;
        entity.PurchaseDate = request.PurchaseDate ?? entity.PurchaseDate;
        entity.LastInspectedAt = request.LastInspectedAt ?? entity.LastInspectedAt;
        entity.Notes = request.Notes ?? entity.Notes;

        await ctx.SaveChangesAsync(ct);

        return Unit.Value;
    }
}
