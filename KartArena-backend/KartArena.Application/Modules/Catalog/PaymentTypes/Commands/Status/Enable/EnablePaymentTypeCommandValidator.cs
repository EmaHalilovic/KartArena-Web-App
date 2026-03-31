namespace KartArena.Application.Modules.Catalog.PaymentTypes.Commands.Status.Enable;

public sealed class EnablePaymentTypeCommandValidator : AbstractValidator<EnablePaymentTypeCommand>
{
    public EnablePaymentTypeCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
