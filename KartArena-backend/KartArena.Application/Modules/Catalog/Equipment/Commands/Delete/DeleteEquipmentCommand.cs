namespace KartArena.Application.Modules.Catalog.Equipment.Commands.Delete;

public sealed record DeleteEquipmentCommand(int Id) : IRequest<Unit>;
