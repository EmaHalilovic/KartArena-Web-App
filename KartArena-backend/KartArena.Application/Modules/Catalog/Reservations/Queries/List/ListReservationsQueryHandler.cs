using Microsoft.EntityFrameworkCore;

namespace KartArena.Application.Modules.Catalog.Reservations.Queries.List
{
    public sealed class ListReservationsQueryHandler(IAppDbContext ctx)
        : IRequestHandler<ListReservationsQuery, PageResult<ListReservationsQueryDto>>
    {
        public async Task<PageResult<ListReservationsQueryDto>> Handle(
            ListReservationsQuery request,
            CancellationToken ct)
        {
            var q = ctx.Reservations
                .AsNoTracking()
                .Where(x => !x.IsDeleted);

            if (request.UserId.HasValue)
                q = q.Where(x => x.UserId == request.UserId.Value);

            if (request.TrackId.HasValue)
                q = q.Where(x => x.TrackId == request.TrackId.Value);

            if (request.KartId.HasValue)
                q = q.Where(x => x.KartId == request.KartId.Value);

            if (request.Status.HasValue)
                q = q.Where(x => x.Status == request.Status.Value);

            if (request.PaymentStatus.HasValue)
                q = q.Where(x => x.PaymentStatus == request.PaymentStatus.Value);

            if (request.DateFrom.HasValue)
            {
                var from = request.DateFrom.Value.Date;
                q = q.Where(x => x.Date >= from);
            }

            if (request.DateTo.HasValue)
            {
                var to = request.DateTo.Value.Date;
                q = q.Where(x => x.Date <= to);
            }

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var s = request.Search.Trim();

                q = q.Where(x =>
                    x.Id.ToString().Contains(s) ||
                    x.UserId.ToString().Contains(s) ||
                    x.TrackId.ToString().Contains(s) ||
                    x.KartId.ToString().Contains(s) ||
                    (x.User != null && (
                        x.User.FirstName.Contains(s) ||
                        x.User.LastName.Contains(s))) ||
                    (x.Track != null && x.Track.Name.Contains(s)) ||
                    (x.Kart != null && x.Kart.Name.Contains(s)) ||
                    (x.Payment != null && x.Payment.PaymentType != null && x.Payment.PaymentType.Name.Contains(s))
                );
            }

            var projectedQuery = q
                .OrderByDescending(x => x.Date)
                .ThenByDescending(x => x.StartTime)
                .Select(x => new ListReservationsQueryDto
                {
                    Id = x.Id,
                    UserId = x.UserId,
                    TrackId = x.TrackId,
                    KartId = x.KartId,
                    Date = x.Date,
                    StartTime = x.StartTime,
                    EndTime = x.EndTime,
                    Status = x.Status,
                    PaymentStatus = x.PaymentStatus,
                    PaymentAmount = x.Payment != null ? x.Payment.Amount : null,
                    PaymentTypeName = x.Payment != null && x.Payment.PaymentType != null
                        ? x.Payment.PaymentType.Name
                        : null,
                    UserFirstName = x.User != null ? x.User.FirstName : null,
                    UserLastName = x.User != null ? x.User.LastName : null,
                    TrackName = x.Track != null ? x.Track.Name : null,
                    KartName = x.Kart != null ? x.Kart.Name : null
                });

            return await PageResult<ListReservationsQueryDto>
                .FromQueryableAsync(projectedQuery, request.Paging, ct);
        }
    }
}