namespace KartArena.Application.Modules.Catalog.Equipment.Commands.Status.Disable;

public sealed record DisableEquipmentCommand(int Id) : IRequest<Unit>;
