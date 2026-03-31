using KartArena.Domain.Entities.Catalog;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using ValidationException = System.ComponentModel.DataAnnotations.ValidationException;

namespace KartArena.Application.Modules.Catalog.Karts.Commands.Create
{
    public class CreateKartCommandHandler(IAppDbContext context)
        : IRequestHandler<CreateKartCommand, int>
    {
        public async Task<int> Handle(CreateKartCommand request, CancellationToken cancellationToken)
        {
            var normalizedName = request.Name?.Trim();

            if (string.IsNullOrWhiteSpace(normalizedName))
                throw new ValidationException("Name is required.");

            // provjera da li već postoji kart s istim imenom
            bool exists = await context.Karts
                .AnyAsync(x => x.Name == normalizedName, cancellationToken);

            if (exists)
                throw new ValidationException("Kart with this name already exists.");

            var kart = new KartEntity
            {
                Name = normalizedName,
                Colour = request.Colour?.Trim(),
                YearOfManufacture = request.YearOfManufacture,
                ChassisNumber = request.ChassisNumber?.Trim(),
                Manufacturer = request.Manufacturer?.Trim(),
                ImageUrl = request.ImageUrl?.Trim(),
                PricePerSession = request.PricePerSession,
                Description = request.Description?.Trim(),
                PowertrainTypeId = request.PowertrainTypeId
            };

            context.Karts.Add(kart);
            await context.SaveChangesAsync(cancellationToken);

            return kart.Id;
        }
    }
}
