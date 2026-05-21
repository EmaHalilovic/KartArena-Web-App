using FluentValidation;

namespace KartArena.Application.Modules.Catalog.Reservations.Commands.Create
{
    public sealed class CreateReservationCommandValidator
        : AbstractValidator<CreateReservationCommand>
    {
        public CreateReservationCommandValidator()
        {
            RuleFor(x => x.UserId)
                .GreaterThan(0);

            RuleFor(x => x.TrackId)
                .GreaterThan(0);

            RuleFor(x => x.KartId)
                .GreaterThan(0);

            RuleFor(x => x.PaymentTypeId)
                .GreaterThan(0);

            RuleFor(x => x.Amount)
                .GreaterThan(0);

            RuleFor(x => x.ReservationDate)
                .NotEmpty();

            RuleFor(x => x.StartTime)
                .NotEmpty();

            RuleFor(x => x.EndTime)
                .NotEmpty()
                .Must((cmd, end) => end > cmd.StartTime)
                .WithMessage("End time must be after start time.");

            RuleFor(x => x.PaymentNote)
                .MaximumLength(500);
        }
    }
}