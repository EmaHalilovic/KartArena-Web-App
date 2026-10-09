namespace KartArena.Application.Modules.Reports.Payments;

public sealed class GeneratePaymentsPdfQueryHandler(
    IAppDbContext db,
    PdfReportService pdf,
    IBusinessClock businessClock)
    : IRequestHandler<GeneratePaymentsPdfQuery, ReportFileResult>
{
    public async Task<ReportFileResult> Handle(GeneratePaymentsPdfQuery request, CancellationToken ct)
    {
        var query = db.Payments.AsNoTracking().Where(x => !x.IsDeleted);
        if (request.Id.HasValue) query = query.Where(x => x.Id == request.Id.Value);
        if (request.DateFrom.HasValue)
        {
            var utcStart = businessClock.ToUtc(request.DateFrom.Value.Date);
            query = query.Where(x => (x.PaymentDate ?? x.CreatedAtUtc) >= utcStart);
        }
        if (request.DateTo.HasValue)
        {
            var utcExclusiveEnd = businessClock.ToUtc(request.DateTo.Value.Date.AddDays(1));
            query = query.Where(x => (x.PaymentDate ?? x.CreatedAtUtc) < utcExclusiveEnd);
        }
        var records = await query.OrderByDescending(x => x.PaymentDate ?? x.CreatedAtUtc).Select(x => new
        {
            x.Id,
            ReservationId = x.PaymentReservations
            .Select(pr => pr.ReservationId)
            .FirstOrDefault(),

            FirstName = x.PaymentReservations
            .Select(pr => pr.Reservation.User != null
                ? pr.Reservation.User.FirstName
                : pr.Reservation.CustomerFirstName)
            .FirstOrDefault(),

            LastName = x.PaymentReservations
            .Select(pr => pr.Reservation.User != null
                ? pr.Reservation.User.LastName
                : pr.Reservation.CustomerLastName)
            .FirstOrDefault(),
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
            x.PaymentDate is DateTime paymentDate
                ? businessClock.ToBusinessTime(paymentDate).ToString("dd.MM.yyyy HH:mm")
                : "-",
            x.TransactionReference ?? "-", x.StripeCheckoutSessionId ?? "-",
            businessClock.ToBusinessTime(x.CreatedAtUtc).ToString("dd.MM.yyyy HH:mm")
        }).ToList();
        var bytes = pdf.Generate("Payments Report", Filters(request.Id, request.DateFrom, request.DateTo),
            new[] { "ID", "Reservation", "Customer", "Amount", "Currency", "Type", "Status", "Paid", "Reference", "Stripe Session", "Created" }, rows);
        return new(bytes, $"payments-report-{businessClock.Today:yyyy-MM-dd}.pdf");
    }
    private static string Filters(int? id, DateTime? from, DateTime? to) => id is null && from is null && to is null
        ? "All records" : $"ID: {id?.ToString() ?? "Any"}; From: {from?.ToString("dd.MM.yyyy") ?? "Any"}; To: {to?.ToString("dd.MM.yyyy") ?? "Any"}";
}
