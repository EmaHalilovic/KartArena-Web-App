using FluentValidation;

namespace KartArena.Application.Modules.Catalog.Reservations.Commands.MarkCashPaymentAsPaid
{
    public sealed class MarkCashReservationPaymentAsPaidCommandValidator
        : AbstractValidator<MarkCashReservationPaymentAsPaidCommand>
    {
        public MarkCashReservationPaymentAsPaidCommandValidator()
        {
            RuleFor(x => x.ReservationId)
                .GreaterThan(0);

            RuleFor(x => x.TransactionReference)
                .MaximumLength(100);

            RuleFor(x => x.Note)
                .MaximumLength(500);
        }
    }
}