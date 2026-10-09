using KartArena.Application.Modules.Reports.Payments;
using KartArena.Application.Modules.Reports.Reservations;
using Microsoft.AspNetCore.Authorization;

namespace KartArena.API.Controllers;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/reports")]
public sealed class ReportsController(ISender sender) : ControllerBase
{
    [HttpGet("reservations/pdf")]
    public async Task<IActionResult> ReservationsPdf(
        [FromQuery] int? id,
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo,
        CancellationToken ct)
    {
        var report = await sender.Send(
            new GenerateReservationsPdfQuery(id, dateFrom, dateTo),
            ct);

        return File(
            report.Content,
            "application/pdf",
            report.FileName);
    }

    [HttpGet("payments/pdf")]
    public async Task<IActionResult> PaymentsPdf(
        [FromQuery] int? id,
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo,
        CancellationToken ct)
    {
        var report = await sender.Send(
            new GeneratePaymentsPdfQuery(id, dateFrom, dateTo),
            ct);

        return File(
            report.Content,
            "application/pdf",
            report.FileName);
    }
}