using KartArena.Domain.Entities.Catalog;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace KartArena.Application.Modules.Catalog.Karts.Commands.Delete
{
    public class DeleteKartCommandHandler(IAppDbContext context)
        : IRequestHandler<DeleteKartCommand, Unit>
    {
        public async Task<Unit> Handle(DeleteKartCommand request, CancellationToken cancellationToken)
        {
            // Pronađi kart prema ID-u
            var kart = await context.Karts
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (kart == null)
                throw new System.ComponentModel.DataAnnotations.ValidationException($"Kart with ID {request.Id} was not found.");

            // Brišemo kart
            context.Karts.Remove(kart);
            await context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
