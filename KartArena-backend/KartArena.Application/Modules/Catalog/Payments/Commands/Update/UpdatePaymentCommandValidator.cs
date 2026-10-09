using FluentValidation;
using KartArena.Application.Abstractions;
using KartArena.Domain.Entities.Payments;

namespace KartArena.Application.Modules.Catalog.Payments.Commands.Update;

public sealed class UpdatePaymentCommandValidator : AbstractValidator<UpdatePaymentCommand>
{
    public UpdatePaymentCommandValidator(IBusinessClock businessClock)
    {
        RuleFor(x => x.Id)
            .GreaterThan(0);

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Amount must be greater than 0.");

        RuleFor(x => x.PaymentTypeId)
            .GreaterThan(0)
           
            .WithMessage("PaymentTypeId must be greater than 0.");

        RuleFor(x => x.TransactionReference)
            .MaximumLength(100);

        RuleFor(x => x.Note)
            .MaximumLength(500);

        RuleFor(x => x.PaymentDate)
             .Must(paymentDate =>
                 !paymentDate.HasValue ||
                 businessClock.ToUtc(paymentDate.Value)
                     <= businessClock.UtcNow.AddMinutes(1))
             .WithMessage(
                 "PaymentDate cannot be in the future.");
    }
}