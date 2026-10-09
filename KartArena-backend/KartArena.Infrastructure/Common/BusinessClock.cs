using KartArena.Application.Abstractions;
using Microsoft.Extensions.Options;
using TimeZoneConverter;


namespace KartArena.Infrastructure.Common
{
    public sealed class BusinessClock : IBusinessClock
    {
        private readonly TimeZoneInfo _timeZone;

        public BusinessClock(IOptions<RegionalSettings> settings)
        {
            _timeZone = TZConvert.GetTimeZoneInfo(settings.Value.TimeZone);
        }

        public DateTime UtcNow => DateTime.UtcNow;

        public DateTime Now =>
            TimeZoneInfo.ConvertTimeFromUtc(
                DateTime.UtcNow,
                _timeZone);

        public DateTime Today => Now.Date;

        public DateTime ToBusinessTime(DateTime utcDateTime) =>
            TimeZoneInfo.ConvertTimeFromUtc(
                DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc),
                _timeZone);

        public DateTime ToUtc(DateTime businessLocalDateTime) =>
            TimeZoneInfo.ConvertTimeToUtc(
                DateTime.SpecifyKind(businessLocalDateTime, DateTimeKind.Unspecified),
                _timeZone);
    }
}
