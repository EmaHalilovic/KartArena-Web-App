namespace KartArena.Application.Modules.Reports.Payments;

public sealed class GeneratePaymentsPdfQueryHandler(IAppDbContext db, PdfReportService pdf)
    : IRequestHandler<GeneratePaymentsPdfQuery, ReportFileResult>
{
    public async Task<ReportFileResult> Handle(GeneratePaymentsPdfQuery request, CancellationToken ct)
    {
        var query = db.Payments.AsNoTracking().Where(x => !x.IsDeleted);
        if (request.Id.HasValue) query = query.Where(x => x.Id == request.Id.Value);
        if (request.DateFrom.HasValue) query = query.Where(x => (x.PaymentDate ?? x.CreatedAtUtc) >= request.DateFrom.Value.Date);
        if (request.DateTo.HasValue)
        {
            var exclusiveEnd = request.DateTo.Value.Date.AddDays(1);
            query = query.Where(x => (x.PaymentDate ?? x.CreatedAtUtc) < exclusiveEnd);
        }
        var records = await query.OrderByDescending(x => x.PaymentDate ?? x.CreatedAtUtc).Select(x => new
        {
            x.Id, x.ReservationId,
            FirstName = x.Reservation.User != null ? x.Reservation.User.FirstName : x.Reservation.CustomerFirstName,
            LastName = x.Reservation.User != null ? x.Reservation.User.LastName : x.Reservation.CustomerLastName,
            x.Amount, x.Currency, PaymentType = x.PaymentType != null ? x.PaymentType.Name : string.Empty,
            x.Status, x.PaymentDate, x.TransactionReference, x.StripeCheckoutSessionId, x.CreatedAtUtc
        }).ToListAsync(ct);
        if (records.Count == 0) throw new MarketNotFoundException("No payments match the selected report filters.");
        IReadOnlyList<IReadOnlyList<string>> rows = records.Select(x => (IReadOnlyList<string>)new[]
        {
            x.Id.ToString(), x.ReservationId.ToString(),
            $"{x.FirstName ?? string.Empty} {x.LastName ?? string.Empty}".Trim(), x.Amount.ToString("N2"),
            string.IsNullOrWhiteSpace(x.Currency) ? "BAM" : x.Currency.ToUpperInvariant(),
            string.IsNullOrWhiteSpace(x.PaymentType) ? "-" : x.PaymentType, x.Status.ToString(),
            x.PaymentDate?.ToString("dd.MM.yyyy HH:mm") ?? "-", x.TransactionReference ?? "-",
            x.StripeCheckoutSessionId ?? "-", x.CreatedAtUtc.ToString("dd.MM.yyyy HH:mm")
        }).ToList();
        var bytes = pdf.Generate("Payments Report", Filters(request.Id, request.DateFrom, request.DateTo),
            new[] { "ID", "Reservation", "Customer", "Amount", "Currency", "Type", "Status", "Paid", "Reference", "Stripe Session", "Created" }, rows);
        return new(bytes, $"payments-report-{DateTime.Today:yyyy-MM-dd}.pdf");
    }
    private static string Filters(int? id, DateTime? from, DateTime? to) => id is null && from is null && to is null
        ? "All records" : $"ID: {id?.ToString() ?? "Any"}; From: {from?.ToString("dd.MM.yyyy") ?? "Any"}; To: {to?.ToString("dd.MM.yyyy") ?? "Any"}";
}
