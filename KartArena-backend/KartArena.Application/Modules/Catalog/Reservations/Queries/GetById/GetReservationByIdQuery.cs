namespace KartArena.Application.Modules.Catalog.Reservations.Queries.GetById;

public sealed record GetReservationByIdQuery(int Id) : IRequest<GetReservationByIdQueryDto>;
