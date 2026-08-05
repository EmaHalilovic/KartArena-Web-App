using KartArena.Domain.Entities.Payments;

namespace KartArena.Application.Modules.Catalog.Payments.Queries.GetById;

public sealed class GetPaymentByIdQueryDto
{
    public int Id { get; init; }

    public List<GetPaymentReservationDto> Reservations { get; init; }
        = new();

    public decimal Amount { get; init; }

    public string Currency { get; init; }
        = string.Empty;

    public DateTime? PaymentDate { get; init; }

    public int? PaymentTypeId { get; init; }

    public string? PaymentTypeName { get; init; }

    public PaymentStatus Status { get; init; }

    public string? TransactionReference { get; init; }

    public string? Note { get; init; }
}