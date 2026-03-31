namespace KartArena.Application.Modules.Catalog.Reservations.Queries.List;

public sealed class ListReservationsQuery : BasePagedQuery<ListReservationsQueryDto>
{
    public string? Search { get; init; }
    public bool? OnlyEnabled { get; init; }
}
