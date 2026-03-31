using KartArena.Domain.Entities.Equipment;

namespace KartArena.Application.Modules.Catalog.Equipment.Queries.GetById;

public class GetEquipmentByIdQueryHandler(IAppDbContext context)
    : IRequestHandler<GetEquipmentByIdQuery, GetEquipmentByIdQueryDto>
{
    private const int LowStockThreshold = 5;

    public async Task<GetEquipmentByIdQueryDto> Handle(GetEquipmentByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await context.EquipmentEntity
            .AsNoTracking()
            .Where(x => x.Id == request.Id)
            .Select(x => new GetEquipmentByIdQueryDto
            {
                Id = x.Id,
                Name = x.Name,
                Category = x.Category,
                Price = x.Price,
                Size = x.Size,
                Description = x.Description,
                IsActive = x.isEnabled,

                TotalItems = x.Items.Count(),
                AvailableItems = x.Items.Count(i => i.Status == EquipmentItemStatus.Available),
                InUseItems = x.InUseItems,
                MaintenanceItems = x.MaintenanceItems,
                LostItems = x.LostItems,
                StockStatus = x.Items.Count(i => i.Status == EquipmentItemStatus.Available) == 0
                    ? EquipmentStockStatus.OutOfStock
                    : x.Items.Count(i => i.Status == EquipmentItemStatus.Available) <= LowStockThreshold
                        ? EquipmentStockStatus.Low
                        : EquipmentStockStatus.Good,

                Items = x.Items
                    .OrderBy(i => i.ItemCode)
                    .Select(i => new GetEquipmentByIdQueryDtoItem
                    {
                        Id = i.Id,
                        ItemCode = i.ItemCode,
                        Status = i.Status,
                        PurchaseDate = i.PurchaseDate,
                        LastInspectedAt = i.LastInspectedAt,
                        Notes = i.Notes
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (entity is null)
            throw new MarketNotFoundException($"Equipment with Id {request.Id} not found.");

        return entity;
    }
}