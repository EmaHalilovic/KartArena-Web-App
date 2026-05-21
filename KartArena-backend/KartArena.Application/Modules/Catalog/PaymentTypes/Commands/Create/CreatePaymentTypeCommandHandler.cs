using KartArena.Domain.Entities.Payments;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.RegularExpressions;

namespace KartArena.Application.Modules.Catalog.PaymentTypes.Commands.Create;

public sealed class CreatePaymentTypeCommandHandler(IAppDbContext context)
    : IRequestHandler<CreatePaymentTypeCommand, int>
{
    public async Task<int> Handle(CreatePaymentTypeCommand request, CancellationToken cancellationToken)
    {
        var normalizedName = request.Name.Trim();

        var nameExists = await context.PaymentTypes
            .AnyAsync(
                x => x.Name != null && x.Name.ToLower() == normalizedName.ToLower(),
                cancellationToken);

        if (nameExists)
            throw new MarketConflictException("Payment type with the same name already exists.");


        var entity = new PaymentTypeEntity
        {
            Name = normalizedName,
            Code = request.Code,
            AllowedOnline = request.AllowedOnline,
            AllowedAtDesk = request.AllowedAtDesk,
            Description = request.Description?.Trim(),
            isEnabled = true
        };

        context.PaymentTypes.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }

   
}