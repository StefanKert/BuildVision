using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BuildVision.Common.Extensions;
using FluentAssertions;
using Xunit;

namespace BuildVision.UnitTests
{
    public class DateTimeExtensionTests
    {
        [Fact]
        public void TruncateDateTime_ForZeroTimeStamp_ShouldReturn_DateTime()
        {
            var dateTime = DateTime.Now;
            var truncatedDateTime = DateTime.Now.Truncate(TimeSpan.Zero);
            truncatedDateTime.Should().Be(dateTime);
        }

        [Fact]
        public void TruncateDateTime_ForNegativeTimestamp_ShouldReturn_DateTime()
        {
            var dateTime = DateTime.Now;
            var truncatedDateTime = DateTime.Now.Truncate(TimeSpan.FromMinutes(-1));
            truncatedDateTime.Should().Be(dateTime);
        }

        [Fact]
        public void TruncateDateTime_ForAMinute_ShouldReturn_DateTime_WithoutSeconds()
        {
            var dateTime = DateTime.Now;
            var truncatedDateTime = dateTime.Truncate(TimeSpan.FromMinutes(1));
            truncatedDateTime.Should().Be(new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, 0));
        }
    }
}
