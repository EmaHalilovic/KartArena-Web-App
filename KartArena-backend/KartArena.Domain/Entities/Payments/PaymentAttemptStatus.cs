namespace KartArena.Domain.Entities.Payments;

public enum PaymentAttemptStatus
{
    Creating = 0,
    Pending = 1,
    Paid = 2,
    Failed = 3,
    Expired = 4,
    Cancelled = 5,
    Refunded = 6
}
