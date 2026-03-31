using KartArena.Domain.Entities.Equipment;
using System.Text.Json.Serialization;

namespace KartArena.Application.Modules.Catalog.Equipment.Commands.Update;

public sealed class UpdateEquipmentCommand : IRequest<Unit>
{
    [JsonIgnore]
    public int Id { get; set; }
    public string? Name { get; set; }
    public EquipmentCategory? Category { get; set; }
    public decimal? Price { get; set; }
    public string? Size { get; set; }
    public string? Description { get; set; }
    public bool? IsActive { get; set; }
}
