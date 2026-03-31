using FluentValidation;

namespace KartArena.Application.Modules.Catalog.PaymentTypes.Commands.Update;

public sealed class UpdatePaymentTypeCommandValidator : AbstractValidator<UpdatePaymentTypeCommand>
{
    public UpdatePaymentTypeCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0);

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Description)
            .MaximumLength(500);

        RuleFor(x => x)
            .Must(x => x.AllowedOnline || x.AllowedAtDesk)
            .WithMessage("Payment type must be allowed either online or at desk.");
    }
}