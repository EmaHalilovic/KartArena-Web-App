using System.Text.Json.Serialization;

namespace KartArena.Application.Modules.Catalog.PaymentTypes.Commands.Update;

public sealed class UpdatePaymentTypeCommand : IRequest<Unit>
{
    [JsonIgnore]
    public int Id { get; set; }

    public required string Name { get; set; }

    public bool AllowedOnline { get; set; }

    public bool AllowedAtDesk { get; set; }

    public string? Description { get; set; }
}