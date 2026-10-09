using MediatR;

namespace KartArena.Application.Modules.Catalog.Reservations.Commands.Checkout;
public sealed class CheckoutReservationsCommandValidator
    : AbstractValidator<CheckoutReservationsCommand>
{
    public CheckoutReservationsCommandValidator()
    {
        RuleFor(x => x.Items)
            .NotEmpty()
            .WithMessage("Cart must contain at least one reservation.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.TrackId)
                .GreaterThan(0);

            item.RuleFor(x => x.KartId)
                .GreaterThan(0);

            item.RuleFor(x => x.ReservationDate)
                .NotEmpty();

            item.RuleFor(x => x.StartTime)
                .NotEmpty();

            item.RuleFor(x => x.EndTime)
                .GreaterThan(x => x.StartTime);
        });

        RuleFor(x => x.CustomerFirstName)
            .NotEmpty()
            .WithMessage("First name is required.");

        RuleFor(x => x.CustomerLastName)
            .NotEmpty()
            .WithMessage("Last name is required.");

        RuleFor(x => x.CustomerEmail)
            .NotEmpty()
            .WithMessage("Email is required.")
            .EmailAddress()
            .WithMessage("Email address is not valid.");

        RuleFor(x => x.CustomerPhone)
            .NotEmpty()
            .WithMessage("Phone number is required.");

        //RuleFor(x => x.UserId)
        //    .GreaterThan(0)
        //    .When(x => x.UserId.HasValue);

        RuleFor(x => x.PaymentTypeId)
            .NotNull()
            .WithMessage("Payment type is required.")
            .GreaterThan(0)
            .When(x => x.PaymentTypeId.HasValue);

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .When(x => x.Amount.HasValue);
    }
}