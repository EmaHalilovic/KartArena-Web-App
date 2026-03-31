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
