namespace KartArena.Application.Modules.Catalog.PaymentTypes.Queries.List;

public sealed class ListPaymentTypesQuery : BasePagedQuery<ListPaymentTypesQueryDto>
{
    public string? Search { get; init; }
    public bool? OnlyEnabled { get; init; }
}
