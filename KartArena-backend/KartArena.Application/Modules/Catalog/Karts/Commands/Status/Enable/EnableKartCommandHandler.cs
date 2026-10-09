using KartArena.Domain.Entities.Catalog;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace KartArena.Application.Modules.Catalog.Karts.Commands.Status.Enable
{
    public class EnableKartCommandHandler(IAppDbContext context)
        : IRequestHandler<EnableKartCommand, Unit>
    {
        public async Task<Unit> Handle(EnableKartCommand request, CancellationToken cancellationToken)
        {
           
            var kart = await context.Karts
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (kart == null)
                throw new System.ComponentModel.DataAnnotations.ValidationException($"Kart with ID {request.Id} was not found.");

           
            kart.isEnabled = true;

            context.Karts.Update(kart);
            await context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
