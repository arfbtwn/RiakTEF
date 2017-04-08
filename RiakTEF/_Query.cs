using System;

namespace RiakTEF
{
    public static class _Query
    {
        public static bool InRange(this DateTimeOffset dateTime, DateTimeOffset start, DateTimeOffset end)
        {
            return start < dateTime && dateTime <= end;
        }

        public static bool InRange(this DateTime dateTime, DateTime start, DateTime end)
        {
            return InRange((DateTimeOffset) dateTime, start, end);
        }

        public static bool InRange(this DateTimeOffset dateTime, DateTimeOffset end, TimeSpan range)
        {
            return end - range < dateTime && dateTime <= end;
        }

        public static bool InRange(this DateTime dateTime, DateTime end, TimeSpan range)
        {
            return InRange((DateTimeOffset) dateTime, end, range);
        }
    }
}