namespace KartArena.Application.Modules.Catalog.PaymentTypes.Queries.List;

public sealed class ListPaymentTypesQueryHandler(IAppDbContext context)
    : IRequestHandler<ListPaymentTypesQuery, PageResult<ListPaymentTypesQueryDto>>
{
    public async Task<PageResult<ListPaymentTypesQueryDto>> Handle(ListPaymentTypesQuery request, CancellationToken ct)
    {
        var q = context.PaymentTypes.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            q = q.Where(x =>
                (x.Name != null && x.Name.Contains(search)) ||
                (x.Description != null && x.Description.Contains(search)));
        }

        if (request.OnlyEnabled.HasValue)
            q = q.Where(x => x.isEnabled == request.OnlyEnabled.Value);

        var projected = q
            .OrderBy(x => x.Name)
            .Select(x => new ListPaymentTypesQueryDto
            {
                Id = x.Id,
                Name = x.Name ?? string.Empty,
                Code = x.Code,
                AllowedOnline = x.AllowedOnline,
                AllowedAtDesk = x.AllowedAtDesk,
                Description = x.Description,
            });

        return await PageResult<ListPaymentTypesQueryDto>.FromQueryableAsync(projected, request.Paging, ct);
    }
}
