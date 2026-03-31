using KartArena.Domain.Entities.Equipment;

namespace KartArena.Application.Modules.Catalog.Equipment.Queries.GetById;

public class GetEquipmentByIdQueryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public EquipmentCategory Category { get; set; }
    public decimal Price { get; set; }
    public string? Size { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }

    public int TotalItems { get; set; }
    public int AvailableItems { get; set; }
    public int InUseItems { get; set; }
    public int MaintenanceItems { get; set; }
    public int LostItems { get; set; }
    public EquipmentStockStatus StockStatus { get; set; }

    public List<GetEquipmentByIdQueryDtoItem> Items { get; set; } = [];
}

public class GetEquipmentByIdQueryDtoItem
{
    public int Id { get; set; }
    public string ItemCode { get; set; } = default!;
    public EquipmentItemStatus Status { get; set; }
    public DateTime? PurchaseDate { get; set; }
    public DateTime? LastInspectedAt { get; set; }
    public string? Notes { get; set; }
}