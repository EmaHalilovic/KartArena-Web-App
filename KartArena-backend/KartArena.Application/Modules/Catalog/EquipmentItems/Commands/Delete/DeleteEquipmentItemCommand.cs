namespace KartArena.Application.Modules.Catalog.EquipmentItems.Commands.Delete;

public sealed record DeleteEquipmentItemCommand(int Id) : IRequest<Unit>;
