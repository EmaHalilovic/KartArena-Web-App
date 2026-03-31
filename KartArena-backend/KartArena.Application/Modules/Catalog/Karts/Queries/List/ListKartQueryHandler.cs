using KartArena.Application.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KartArena.Application.Modules.Catalog.Karts.Queries.List
{
    public sealed class ListKartsQueryHandler(IAppDbContext context)
        : IRequestHandler<ListKartsQuery, PageResult<ListKartsQueryDto>>
    {
        public async Task<PageResult<ListKartsQueryDto>> Handle(ListKartsQuery request, CancellationToken cancellationToken)
        {
            var query = context.Karts.AsQueryable();

            // Filtriranje po imenu
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.Trim().ToLower();
                query = query.Where(x => x.Name.ToLower().Contains(search));
            }

            // Filtriranje po statusu
            if (request.OnlyEnabled.HasValue && request.OnlyEnabled.Value)
            {
                query = query.Where(x => x.isEnabled);
            }

            // Projekcija na DTO
            var projectedQuery = query
                .OrderBy(x => x.Name)
                .Select(x => new ListKartsQueryDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Colour = x.Colour,
                    Manufacturer = x.Manufacturer,
                    ImageUrl = x.ImageUrl,
                    PricePerSession = x.PricePerSession,
                    Description = x.Description,
                    IsEnabled = x.isEnabled,
                });

            // Korištenje tvoje PageResult helper metode
            return await PageResult<ListKartsQueryDto>.FromQueryableAsync(
                projectedQuery,
                request.Paging,
                cancellationToken
            );
        }
    }
}

