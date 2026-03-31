using KartArena.Domain.Entities.Equipment;

namespace KartArena.Application.Modules.Catalog.EquipmentItems.Queries.List;

public sealed class ListEquipmentItemsQuery : BasePagedQuery<ListEquipmentItemsQueryDto>
{
    public string? Search { get; init; }
    public int? EquipmentTypeId { get; init; }
    public EquipmentItemStatus? Status { get; init; }
}
