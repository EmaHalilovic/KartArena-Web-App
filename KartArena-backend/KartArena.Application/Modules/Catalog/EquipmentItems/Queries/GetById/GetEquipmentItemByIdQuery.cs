namespace KartArena.Application.Modules.Catalog.EquipmentItems.Queries.GetById;

public sealed record GetEquipmentItemByIdQuery(int Id) : IRequest<GetEquipmentItemByIdQueryDto>;
