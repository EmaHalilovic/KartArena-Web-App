using KartArena.Domain.Entities.Catalog;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KartArena.Application.Modules.Catalog.Karts.Queries.GetAll
{
    public class GetAllKartsQueryHandler(IAppDbContext context)
        : IRequestHandler<GetAllKartsQuery, GetAllKartsQueryResponse>
    {
        public async Task<GetAllKartsQueryResponse> Handle(GetAllKartsQuery request, CancellationToken cancellationToken)
        {
            var query = context.Karts
                .Include(x => x.PowertrainType)
                .AsQueryable();

            
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var searchTerm = request.Search.Trim().ToLower();
                query = query.Where(x => x.Name.ToLower().Contains(searchTerm));
            }

           
            if (request.IsEnabled.HasValue)
            {
                query = query.Where(x => x.isEnabled == request.IsEnabled.Value);
            }

           
            var totalCount = await query.CountAsync(cancellationToken);

          
            var items = await query
                .OrderBy(x => x.Name)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(x => new GetAllKartsQueryDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Colour = x.Colour,
                    YearOfManufacture = x.YearOfManufacture,
                    ChassisNumber = x.ChassisNumber,
                    Manufacturer = x.Manufacturer,
                    ImageUrl = x.ImageUrl,
                    PricePerSession = x.PricePerSession,
                    Description = x.Description,
                    PowertrainTypeName = x.PowertrainType != null ? x.PowertrainType.Name : null,
                    IsEnabled = x.isEnabled
                })
                .ToListAsync(cancellationToken);

            return new GetAllKartsQueryResponse
            {
                TotalCount = totalCount,
                Items = items
            };
        }
    }
}
