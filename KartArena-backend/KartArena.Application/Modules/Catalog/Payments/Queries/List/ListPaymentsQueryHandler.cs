using KartArena.Domain.Entities.Payments;
using Microsoft.EntityFrameworkCore;

namespace KartArena.Application.Modules.Catalog.Payments.Queries.List;

public sealed class ListPaymentsQueryHandler(IAppDbContext context)
    : IRequestHandler<ListPaymentsQuery, PageResult<ListPaymentsQueryDto>>
{
    public async Task<PageResult<ListPaymentsQueryDto>> Handle(ListPaymentsQuery request, CancellationToken ct)
    {
        var q = context.Payments
            .AsNoTracking()
            .Where(x => !x.IsDeleted);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();

            q = q.Where(x =>
                x.ReservationId.ToString().Contains(search) ||
                x.Reservation.User.FirstName.Contains(search) ||
                x.Reservation.User.LastName.Contains(search) ||
                x.Amount.ToString().Contains(search) ||
                (x.TransactionReference != null && x.TransactionReference.Contains(search)) ||
                (x.Note != null && x.Note.Contains(search)) ||
                (x.PaymentType != null && x.PaymentType.Name != null && x.PaymentType.Name.Contains(search)));
                        }

        if (request.ReservationId.HasValue)
            q = q.Where(x => x.ReservationId == request.ReservationId.Value);

        if (request.PaymentTypeId.HasValue)
            q = q.Where(x => x.PaymentTypeId == request.PaymentTypeId.Value);

        if (request.Status.HasValue)
            q = q.Where(x => x.Status == request.Status.Value);

        var projected = q
            .OrderByDescending(x => x.PaymentDate)
            .ThenByDescending(x => x.Id)
            .Select(x => new ListPaymentsQueryDto
            {
                Id = x.Id,
                CustomerName=x.Reservation.User.FirstName+" "+x.Reservation.User.LastName,
                ReservationDate = x.Reservation.Date.ToShortDateString()+" "+x.Reservation.StartTime.ToShortTimeString()+"-"+x.Reservation.EndTime.ToShortTimeString(),
                Amount = x.Amount,
                PaymentDate = x.PaymentDate,
                PaymentTypeName = x.PaymentType != null ? x.PaymentType.Name : null,
                Status = x.Status,
            });

        return await PageResult<ListPaymentsQueryDto>.FromQueryableAsync(projected, request.Paging, ct);
    }
}