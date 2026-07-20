namespace KartArena.Application.Modules.Reports.Reservations;

public sealed record GenerateReservationsPdfQuery(int? Id, DateTime? DateFrom, DateTime? DateTo) : IRequest<ReportFileResult>;

public sealed class GenerateReservationsPdfQueryValidator : AbstractValidator<GenerateReservationsPdfQuery>
{
    public GenerateReservationsPdfQueryValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).When(x => x.Id.HasValue);
        RuleFor(x => x.DateTo).GreaterThanOrEqualTo(x => x.DateFrom)
            .When(x => x.DateFrom.HasValue && x.DateTo.HasValue)
            .WithMessage("Start date cannot be later than end date.");
    }
}
