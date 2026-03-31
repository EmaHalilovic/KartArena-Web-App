namespace KartArena.Application.Modules.Catalog.Payments.Queries.GetById;

public sealed record GetPaymentByIdQuery(int Id) : IRequest<GetPaymentByIdQueryDto>;
