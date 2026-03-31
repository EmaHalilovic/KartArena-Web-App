namespace KartArena.Application.Modules.Catalog.EquipmentItems.Queries.List;

public sealed class ListEquipmentItemsQueryHandler(IAppDbContext ctx)
    : IRequestHandler<ListEquipmentItemsQuery, PageResult<ListEquipmentItemsQueryDto>>
{
    public async Task<PageResult<ListEquipmentItemsQueryDto>> Handle(ListEquipmentItemsQuery request, CancellationToken ct)
    {
        var query = ctx.EquipmentItemEntity.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(x => x.ItemCode.Contains(request.Search) ||
                                     (x.Notes != null && x.Notes.Contains(request.Search)));
        }

        if (request.EquipmentTypeId.HasValue)
            query = query.Where(x => x.EquipmentTypeId == request.EquipmentTypeId.Value);

        if (request.Status.HasValue)
            query = query.Where(x => x.Status == request.Status.Value);

        var projectedQuery = query
            .OrderBy(x => x.ItemCode)
            .Select(x => new ListEquipmentItemsQueryDto
            {
                Id = x.Id,
                ItemCode = x.ItemCode,
                EquipmentTypeId = x.EquipmentTypeId,
                EquipmentTypeName = x.EquipmentType.Name,
                Status = x.Status,
                PurchaseDate = x.PurchaseDate,
                LastInspectedAt = x.LastInspectedAt,
                Notes = x.Notes
            });

        return await PageResult<ListEquipmentItemsQueryDto>.FromQueryableAsync(projectedQuery, request.Paging, ct);
    }
}
