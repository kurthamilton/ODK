using System;
using System.Globalization;
using NUnit.Framework;
using ODK.Core.Utils;

namespace ODK.Core.Tests.Utils;

[Parallelizable]
public static class DateUtilsTests
{
    private const int OneMinuteInSeconds = 60;
    private const int OneHourInSeconds = OneMinuteInSeconds * 60;
    private const int OneDayInSeconds = OneHourInSeconds * 24;

    [TestCase(DayOfWeek.Sunday, ExpectedResult = 1)]
    [TestCase(DayOfWeek.Monday, ExpectedResult = 2)]
    [TestCase(DayOfWeek.Tuesday, ExpectedResult = 3)]
    [TestCase(DayOfWeek.Wednesday, ExpectedResult = 4)]
    [TestCase(DayOfWeek.Thursday, ExpectedResult = 5)]
    [TestCase(DayOfWeek.Friday, ExpectedResult = 6)]
    [TestCase(DayOfWeek.Saturday, ExpectedResult = 7)]
    public static int Next_ReturnsNextInstance(DayOfWeek dayOfWeek)
    {
        // Arrange
        // Saturday
        var date = new DateTime(2024, 07, 20);

        // Act
        var result = date.Next(dayOfWeek);

        // Assert
        return (result - date).Days;
    }

    [TestCase(DayOfWeek.Sunday, ExpectedResult = -6)]
    [TestCase(DayOfWeek.Monday, ExpectedResult = -5)]
    [TestCase(DayOfWeek.Tuesday, ExpectedResult = -4)]
    [TestCase(DayOfWeek.Wednesday, ExpectedResult = -3)]
    [TestCase(DayOfWeek.Thursday, ExpectedResult = -2)]
    [TestCase(DayOfWeek.Friday, ExpectedResult = -1)]
    [TestCase(DayOfWeek.Saturday, ExpectedResult = -7)]
    public static int Previous_ReturnsPreviousInstance(DayOfWeek dayOfWeek)
    {
        // Arrange
        // Saturday
        var date = new DateTime(2024, 07, 20);

        // Act
        var result = date.Previous(dayOfWeek);

        // Assert
        return (result - date).Days;
    }

    [TestCase(0, ExpectedResult = "just now")]
    [TestCase(OneMinuteInSeconds - 1, ExpectedResult = "just now")]
    [TestCase(OneMinuteInSeconds, ExpectedResult = "1 minute ago")]
    [TestCase((2 * OneMinuteInSeconds) - 1, ExpectedResult = "1 minute ago")]
    [TestCase(2 * OneMinuteInSeconds, ExpectedResult = "2 minutes ago")]
    [TestCase(OneDayInSeconds - 1, ExpectedResult = "23 hours ago")]
    public static string ToRelativeTime(int secondsAgo)
    {
        // Arrange
        var utcNow = DateTime.UtcNow;
        var dateUtc = utcNow.AddSeconds(-1 * secondsAgo);

        // Act
        var result = DateUtils.ToRelativeTime(dateUtc);

        // Assert
        return result;
    }

    [TestCase("Pacific Standard Time", "2024-01-01 12:00:00", ExpectedResult = "2024-01-01 20:00:00")]
    [TestCase("Pacific Standard Time", "2024-07-01 12:00:00", ExpectedResult = "2024-07-01 19:00:00")]
    [TestCase("GMT Standard Time", "2024-01-01 12:00:00", ExpectedResult = "2024-01-01 12:00:00")]
    [TestCase("GMT Standard Time", "2024-07-01 12:00:00", ExpectedResult = "2024-07-01 11:00:00")]
    [TestCase("AUS Eastern Standard Time", "2024-01-01 12:00:00", ExpectedResult = "2024-01-01 01:00:00")]
    [TestCase("AUS Eastern Standard Time", "2024-07-01 12:00:00", ExpectedResult = "2024-07-01 02:00:00")]
    public static string ToUtc(string timeZoneId, string timeString)
    {
        // Arrange
        var time = DateTime.ParseExact(timeString, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);

        // Act
        var result = time.ToUtc(timeZone);

        // Assert
        return result.ToString("yyyy-MM-dd HH:mm:ss");
    }
}