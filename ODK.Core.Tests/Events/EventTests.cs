using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using ODK.Core.Events;
using ODK.Core.Utils;

namespace ODK.Core.Tests.Events;

[Parallelizable]
public static class EventTests
{
    [TestCase("Pacific Standard Time", "2024-01-01 12:00:00", ExpectedResult = "2024-01-01 20:00:00")]
    [TestCase("Pacific Standard Time", "2024-07-01 12:00:00", ExpectedResult = "2024-07-01 19:00:00")]
    [TestCase("GMT Standard Time", "2024-01-01 12:00:00", ExpectedResult = "2024-01-01 12:00:00")]
    [TestCase("GMT Standard Time", "2024-07-01 12:00:00", ExpectedResult = "2024-07-01 11:00:00")]
    [TestCase("AUS Eastern Standard Time", "2024-01-01 12:00:00", ExpectedResult = "2024-01-01 01:00:00")]
    [TestCase("AUS Eastern Standard Time", "2024-07-01 12:00:00", ExpectedResult = "2024-07-01 02:00:00")]
    public static string FromLocalTime_WithTime_ConvertsToUtc(string timeZoneId, string timeString)
    {
        // Arrange
        var time = DateTime.ParseExact(timeString, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);

        // Act
        var result = Event.FromLocalTime(time, timeZone);

        // Assert
        return result.ToString("yyyy-MM-dd HH:mm:ss");
    }

    [TestCase("Pacific Standard Time", "2024-01-01 00:00:00", ExpectedResult = "2024-01-01 00:00:00")]
    [TestCase("Pacific Standard Time", "2024-07-01 00:00:00", ExpectedResult = "2024-07-01 00:00:00")]
    [TestCase("GMT Standard Time", "2024-01-01 00:00:00", ExpectedResult = "2024-01-01 00:00:00")]
    [TestCase("GMT Standard Time", "2024-07-01 00:00:00", ExpectedResult = "2024-07-01 00:00:00")]
    [TestCase("AUS Eastern Standard Time", "2024-01-01 00:00:00", ExpectedResult = "2024-01-01 00:00:00")]
    [TestCase("AUS Eastern Standard Time", "2024-07-01 00:00:00", ExpectedResult = "2024-07-01 00:00:00")]
    public static string FromLocalTime_NoTime_DoesNotConvert(string timeZoneId, string timeString)
    {
        // Arrange
        var time = DateTime.ParseExact(timeString, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);

        // Act
        var result = Event.FromLocalTime(time, timeZone);

        // Assert
        return result.ToString("yyyy-MM-dd HH:mm:ss");
    }
}
