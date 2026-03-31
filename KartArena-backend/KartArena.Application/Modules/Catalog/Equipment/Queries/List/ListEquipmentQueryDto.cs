using KartArena.Domain.Entities.Equipment;

namespace KartArena.Application.Modules.Catalog.Equipment.Queries.List;

public sealed class ListEquipmentQueryDto
{
    public required int Id { get; init; }
    public required string Name { get; init; }
    public required EquipmentCategory Category { get; init; }
    public required decimal Price { get; init; }
    public required string Size { get; init; }
    public string? Description { get; init; }
    public required bool IsActive { get; init; }
    public required int TotalItems { get; init; }
    public required int AvailableItems { get; init; }
    public int InUseItems { get; set; }
    public int MaintenanceItems { get; set; }
    public int LostItems { get; set; }
    public required EquipmentStockStatus StockStatus { get; init; }
}
