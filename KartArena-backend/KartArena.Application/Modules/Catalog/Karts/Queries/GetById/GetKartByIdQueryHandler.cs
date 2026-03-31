using KartArena.Domain.Entities.Catalog;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using ValidationException = System.ComponentModel.DataAnnotations.ValidationException;

namespace KartArena.Application.Modules.Catalog.Karts.Queries.GetById
{
    public class GetKartByIdQueryHandler(IAppDbContext context)
        : IRequestHandler<GetKartByIdQuery, GetKartByIdQueryDto>
    {
        public async Task<GetKartByIdQueryDto> Handle(GetKartByIdQuery request, CancellationToken cancellationToken)
        {
            var kart = await context.Karts
                .Include(x => x.PowertrainType)
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (kart == null)
                throw new ValidationException($"Kart with ID {request.Id} was not found.");

            var dto = new GetKartByIdQueryDto
            {
                Id = kart.Id,
                Name = kart.Name,
                Colour = kart.Colour,
                YearOfManufacture = kart.YearOfManufacture,
                ChassisNumber = kart.ChassisNumber,
                Manufacturer = kart.Manufacturer,
                ImageUrl = kart.ImageUrl,
                PricePerSession = kart.PricePerSession,
                Description = kart.Description,
                PowertrainTypeId = kart.PowertrainTypeId,
                PowertrainTypeName = kart.PowertrainType?.Name,
                IsEnabled = kart.isEnabled
            };

            return dto;
        }
    }
}
