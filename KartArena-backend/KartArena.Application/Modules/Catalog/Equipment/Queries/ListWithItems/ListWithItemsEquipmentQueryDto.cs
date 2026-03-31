using KartArena.Domain.Entities.Equipment;

namespace KartArena.Application.Modules.Catalog.Equipment.Queries.List;

public sealed class ListWithItemsEquipmentQueryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Size { get; set; } = string.Empty;
    public string? Description { get; set; }
    public EquipmentCategory Category { get; set; }

    public int TotalItems { get; set; }
    public int AvailableItems { get; set; }
    public int InUseItems { get; set; }
    public int MaintenanceItems { get; set; }
    public int LostItems { get; set; }

    public EquipmentStockStatus StockStatus { get; set; }

    public List<ListWithItemsEquipmentQueryDtoItem> Items { get; set; } = new();
}

public sealed class ListWithItemsEquipmentQueryDtoItem
{
    public int Id { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public EquipmentItemStatus Status { get; set; }
    public DateTime? PurchaseDate { get; set; }
    public DateTime? LastInspectedAt { get; set; }
    public string? Notes { get; set; }
}
