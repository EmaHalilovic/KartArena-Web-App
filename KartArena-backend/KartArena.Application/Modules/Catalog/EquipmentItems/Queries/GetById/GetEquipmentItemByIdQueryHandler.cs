namespace KartArena.Application.Modules.Catalog.EquipmentItems.Queries.GetById;

public sealed class GetEquipmentItemByIdQueryHandler(IAppDbContext context)
    : IRequestHandler<GetEquipmentItemByIdQuery, GetEquipmentItemByIdQueryDto>
{
    public async Task<GetEquipmentItemByIdQueryDto> Handle(GetEquipmentItemByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await context.EquipmentItemEntity
            .Where(x => x.Id == request.Id)
            .Select(x => new GetEquipmentItemByIdQueryDto
            {
                Id = x.Id,
                ItemCode = x.ItemCode,
                EquipmentTypeId = x.EquipmentTypeId,
                EquipmentTypeName = x.EquipmentType.Name,
                Status = x.Status,
                PurchaseDate = x.PurchaseDate,
                LastInspectedAt = x.LastInspectedAt,
                Notes = x.Notes
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (entity is null)
            throw new MarketNotFoundException($"Equipment item with Id {request.Id} was not found.");

        return entity;
    }
}
