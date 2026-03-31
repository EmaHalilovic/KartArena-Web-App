namespace KartArena.Application.Modules.Catalog.PaymentTypes.Queries.GetById;

public sealed class GetPaymentTypeByIdQueryValidator : AbstractValidator<GetPaymentTypeByIdQuery>
{
    public GetPaymentTypeByIdQueryValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
