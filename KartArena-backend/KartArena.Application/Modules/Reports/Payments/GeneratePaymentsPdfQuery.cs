namespace KartArena.Application.Modules.Reports.Payments;

public sealed record GeneratePaymentsPdfQuery(int? Id, DateTime? DateFrom, DateTime? DateTo) : IRequest<ReportFileResult>;

public sealed class GeneratePaymentsPdfQueryValidator : AbstractValidator<GeneratePaymentsPdfQuery>
{
    public GeneratePaymentsPdfQueryValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).When(x => x.Id.HasValue);
        RuleFor(x => x.DateTo).GreaterThanOrEqualTo(x => x.DateFrom)
            .When(x => x.DateFrom.HasValue && x.DateTo.HasValue)
            .WithMessage("Start date cannot be later than end date.");
    }
}
