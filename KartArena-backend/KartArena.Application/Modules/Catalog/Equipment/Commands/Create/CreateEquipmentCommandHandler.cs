using KartArena.Domain.Entities.Equipment;

namespace KartArena.Application.Modules.Catalog.Equipment.Commands.Create;

public class CreateEquipmentCommandHandler(IAppDbContext context)
    : IRequestHandler<CreateEquipmentCommand, int>
{
    public async Task<int> Handle(CreateEquipmentCommand request, CancellationToken cancellationToken)
    {
        var normalizedName = request.Name?.Trim();
        var normalizedSize = request.Size?.Trim();

        if (string.IsNullOrWhiteSpace(normalizedName))
            throw new ValidationException("Name is required.");

        if (string.IsNullOrWhiteSpace(normalizedSize))
            throw new ValidationException("Size is required.");

        var exists = await context.EquipmentEntity
            .AnyAsync(x => x.Name == normalizedName
                        && x.Size == normalizedSize
                        && x.Category == request.Category,
                cancellationToken);

        if (exists)
            throw new MarketConflictException("Oprema za odabranu kategoriju i veličinu već postoji.");

        var equipmentType = new EquipmentTypeEntity
        {
            Name = normalizedName,
            Category = request.Category,
            Price = request.Price,
            Size = normalizedSize,
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            isEnabled = true
        };

        context.EquipmentEntity.Add(equipmentType);
        await context.SaveChangesAsync(cancellationToken);

        return equipmentType.Id;
    }
}
