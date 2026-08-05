using KartArena.Domain.Entities.Payments;

namespace KartArena.Application.Modules.Catalog.Payments.Queries.List;

public sealed class ListPaymentsQueryDto
{
    public int Id { get; init; }

    public string? CustomerName { get; init; }

    public List<ListPaymentReservationDto> Reservations { get; init; }
        = new();

    public decimal Amount { get; init; }

    public string Currency { get; init; } = string.Empty;

    public DateTime? PaymentDate { get; init; }

    public string? PaymentTypeName { get; init; }

    public PaymentStatus Status { get; init; }
}