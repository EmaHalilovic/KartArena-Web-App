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

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            var name = request.Name.Trim();
            q = q.Where(x => x.Name != null && x.Name.Contains(name));
        }

        if (!string.IsNullOrWhiteSpace(request.Code))
        {
            var code = request.Code.Trim();
            q = q.Where(x => x.Code.Contains(code));
        }

        if (request.PaymentMethod?.Equals("online", StringComparison.OrdinalIgnoreCase) == true)
            q = q.Where(x => x.AllowedOnline);
        else if (request.PaymentMethod?.Equals("desk", StringComparison.OrdinalIgnoreCase) == true)
            q = q.Where(x => x.AllowedAtDesk);

        if (request.AllowedOnline.HasValue)
            q = q.Where(x => x.AllowedOnline == request.AllowedOnline.Value);

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
                IsEnabled=x.isEnabled
            });

        return await PageResult<ListPaymentTypesQueryDto>.FromQueryableAsync(projected, request.Paging, ct);
    }
}
