namespace KartArena.Application.Modules.Catalog.PaymentTypes.Queries.List;

public sealed class ListPaymentTypesQuery : BasePagedQuery<ListPaymentTypesQueryDto>
{
    public string? Search { get; init; }
    public string? Name { get; init; }
    public string? Code { get; init; }
    public string? PaymentMethod { get; init; }
    public bool? AllowedOnline { get; init; }
    public bool? OnlyEnabled { get; init; }
}
