using KartArena.Domain.Entities.Equipment;
using System.Text.Json.Serialization;

namespace KartArena.Application.Modules.Catalog.EquipmentItems.Commands.Update;

public sealed class UpdateEquipmentItemCommand : IRequest<Unit>
{
    [JsonIgnore]
    public int Id { get; set; }
    public string? ItemCode { get; set; }
    public int? EquipmentTypeId { get; set; }
    public EquipmentItemStatus? Status { get; set; }
    public DateTime? PurchaseDate { get; set; }
    public DateTime? LastInspectedAt { get; set; }
    public string? Notes { get; set; }
}
