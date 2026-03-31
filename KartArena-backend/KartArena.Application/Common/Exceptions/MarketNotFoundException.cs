namespace KartArena.Application.Common.Exceptions;

public sealed class MarketNotFoundException : Exception
{
    public MarketNotFoundException(string message) : base(message) { }
}
