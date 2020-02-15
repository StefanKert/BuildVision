using System;

namespace BuildVision.Common.Extensions
{
    public static class DateTimeExtensions
    {
        public static DateTime Truncate(this DateTime dateTime, TimeSpan timeSpan)
        {
            return timeSpan <= TimeSpan.Zero ? dateTime : dateTime.AddTicks(-(dateTime.Ticks % timeSpan.Ticks));
        }
    }
}
