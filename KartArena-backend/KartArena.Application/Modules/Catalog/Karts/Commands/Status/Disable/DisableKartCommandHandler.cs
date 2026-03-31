using KartArena.Domain.Entities.Catalog;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace KartArena.Application.Modules.Catalog.Karts.Commands.Status.Disable
{
    public class DisableKartCommandHandler(IAppDbContext context)
        : IRequestHandler<DisableKartCommand, Unit>
    {
        public async Task<Unit> Handle(DisableKartCommand request, CancellationToken cancellationToken)
        {
            // Pronađi kart po ID-u
            var kart = await context.Karts
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (kart == null)
                throw new System.ComponentModel.DataAnnotations.ValidationException($"Kart with ID {request.Id} was not found.");

            // Postavi status na onemogućen
            kart.isEnabled = false;

            context.Karts.Update(kart);
            await context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
