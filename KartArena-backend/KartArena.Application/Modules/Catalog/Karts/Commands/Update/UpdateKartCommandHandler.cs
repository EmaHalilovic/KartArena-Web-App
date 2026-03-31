using KartArena.Domain.Entities.Catalog;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace KartArena.Application.Modules.Catalog.Karts.Commands.Update
{
    public class UpdateKartCommandHandler(IAppDbContext context)
        : IRequestHandler<UpdateKartCommand, Unit>
    {
        public async Task<Unit> Handle(UpdateKartCommand request, CancellationToken cancellationToken)
        {
            // Pronađi postojeći kart
            var kart = await context.Karts
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (kart == null)
                throw new System.ComponentModel.DataAnnotations.ValidationException($"Kart with ID {request.Id} was not found.");

            // Normalizuj ime
            var normalizedName = request.Name.Trim();

            if (string.IsNullOrWhiteSpace(normalizedName))
                throw new System.ComponentModel.DataAnnotations.ValidationException("Name is required.");

            // Provjera duplikata (da li postoji drugi kart sa istim imenom)
            bool exists = await context.Karts
                .AnyAsync(x => x.Id != request.Id && x.Name == normalizedName, cancellationToken);

            if (exists)
                throw new System.ComponentModel.DataAnnotations.ValidationException("Another kart with this name already exists.");

            // Ažuriranje vrijednosti
            kart.Name = normalizedName;
            kart.Colour = request.Colour?.Trim();
            kart.YearOfManufacture = request.YearOfManufacture;
            kart.ChassisNumber = request.ChassisNumber?.Trim();
            kart.Manufacturer = request.Manufacturer?.Trim();
            kart.ImageUrl = request.ImageUrl?.Trim();
            kart.PricePerSession = request.PricePerSession;
            kart.Description = request.Description?.Trim();
            kart.PowertrainTypeId = request.PowertrainTypeId;

            context.Karts.Update(kart);
            await context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
