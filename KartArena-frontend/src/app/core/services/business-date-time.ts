export const BUSINESS_CULTURE = 'bs-BA';
export const BUSINESS_TIME_ZONE = 'Europe/Sarajevo';

const businessDateFormatter = new Intl.DateTimeFormat('en-CA', {
  timeZone: BUSINESS_TIME_ZONE,
  year: 'numeric',
  month: '2-digit',
  day: '2-digit',
});

const businessDateTimeFormatter = new Intl.DateTimeFormat('en-CA', {
  timeZone: BUSINESS_TIME_ZONE,
  year: 'numeric',
  month: '2-digit',
  day: '2-digit',
  hour: '2-digit',
  minute: '2-digit',
  second: '2-digit',
  hourCycle: 'h23',
});

const businessDisplayDateTimeFormatter = new Intl.DateTimeFormat(BUSINESS_CULTURE, {
  timeZone: BUSINESS_TIME_ZONE,
  year: 'numeric',
  month: '2-digit',
  day: '2-digit',
  hour: '2-digit',
  minute: '2-digit',
  second: '2-digit',
  hourCycle: 'h23',
});

/** Returns the current business date independently of the browser's time zone. */
export function getBusinessDateKey(now: Date = new Date()): string {
  const parts = businessDateFormatter.formatToParts(now);
  const value = (type: Intl.DateTimeFormatPartTypes) =>
    parts.find((part) => part.type === type)?.value ?? '';

  return `${value('year')}-${value('month')}-${value('day')}`;
}

/** Converts a business calendar date to a Date suitable for Angular date pickers. */
export function getBusinessToday(now: Date = new Date()): Date {
  const [year, month, day] = getBusinessDateKey(now).split('-').map(Number);
  return new Date(year, month - 1, day);
}

/** Returns a lexicographically sortable business-local date-time key. */
export function getBusinessDateTimeKey(now: Date = new Date()): string {
  const parts = businessDateTimeFormatter.formatToParts(now);
  const value = (type: Intl.DateTimeFormatPartTypes) =>
    parts.find((part) => part.type === type)?.value ?? '';

  return `${value('year')}-${value('month')}-${value('day')}T${value('hour')}:${value('minute')}:${value('second')}`;
}

/** Combines API date and time values into a sortable business-local date-time key. */
export function toBusinessDateTimeKey(dateValue: string, timeValue: string): string | null {
  const date = dateValue.trim().split('T')[0];
  const rawTime = timeValue.trim().includes('T')
    ? timeValue.trim().split('T')[1] ?? ''
    : timeValue.trim();
  const time = rawTime.split(/[.+Z-]/)[0];
  const match = time.match(/^(\d{2}):(\d{2})(?::(\d{2}))?$/);

  if (!/^\d{4}-\d{2}-\d{2}$/.test(date) || !match) {
    return null;
  }

  return `${date}T${match[1]}:${match[2]}:${match[3] ?? '00'}`;
}

/** Returns a calendar difference between two business-local keys without using the browser time zone. */
export function differenceBetweenBusinessDateTimeKeys(later: string, earlier: string): number | null {
  const toCalendarMilliseconds = (value: string): number | null => {
    const match = value.match(/^(\d{4})-(\d{2})-(\d{2})T(\d{2}):(\d{2}):(\d{2})$/);
    if (!match) return null;

    const [, year, month, day, hour, minute, second] = match.map(Number);
    return Date.UTC(year, month - 1, day, hour, minute, second);
  };

  const laterValue = toCalendarMilliseconds(later);
  const earlierValue = toCalendarMilliseconds(earlier);
  return laterValue == null || earlierValue == null ? null : laterValue - earlierValue;
}

/** Formats an absolute timestamp in the configured business culture and time zone. */
export function formatBusinessInstant(value: string | Date): string | null {
  // SQL Server DateTime values can arrive without an offset even when they
  // represent UTC. Absolute timestamps handled by this function are therefore
  // normalized to UTC before being converted to the business time zone.
  const normalizedValue =
    typeof value === 'string' &&
    /^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}(?::\d{2}(?:\.\d+)?)?$/.test(value)
      ? `${value}Z`
      : value;
  const date = normalizedValue instanceof Date ? normalizedValue : new Date(normalizedValue);
  return Number.isNaN(date.getTime()) ? null : businessDisplayDateTimeFormatter.format(date);
}

/** Formats a date-picker value as a date-only API value without UTC conversion. */
export function formatLocalCalendarDate(date: Date): string {
  const year = date.getFullYear();
  const month = `${date.getMonth() + 1}`.padStart(2, '0');
  const day = `${date.getDate()}`.padStart(2, '0');

  return `${year}-${month}-${day}`;
}

export function addDaysToDateKey(dateKey: string, days: number): string {
  const [year, month, day] = dateKey.split('-').map(Number);
  const date = new Date(Date.UTC(year, month - 1, day + days));
  return date.toISOString().slice(0, 10);
}
