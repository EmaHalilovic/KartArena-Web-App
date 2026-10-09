namespace KartArena.Infrastructure.Common
{
    public sealed class RegionalSettings
    {
        public const string SectionName = "RegionalSettings";

        public string Culture { get; init; } = "bs-BA";
        public string TimeZone { get; init; } = "Europe/Sarajevo";
    }
}
