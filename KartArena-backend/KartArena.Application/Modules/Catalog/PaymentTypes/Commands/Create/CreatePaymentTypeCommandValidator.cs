using FluentValidation;

namespace KartArena.Application.Modules.Catalog.PaymentTypes.Commands.Create;

public sealed class CreatePaymentTypeCommandValidator : AbstractValidator<CreatePaymentTypeCommand>
{
    public CreatePaymentTypeCommandValidator()
    {
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