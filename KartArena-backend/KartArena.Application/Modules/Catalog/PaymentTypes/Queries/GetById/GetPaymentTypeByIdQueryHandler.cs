using Microsoft.EntityFrameworkCore;

namespace KartArena.Application.Modules.Catalog.PaymentTypes.Queries.GetById;

public sealed class GetPaymentTypeByIdQueryHandler(IAppDbContext context)
    : IRequestHandler<GetPaymentTypeByIdQuery, GetPaymentTypeByIdQueryDto>
{
    public async Task<GetPaymentTypeByIdQueryDto> Handle(GetPaymentTypeByIdQuery request, CancellationToken cancellationToken)
    {
        var dto = await context.PaymentTypes
            .AsNoTracking()
            .Where(x => x.Id == request.Id && !x.IsDeleted)
            .Select(x => new GetPaymentTypeByIdQueryDto
            {
                Id = x.Id,
                Name = x.Name ?? string.Empty,
                Code = x.Code,
                AllowedOnline = x.AllowedOnline,
                AllowedAtDesk = x.AllowedAtDesk,
                Description = x.Description,
           
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (dto is null)
            throw new MarketNotFoundException($"Payment type with id {request.Id} was not found.");

        return dto;
    }
}