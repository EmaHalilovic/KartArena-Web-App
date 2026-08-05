namespace KartArena.Application.Modules.Reports.Reservations;

public sealed class GenerateReservationsPdfQueryHandler(IAppDbContext db, PdfReportService pdf)
    : IRequestHandler<GenerateReservationsPdfQuery, ReportFileResult>
{
    public async Task<ReportFileResult> Handle(GenerateReservationsPdfQuery request, CancellationToken ct)
    {
        var query = db.Reservations.AsNoTracking().Where(x => !x.IsDeleted);
        if (request.Id.HasValue) query = query.Where(x => x.Id == request.Id.Value);
        if (request.DateFrom.HasValue) query = query.Where(x => x.Date >= request.DateFrom.Value.Date);
        if (request.DateTo.HasValue)
        {
            var exclusiveEnd = request.DateTo.Value.Date.AddDays(1);
            query = query.Where(x => x.Date < exclusiveEnd);
        }
        var records = await query.OrderByDescending(x => x.Date).ThenByDescending(x => x.StartTime).Select(x => new
        {
            x.Id, x.Date, x.StartTime, x.EndTime,
            FirstName = x.User != null ? x.User.FirstName : x.CustomerFirstName,
            LastName = x.User != null ? x.User.LastName : x.CustomerLastName,
            Email = x.User != null ? x.User.Email : x.CustomerEmail,
            Track = x.Track != null ? x.Track.Name : string.Empty,
            Kart = x.Kart != null ? x.Kart.Name : string.Empty,
            x.Status, x.PaymentStatus, x.CreatedAtUtc
        }).ToListAsync(ct);
        if (records.Count == 0) throw new MarketNotFoundException("No reservations match the selected report filters.");
        IReadOnlyList<IReadOnlyList<string>> rows = records.Select(x => (IReadOnlyList<string>)new[]
        {
            x.Id.ToString(), x.Date.ToString("dd.MM.yyyy"), x.StartTime.ToString("HH:mm"), x.EndTime.ToString("HH:mm"),
            $"{x.FirstName ?? string.Empty} {x.LastName ?? string.Empty}".Trim(), x.Email ?? "-",
            x.Track ?? "-", x.Kart ?? "-", x.Status.ToString(),
            x.PaymentStatus.ToString(), x.CreatedAtUtc.ToString("dd.MM.yyyy HH:mm")
        }).ToList();
        var bytes = pdf.Generate("Reservations Report", Filters(request.Id, request.DateFrom, request.DateTo),
            new[] { "ID", "Date", "Start", "End", "Customer", "Email", "Track", "Kart", "Status", "Payment", "Created" }, rows);
        return new(bytes, $"reservations-report-{DateTime.Today:yyyy-MM-dd}.pdf");
    }
    private static string Filters(int? id, DateTime? from, DateTime? to) => id is null && from is null && to is null
        ? "All records" : $"ID: {id?.ToString() ?? "Any"}; From: {from?.ToString("dd.MM.yyyy") ?? "Any"}; To: {to?.ToString("dd.MM.yyyy") ?? "Any"}";
}
