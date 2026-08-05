using MediatR;

namespace KartArena.Application.Modules.Catalog.Reservations.Commands.Checkout;

public sealed class CheckoutReservationsCommand : IRequest<List<int>>
{
    public int? UserId { get; set; }

    public string CustomerFirstName { get; set; } = string.Empty;
    public string CustomerLastName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string? CustomerNote { get; set; }

    public int? PaymentTypeId { get; set; }
    public decimal? Amount { get; set; }
    public string? PaymentNote { get; set; }

    public List<CheckoutReservationItemDto> Items { get; set; } = new();
}

public sealed class CheckoutReservationItemDto
{
    public DateTime ReservationDate { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }

    public int TrackId { get; set; }
    public int KartId { get; set; }
}
