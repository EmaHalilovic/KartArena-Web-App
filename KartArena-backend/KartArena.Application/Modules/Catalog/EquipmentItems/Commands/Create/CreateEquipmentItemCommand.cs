using KartArena.Domain.Entities.Equipment;

namespace KartArena.Application.Modules.Catalog.EquipmentItems.Commands.Create;

public sealed class CreateEquipmentItemCommand : IRequest<int>
{
    public required string ItemCode { get; set; }
    public required int EquipmentTypeId { get; set; }
    public EquipmentItemStatus Status { get; set; } = EquipmentItemStatus.Available;
    public DateTime? PurchaseDate { get; set; }
    public DateTime? LastInspectedAt { get; set; }
    public string? Notes { get; set; }
}
