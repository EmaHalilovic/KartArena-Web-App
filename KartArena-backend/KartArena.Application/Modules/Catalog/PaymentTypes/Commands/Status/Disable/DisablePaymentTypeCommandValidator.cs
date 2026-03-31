namespace KartArena.Application.Modules.Catalog.PaymentTypes.Commands.Status.Disable;

public sealed class DisablePaymentTypeCommandValidator : AbstractValidator<DisablePaymentTypeCommand>
{
    public DisablePaymentTypeCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
