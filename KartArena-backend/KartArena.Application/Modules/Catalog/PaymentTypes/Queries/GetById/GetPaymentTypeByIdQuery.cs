namespace KartArena.Application.Modules.Catalog.PaymentTypes.Queries.GetById;

public sealed record GetPaymentTypeByIdQuery(int Id) : IRequest<GetPaymentTypeByIdQueryDto>;
