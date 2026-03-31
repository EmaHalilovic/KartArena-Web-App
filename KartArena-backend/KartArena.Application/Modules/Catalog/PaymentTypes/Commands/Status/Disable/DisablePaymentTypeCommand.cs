namespace KartArena.Application.Modules.Catalog.PaymentTypes.Commands.Status.Disable;

public sealed class DisablePaymentTypeCommand(int id) : IRequest<Unit>
{
    public int Id { get; } = id;
}
