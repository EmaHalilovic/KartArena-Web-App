namespace KartArena.Application.Modules.Catalog.Equipment.Queries.GetById;

public sealed record GetEquipmentByIdQuery(int Id) : IRequest<GetEquipmentByIdQueryDto>;
