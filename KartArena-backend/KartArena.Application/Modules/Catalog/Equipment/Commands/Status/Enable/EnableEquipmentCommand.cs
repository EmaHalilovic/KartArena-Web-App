namespace KartArena.Application.Modules.Catalog.Equipment.Commands.Status.Enable;

public sealed record EnableEquipmentCommand(int Id) : IRequest<Unit>;
