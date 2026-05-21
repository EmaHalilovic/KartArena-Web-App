namespace KartArena.Application.Modules.Catalog.Reservations.Commands.Update;

public sealed class UpdateReservationCommandValidator
    : AbstractValidator<UpdateReservationCommand>
{
    public UpdateReservationCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0);

        RuleFor(x => x.UserId)
            .GreaterThan(0);

        RuleFor(x => x.TrackId)
            .GreaterThan(0);

        RuleFor(x => x.KartId)
            .GreaterThan(0);

        RuleFor(x => x.ReservationDate)
            .NotEmpty();

        RuleFor(x => x.StartTime)
            .NotEmpty();

        RuleFor(x => x.EndTime)
            .NotEmpty()
            .Must((cmd, end) => end > cmd.StartTime)
            .WithMessage("End time must be after start time.");
    }
}