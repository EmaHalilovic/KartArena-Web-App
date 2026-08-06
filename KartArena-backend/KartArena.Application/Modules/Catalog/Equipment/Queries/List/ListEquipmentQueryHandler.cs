using KartArena.Domain.Entities.Equipment;

namespace KartArena.Application.Modules.Catalog.Equipment.Queries.List;

public sealed class ListEquipmentQueryHandler(IAppDbContext ctx)
    : IRequestHandler<ListEquipmentQuery, PageResult<ListEquipmentQueryDto>>
{
    private const int LowStockThreshold = 5;

    public async Task<PageResult<ListEquipmentQueryDto>> Handle(ListEquipmentQuery request, CancellationToken ct)
    {
        var q = ctx.EquipmentEntity.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            q = q.Where(x => x.Name.Contains(search) || x.Size.Contains(search));
        }

        if (request.OnlyActive is not null)
            q = q.Where(x => x.isEnabled == request.OnlyActive);

        if (request.StockStatus is not null)
            q = q.Where(x => x.StockStatus == request.StockStatus);

        if (request.Category is not null)
            q = q.Where(x => x.Category == request.Category);

        if (!string.IsNullOrWhiteSpace(request.Size))
        {
            var size = request.Size.Trim().ToLower();
            q = q.Where(x => x.Size.ToLower().Contains(size));
        }


        var projectedQuery = q.OrderBy(x => x.Name)
            .ThenBy(x => x.Size)
            .Select(x => new ListEquipmentQueryDto
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
                        : EquipmentStockStatus.Good
            });

        return await PageResult<ListEquipmentQueryDto>.FromQueryableAsync(projectedQuery, request.Paging, ct);
    }
}
