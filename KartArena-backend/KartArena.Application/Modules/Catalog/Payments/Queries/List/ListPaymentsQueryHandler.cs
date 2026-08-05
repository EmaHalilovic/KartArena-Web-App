using KartArena.Domain.Entities.Payments;
using Microsoft.EntityFrameworkCore;

namespace KartArena.Application.Modules.Catalog.Payments.Queries.List;

public sealed class ListPaymentsQueryHandler(IAppDbContext context)
    : IRequestHandler<ListPaymentsQuery, PageResult<ListPaymentsQueryDto>>
{
    public async Task<PageResult<ListPaymentsQueryDto>> Handle(
        ListPaymentsQuery request,
        CancellationToken ct)
    {
        var q = context.Payments
            .AsNoTracking()
            .Where(x => !x.IsDeleted);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();

            q = q.Where(x =>
                x.Id.ToString().Contains(search) ||

                x.PaymentReservations.Any(link =>
                    link.ReservationId.ToString().Contains(search) ||

                    link.Reservation.CustomerFirstName.Contains(search) ||

                    link.Reservation.CustomerLastName.Contains(search) ||

                    link.Reservation.CustomerEmail.Contains(search)) ||

                x.Amount.ToString().Contains(search) ||

                (x.TransactionReference != null &&
                 x.TransactionReference.Contains(search)) ||

                (x.Note != null &&
                 x.Note.Contains(search)) ||

                (x.PaymentType != null &&
                 x.PaymentType.Name != null &&
                 x.PaymentType.Name.Contains(search)));
        }

        if (request.ReservationId.HasValue)
        {
            var reservationId = request.ReservationId.Value;

            q = q.Where(x =>
                x.PaymentReservations.Any(link =>
                    link.ReservationId == reservationId));
        }

        if (request.PaymentTypeId.HasValue)
        {
            q = q.Where(x =>
                x.PaymentTypeId == request.PaymentTypeId.Value);
        }

        if (request.Status.HasValue)
        {
            q = q.Where(x =>
                x.Status == request.Status.Value);
        }

        var projected = q
            .OrderByDescending(x => x.PaymentDate)
            .ThenByDescending(x => x.Id)
            .Select(x => new ListPaymentsQueryDto
            {
                Id = x.Id,

                CustomerName = x.PaymentReservations
                    .OrderBy(link => link.ReservationId)
                    .Select(link =>
                        (
                            link.Reservation.CustomerFirstName +
                            " " +
                            link.Reservation.CustomerLastName
                        ).Trim())
                    .FirstOrDefault(),

                Reservations = x.PaymentReservations
                    .OrderBy(link => link.Reservation.Date)
                    .ThenBy(link => link.Reservation.StartTime)
                    .Select(link => new ListPaymentReservationDto
                    {
                        Id = link.Reservation.Id,

                        Date = link.Reservation.Date,

                        StartTime = link.Reservation.StartTime,

                        EndTime = link.Reservation.EndTime,

                        TrackName = link.Reservation.Track != null
                            ? link.Reservation.Track.Name
                            : string.Empty,

                        KartName = link.Reservation.Kart != null
                            ? link.Reservation.Kart.Name
                            : string.Empty
                    })
                    .ToList(),

                Amount = x.Amount,

                Currency = x.Currency,

                PaymentDate = x.PaymentDate,

                PaymentTypeName = x.PaymentType != null
                    ? x.PaymentType.Name
                    : null,

                Status = x.Status
            });

        return await PageResult<ListPaymentsQueryDto>
            .FromQueryableAsync(
                projected,
                request.Paging,
                ct);
    }
}