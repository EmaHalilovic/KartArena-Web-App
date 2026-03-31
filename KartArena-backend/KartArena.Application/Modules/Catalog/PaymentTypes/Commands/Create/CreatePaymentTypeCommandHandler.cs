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

        var generatedCode = await GenerateUniqueCodeAsync(normalizedName, cancellationToken);

        var entity = new PaymentTypeEntity
        {
            Name = normalizedName,
            Code = generatedCode,
            AllowedOnline = request.AllowedOnline,
            AllowedAtDesk = request.AllowedAtDesk,
            Description = request.Description?.Trim()
        };

        context.PaymentTypes.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }

    private async Task<string> GenerateUniqueCodeAsync(string name, CancellationToken cancellationToken)
    {
        var baseCode = GenerateCode(name);
        var finalCode = baseCode;
        var counter = 1;

        while (await context.PaymentTypes.AnyAsync(
                   x => x.Code.ToLower() == finalCode.ToLower(),
                   cancellationToken))
        {
            finalCode = $"{baseCode}_{counter}";
            counter++;
        }

        return finalCode;
    }

    private static string GenerateCode(string name)
    {
        var trimmed = name.Trim().ToUpperInvariant();

        trimmed = Regex.Replace(trimmed, @"[^A-Z0-9\s]", "");
        trimmed = Regex.Replace(trimmed, @"\s+", "_");

        return string.IsNullOrWhiteSpace(trimmed) ? "PAYMENT_TYPE" : trimmed;
    }
}