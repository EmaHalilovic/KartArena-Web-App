using FluentValidation;

namespace KartArena.Application.Modules.Catalog.Payments.Commands.Create;

public sealed class CreatePaymentCommandValidator : AbstractValidator<CreatePaymentCommand>
{
    public CreatePaymentCommandValidator()
    {
        RuleFor(x => x.ReservationId)
            .GreaterThan(0);

        RuleFor(x => x.Amount)
            .GreaterThan(0);

        RuleFor(x => x.PaymentTypeId)
            .GreaterThan(0)
            .When(x => x.PaymentTypeId.HasValue);

        RuleFor(x => x.TransactionReference)
            .MaximumLength(100);

        RuleFor(x => x.Note)
            .MaximumLength(500);
    }
}