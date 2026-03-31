namespace KartArena.Application.Modules.Catalog.PaymentTypes.Queries.GetById;

public sealed class GetPaymentTypeByIdQueryDto
{
    public int Id { get; init; }

    public required string Name { get; init; }

    public required string Code { get; init; }

    public bool AllowedOnline { get; init; }

    public bool AllowedAtDesk { get; init; }

    public string? Description { get; init; }

}