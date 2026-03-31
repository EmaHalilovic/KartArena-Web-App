using KartArena.Domain.Entities.Payments;
using Microsoft.EntityFrameworkCore;

namespace KartArena.Application.Modules.Catalog.Payments.Queries.GetById;

public sealed class GetPaymentByIdQueryHandler(IAppDbContext context)
    : IRequestHandler<GetPaymentByIdQuery, GetPaymentByIdQueryDto>
{
    public async Task<GetPaymentByIdQueryDto> Handle(GetPaymentByIdQuery request, CancellationToken cancellationToken)
    {
        var dto = await context.Payments
            .AsNoTracking()
            .Where(x => x.Id == request.Id && !x.IsDeleted)
            .Select(x => new GetPaymentByIdQueryDto
            {
                Id = x.Id,
                ReservationId = x.ReservationId,
                Amount = x.Amount,
                PaymentDate = x.PaymentDate,
                PaymentTypeId = x.PaymentTypeId,
                PaymentTypeName = x.PaymentType != null ? x.PaymentType.Name : null,
                Status = x.Status,
                TransactionReference = x.TransactionReference,
                Note = x.Note
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (dto is null)
            throw new MarketNotFoundException($"Payment with id {request.Id} was not found.");

        return dto;
    }
}