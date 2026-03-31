namespace KartArena.Application.Modules.Catalog.Reservations.Queries.List;

public sealed class ListReservationsQueryHandler(IAppDbContext ctx)
    : IRequestHandler<ListReservationsQuery, PageResult<ListReservationsQueryDto>>
{
    public async Task<PageResult<ListReservationsQueryDto>> Handle(
        ListReservationsQuery request, CancellationToken ct)
    {
        var q = ctx.Reservations.AsNoTracking()
            .Where(x => !x.IsDeleted);

        // Search (prilagodi po čemu želiš tražiti)
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var s = request.Search.Trim();

            q = q.Where(x =>
                x.UserId.ToString().Contains(s) ||
                x.TrackId.ToString().Contains(s) ||
                x.KartId.ToString().Contains(s)
                || x.User.FirstName.Contains(s)
                || x.User.LastName.Contains(s)
                || x.Track.Name.Contains(s)
                || x.Kart.Name.Contains(s)
            );
        }

        var projectedQuery = q
            .OrderByDescending(x => x.StartTime)
            .Select(x => new ListReservationsQueryDto
            {
                Id = x.Id,
                UserId = x.UserId,
                TrackId = x.TrackId,
                KartId = x.KartId,
                Date=x.Date,
                StartTime = x.StartTime,
                EndTime = x.EndTime,
                UserFirstName = x.User.FirstName,
                UserLastName=x.User.LastName,
                TrackName = x.Track.Name,
                KartName = x.Kart.Name
            });

        return await PageResult<ListReservationsQueryDto>
            .FromQueryableAsync(projectedQuery, request.Paging, ct);
    }
}
