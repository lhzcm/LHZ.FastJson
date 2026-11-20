using System;

using System.Text;

namespace LHZ.FastJson.Utils
{
    /// <summary>
    /// Allocation-free ISO 8601 DateTime writer and parser used by the serializer and deserializer.
    /// </summary>
    internal static class DateTimeFormatter
    {
        /// <summary>
        /// Maximum length of the round-trip format: yyyy-MM-ddTHH:mm:ss.fffffff+hh:mm
        /// </summary>
        internal const int MaxIsoLength = 33;

        private const long TicksPerSecond = TimeSpan.TicksPerSecond;
        private const long TicksPerMinute = TimeSpan.TicksPerMinute;

        /// <summary>
        /// Write <paramref name="value"/> as a quoted ISO 8601 round-trip string
        /// (the same shape System.Text.Json and Newtonsoft.Json produce):
        /// <list type="bullet">
        /// <item><description>Utc: <c>"2024-01-02T03:04:05.678Z"</c></description></item>
        /// <item><description>Local: <c>"2024-01-02T03:04:05.678+08:00"</c></description></item>
        /// <item><description>Unspecified: <c>"2024-01-02T03:04:05.678"</c></description></item>
        /// </list>
        /// Fractional seconds are written with up to 7 digits; trailing zeros are omitted.
        /// </summary>
        /// <param name="sb">Destination</param>
        /// <param name="value">Value to write</param>
        /// <param name="buffer">Scratch buffer of at least <see cref="MaxIsoLength"/> + 2 characters</param>
        internal static void WriteIsoQuoted(StringBuilder sb, DateTime value, char[] buffer)
        {
            int pos = 0;
            buffer[pos++] = '"';
            int year = value.Year;
            buffer[pos++] = (char)('0' + year / 1000);
            buffer[pos++] = (char)('0' + year / 100 % 10);
            buffer[pos++] = (char)('0' + year / 10 % 10);
            buffer[pos++] = (char)('0' + year % 10);
            buffer[pos++] = '-';
            pos = Write2(buffer, pos, value.Month);
            buffer[pos++] = '-';
            pos = Write2(buffer, pos, value.Day);
            buffer[pos++] = 'T';
            pos = Write2(buffer, pos, value.Hour);
            buffer[pos++] = ':';
            pos = Write2(buffer, pos, value.Minute);
            buffer[pos++] = ':';
            pos = Write2(buffer, pos, value.Second);

            int fraction = (int)(value.Ticks % TicksPerSecond);
            if (fraction != 0)
            {
                buffer[pos++] = '.';
                int digits = 7;
                while (fraction % 10 == 0)
                {
                    fraction /= 10;
                    digits--;
                }
                for (int i = pos + digits - 1; i >= pos; i--)
                {
                    buffer[i] = (char)('0' + fraction % 10);
                    fraction /= 10;
                }
                pos += digits;
            }

            switch (value.Kind)
            {
                case DateTimeKind.Utc:
                    buffer[pos++] = 'Z';
                    break;
                case DateTimeKind.Local:
                    {
                        int offsetMinutes = (int)(TimeZoneInfo.Local.GetUtcOffset(value).Ticks / TicksPerMinute);
                        if (offsetMinutes < 0)
                        {
                            buffer[pos++] = '-';
                            offsetMinutes = -offsetMinutes;
                        }
                        else
                        {
                            buffer[pos++] = '+';
                        }
                        pos = Write2(buffer, pos, offsetMinutes / 60);
                        buffer[pos++] = ':';
                        pos = Write2(buffer, pos, offsetMinutes % 60);
                        break;
                    }
            }
            buffer[pos++] = '"';
            sb.Append(buffer, 0, pos);
        }

        private static int Write2(char[] buffer, int pos, int value)
        {
            buffer[pos] = (char)('0' + value / 10);
            buffer[pos + 1] = (char)('0' + value % 10);
            return pos + 2;
        }

