using KartArena.Domain.Common;

namespace KartArena.Domain.Entities.Payments;

public sealed class PaymentAttemptEntity : BaseEntity
{
    public int PaymentId { get; set; }
    public PaymentEntity Payment { get; set; } = null!;

    public string IdempotencyKey { get; set; } = null!;
    public string? StripeCheckoutSessionId { get; set; }
    public string? CheckoutUrl { get; set; }
    public string? StripeEventId { get; set; }
    public PaymentAttemptStatus Status { get; set; }
    public bool IsActive { get; set; }
}
