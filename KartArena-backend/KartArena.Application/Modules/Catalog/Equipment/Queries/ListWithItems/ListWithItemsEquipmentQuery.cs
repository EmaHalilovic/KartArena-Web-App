namespace KartArena.Application.Modules.Catalog.Equipment.Queries.List;

public sealed class ListWithItemsEquipmentQuery : BasePagedQuery<ListWithItemsEquipmentQueryDto>
{
    public string? Search { get; init; }
}
