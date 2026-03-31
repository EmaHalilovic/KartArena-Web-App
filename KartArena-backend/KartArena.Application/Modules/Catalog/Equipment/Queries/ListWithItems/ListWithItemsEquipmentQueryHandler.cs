using KartArena.Domain.Entities.Equipment;
using Microsoft.EntityFrameworkCore;
using System.Threading;

namespace KartArena.Application.Modules.Catalog.Equipment.Queries.List;

public sealed class ListWithItemsEquipmentQueryHandler(IAppDbContext ctx)
    : IRequestHandler<ListWithItemsEquipmentQuery, PageResult<ListWithItemsEquipmentQueryDto>>
{
    private const int LowStockThreshold = 5;

    public async Task<PageResult<ListWithItemsEquipmentQueryDto>> Handle(ListWithItemsEquipmentQuery request, CancellationToken cancellationToken)
    {
        var result =  ctx.EquipmentEntity
               .AsNoTracking()
               .Select(e => new ListWithItemsEquipmentQueryDto
               {
                   Id = e.Id,
                   Name = e.Name,
                   Price = e.Price,
                   Size = e.Size,
                   Description = e.Description,
                   Category = e.Category,

                   TotalItems = e.Items.Count(),
                   AvailableItems = e.AvailableItems,
                   InUseItems =e.InUseItems,
                   MaintenanceItems = e.MaintenanceItems,
                   LostItems = e.LostItems,
                    
                   StockStatus =
                       e.Items.Count(i => i.Status == EquipmentItemStatus.Available) == 0
                           ? EquipmentStockStatus.OutOfStock
                           : e.Items.Count(i => i.Status == EquipmentItemStatus.Available) <= LowStockThreshold
                               ? EquipmentStockStatus.Low
                               : EquipmentStockStatus.Good,

                   Items = e.Items
                       .OrderBy(i => i.ItemCode)
                       .Select(i => new ListWithItemsEquipmentQueryDtoItem
                       {
                           Id = i.Id,
                           ItemCode = i.ItemCode,
                           Status = i.Status,
                           PurchaseDate = i.PurchaseDate,
                           LastInspectedAt = i.LastInspectedAt,
                           Notes = i.Notes
                       })
                       .ToList()
               });

        return await PageResult<ListWithItemsEquipmentQueryDto>.FromQueryableAsync(result, request.Paging, cancellationToken);
    }
}
