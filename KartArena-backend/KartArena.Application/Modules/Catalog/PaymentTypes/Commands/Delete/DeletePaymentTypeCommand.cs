namespace KartArena.Application.Modules.Catalog.PaymentTypes.Commands.Delete;

public sealed class DeletePaymentTypeCommand(int id) : IRequest<Unit>
{
    public int Id { get; } = id;
}