        /// <summary>
        /// Fast ISO 8601 parser. Accepts:
        /// <c>yyyy-MM-dd</c>, optionally followed by 'T' or ' ' and <c>HH:mm[:ss[.fffffff]]</c>,
        /// optionally followed by <c>Z</c> or <c>±hh[:mm]</c> / <c>±hhmm</c>.
        /// Fractional digits beyond 7 are truncated.
        /// </summary>
        internal static bool TryParseIso(string s, int offset, int length, out DateTime result)
        {
            result = default(DateTime);
            int pos = offset;
            int end = offset + length;
            int year, month, day, hour = 0, minute = 0, second = 0;
            long fraction = 0;

            if (length < 10
                || !TryRead4(s, pos, out year) || s[pos + 4] != '-'
                || !TryRead2(s, pos + 5, out month) || s[pos + 7] != '-'
                || !TryRead2(s, pos + 8, out day))
            {
                return false;
            }
            pos += 10;
            if (year < 1 || month < 1 || month > 12 || day < 1 || day > DateTime.DaysInMonth(year, month))
            {
                return false;
            }

            if (pos < end)
            {
                char sep = s[pos];
                if (sep != 'T' && sep != 't' && sep != ' ')
                {
                    return false;
                }
                pos++;
                if (pos + 5 > end || !TryRead2(s, pos, out hour) || s[pos + 2] != ':' || !TryRead2(s, pos + 3, out minute))
                {
                    return false;
                }
                pos += 5;
                if (pos < end && s[pos] == ':')
                {
                    if (pos + 3 > end || !TryRead2(s, pos + 1, out second))
                    {
                        return false;
                    }
                    pos += 3;
                    if (pos < end && (s[pos] == '.' || s[pos] == ','))
                    {
                        pos++;
                        int digits = 0;
                        while (pos < end && IsDigit(s[pos]))
                        {
                            if (digits < 7)
                            {
                                fraction = fraction * 10 + (s[pos] - '0');
                                digits++;
                            }
                            pos++;
                        }
                        if (digits == 0)
                        {
                            return false;
                        }
                        for (; digits < 7; digits++)
                        {
                            fraction *= 10;
                        }
                    }
                }
                if (hour > 23 || minute > 59 || second > 59)
                {
                    return false;
                }
            }

            long ticks = new DateTime(year, month, day, hour, minute, second).Ticks + fraction;

            if (pos == end)
            {
                result = new DateTime(ticks, DateTimeKind.Unspecified);
                return true;
            }

            char zone = s[pos];
            if ((zone == 'Z' || zone == 'z') && pos + 1 == end)
            {
                result = new DateTime(ticks, DateTimeKind.Utc);
                return true;
            }
            if (zone != '+' && zone != '-')
            {
                return false;
            }
            pos++;
            int offsetHours, offsetMinutes = 0;
            if (pos + 2 > end || !TryRead2(s, pos, out offsetHours))
            {
                return false;
            }
            pos += 2;
            if (pos < end)
            {
                if (s[pos] == ':')
                {
                    pos++;
                }
                if (pos + 2 != end || !TryRead2(s, pos, out offsetMinutes))
                {
                    return false;
                }
                pos += 2;
            }
            if (offsetHours > 14 || offsetMinutes > 59)
            {
                return false;
            }
            long offsetTicks = (offsetHours * 60L + offsetMinutes) * TicksPerMinute;
            long utcTicks = zone == '+' ? ticks - offsetTicks : ticks + offsetTicks;
            if (utcTicks < DateTime.MinValue.Ticks || utcTicks > DateTime.MaxValue.Ticks)
            {
                return false;
            }
            //An explicit offset is converted to local time (same as DateTimeStyles.RoundtripKind)
            result = new DateTime(utcTicks, DateTimeKind.Utc).ToLocalTime();
            return true;
        }

        private static bool IsDigit(char c)
        {
            return c >= '0' && c <= '9';
        }

        private static bool TryRead2(string s, int pos, out int value)
        {
            char a = s[pos], b = s[pos + 1];
            if (!IsDigit(a) || !IsDigit(b))
            {
                value = 0;
                return false;
            }
            value = (a - '0') * 10 + (b - '0');
            return true;
        }

        private static bool TryRead4(string s, int pos, out int value)
        {
            int hi, lo;
            if (!TryRead2(s, pos, out hi) || !TryRead2(s, pos + 2, out lo))
            {
                value = 0;
                return false;
            }
            value = hi * 100 + lo;
            return true;
        }
    }
}
