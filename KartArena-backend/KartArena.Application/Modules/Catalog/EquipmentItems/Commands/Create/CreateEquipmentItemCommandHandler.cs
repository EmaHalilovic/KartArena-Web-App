using KartArena.Domain.Entities.Equipment;

namespace KartArena.Application.Modules.Catalog.EquipmentItems.Commands.Create;

public sealed class CreateEquipmentItemCommandHandler(IAppDbContext context)
    : IRequestHandler<CreateEquipmentItemCommand, int>
{
    public async Task<int> Handle(CreateEquipmentItemCommand request, CancellationToken cancellationToken)
    {
        var normalizedItemCode = request.ItemCode?.Trim();

        if (string.IsNullOrWhiteSpace(normalizedItemCode))
            throw new ValidationException("Item code is required.");

        var equipmentTypeExists = await context.EquipmentEntity
            .AnyAsync(x => x.Id == request.EquipmentTypeId, cancellationToken);

        if (!equipmentTypeExists)
            throw new MarketNotFoundException("Equipment type was not found.");

        var exists = await context.EquipmentItemEntity
            .AnyAsync(x => x.ItemCode == normalizedItemCode, cancellationToken);

        if (exists)
            throw new MarketConflictException("Equipment item with the same code already exists.");

        var entity = new EquipmentItemEntity
        {
            ItemCode = normalizedItemCode,
            EquipmentTypeId = request.EquipmentTypeId,
            Status = request.Status,
            PurchaseDate = request.PurchaseDate,
            LastInspectedAt = request.LastInspectedAt,
            Notes = request.Notes
        };

        context.EquipmentItemEntity.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
