using KartArena.Domain.Entities.Equipment;

namespace KartArena.Application.Modules.Catalog.EquipmentItems.Queries.GetById;

public sealed class GetEquipmentItemByIdQueryDto
{
    public required int Id { get; init; }
    public required string ItemCode { get; init; }
    public required int EquipmentTypeId { get; init; }
    public string? EquipmentTypeName { get; init; }
    public required EquipmentItemStatus Status { get; init; }
    public DateTime? PurchaseDate { get; init; }
    public DateTime? LastInspectedAt { get; init; }
    public string? Notes { get; init; }
}
