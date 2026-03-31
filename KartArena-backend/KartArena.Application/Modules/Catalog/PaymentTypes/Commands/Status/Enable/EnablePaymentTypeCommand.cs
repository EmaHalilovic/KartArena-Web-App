namespace KartArena.Application.Modules.Catalog.PaymentTypes.Commands.Status.Enable;

public sealed class EnablePaymentTypeCommand(int id) : IRequest<Unit>
{
    public int Id { get; } = id;
}
