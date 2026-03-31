namespace KartArena.Application.Modules.Catalog.PaymentTypes.Commands.Create;

public sealed class CreatePaymentTypeCommand : IRequest<int>
{
    public required string Name { get; set; }


    public bool AllowedOnline { get; set; }

    public bool AllowedAtDesk { get; set; }

    public string? Description { get; set; }
}