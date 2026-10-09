

namespace KartArena.Application.Abstractions
{
    public interface IBusinessClock
    {
        DateTime UtcNow { get; }
        DateTime Now { get; }
        DateTime Today { get; }
        DateTime ToBusinessTime(DateTime utcDateTime);
        DateTime ToUtc(DateTime businessLocalDateTime);
    }
}
