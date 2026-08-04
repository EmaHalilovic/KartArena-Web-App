using KartArena.Domain.Entities.Equipment;

namespace KartArena.Application.Modules.Catalog.Equipment.Queries.List;

public sealed class ListEquipmentQuery : BasePagedQuery<ListEquipmentQueryDto>
{
    public string? Search { get; init; }
    public bool? OnlyActive { get; init; }
    public EquipmentCategory? Category { get; init; }
    public EquipmentStockStatus? StockStatus { get; init; }
    public string? Size { get; init; }

}
