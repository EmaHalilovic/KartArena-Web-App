using MediatR;

namespace KartArena.Application.Modules.Catalog.Reservations.Queries.Availability;

public sealed class GetReservationAvailabilityQuery : IRequest<GetReservationAvailabilityDto>
{
    public DateTime Date { get; set; }
    public int Duration { get; set; }
}