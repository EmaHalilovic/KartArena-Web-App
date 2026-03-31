using KartArena.Domain.Entities.Equipment;

namespace KartArena.Application.Modules.Catalog.Equipment.Commands.Create;

public class CreateEquipmentCommand : IRequest<int>
{
    public required string Name { get; set; }
    public required EquipmentCategory Category { get; set; }
    public required decimal Price { get; set; }
    public required string Size { get; set; }
    public string? Description { get; set; }
}
