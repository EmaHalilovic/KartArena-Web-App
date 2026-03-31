namespace KartArena.Application.Modules.Catalog.Payments.Commands.Create;

public sealed class CreatePaymentCommand : IRequest<int>
{
    public int ReservationId { get; set; }

    public decimal Amount { get; set; }

    public DateTime? PaymentDate { get; set; }

    public int? PaymentTypeId { get; set; }

    public string? TransactionReference { get; set; }

    public string? Note { get; set; }
}