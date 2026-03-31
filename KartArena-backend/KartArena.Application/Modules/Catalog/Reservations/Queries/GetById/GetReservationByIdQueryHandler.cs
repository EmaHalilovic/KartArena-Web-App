using KartArena.Application.Modules.Catalog.Equipment.Queries.GetById;

namespace KartArena.Application.Modules.Catalog.Reservations.Queries.GetById;

public class GetReservationByIdQueryHandler(IAppDbContext ctx) : IRequestHandler<GetReservationByIdQuery, GetReservationByIdQueryDto>
{
    public async Task<GetReservationByIdQueryDto> Handle(GetReservationByIdQuery request, CancellationToken cancellationToken)
    {
        var dto = await ctx.Reservations
                 .AsNoTracking()
                 .Where(r => !r.IsDeleted && r.Id == request.Id)
                 .Select(r => new GetReservationByIdQueryDto
                 {
                     Id = r.Id,
                     UserId = r.UserId,
                     TrackId = r.TrackId,
                     KartId = r.KartId,
                     Date = r.Date,
                     StartTime = r.StartTime,
                     EndTime = r.EndTime,

                     UserFirstName = r.User != null ? r.User.FirstName : null,
                     UserLastName = r.User != null ? r.User.LastName : null,
                     TrackName = r.Track != null ? r.Track.Name : null,
                     KartName = r.Kart != null ? r.Kart.Name : null,
                 })
                 .FirstOrDefaultAsync(cancellationToken);

        if (dto is null)
            throw new Exception("Reservation not found.");

        return dto;
    }
}