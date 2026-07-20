using System;
using System.Globalization;
using FluentAssertions;
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

    [Test]
    public static void ToFriendlyDateString_ForceIncludeYear_IncludesCurrentYear()
    {
        // Arrange - a current-year date would normally omit the year; ForceIncludeYear overrides that.
        var year = DateTime.UtcNow.Year;
        var date = new DateTime(year, 6, 5, 0, 0, 0, DateTimeKind.Utc);

        // Act
        var result = date.ToFriendlyDateString(new FriendlyDateStringOptions { ForceIncludeYear = true });

        // Assert
        result.Should().Be($"Jun 5, {year}");
    }

    [Test]
    public static void ToFriendlyDateString_FullMonthName_UsesFullMonthName()
    {
        // Arrange
        var date = new DateTime(DateTime.UtcNow.Year, 6, 5, 0, 0, 0, DateTimeKind.Utc);

        // Act
        var result = date.ToFriendlyDateString(new FriendlyDateStringOptions { FullMonthName = true });

        // Assert
        result.Should().Be("June 5");
    }

    [Test]
    public static void ToFriendlyDateString_IncludeDayOfWeekAndPastYear_IncludesDayNameAndYear()
    {
        // Arrange - 5 June 2020 was a Friday; the year is included because it isn't the current year.
        var date = new DateTime(2020, 6, 5, 0, 0, 0, DateTimeKind.Utc);

        // Act
        var result = date.ToFriendlyDateString(new FriendlyDateStringOptions { IncludeDayOfWeek = true });

        // Assert
        result.Should().Be("Fri, Jun 5, 2020");
    }

    [Test]
    public static void ToFriendlyDateString_IncludeTimeAtMidnight_OmitsTime()
    {
        // Arrange
        var date = new DateTime(DateTime.UtcNow.Year, 6, 5, 0, 0, 0, DateTimeKind.Utc);

        // Act - IncludeTime only shows the time when there is a time-of-day.
        var result = date.ToFriendlyDateString(new FriendlyDateStringOptions { IncludeTime = true });

        // Assert
        result.Should().Be("Jun 5");
    }

    [Test]
    public static void ToFriendlyDateString_IncludeTimeWithTimeOfDay_IncludesTime()
    {
        // Arrange
        var date = new DateTime(DateTime.UtcNow.Year, 6, 5, 14, 30, 0, DateTimeKind.Utc);

        // Act
        var result = date.ToFriendlyDateString(new FriendlyDateStringOptions { IncludeTime = true });

        // Assert
        result.Should().Be("Jun 5 14:30");
    }

    [Test]
    public static void ToFriendlyDateString_NullOptions_ReturnsMonthAndDayOnly()
    {
        // Arrange - a current-year date with a time-of-day; with no options neither year nor time show.
        var date = new DateTime(DateTime.UtcNow.Year, 6, 5, 14, 30, 0, DateTimeKind.Utc);

        // Act
        var result = date.ToFriendlyDateString(null);

        // Assert
        result.Should().Be("Jun 5");
    }

    [Test]
    public static void ToFriendlyDateTimeString_AtMidnight_ForcesTime()
    {
        // Arrange - unlike ToFriendlyDateString, the DateTime flavour always shows the time.
        var date = new DateTime(DateTime.UtcNow.Year, 6, 5, 0, 0, 0, DateTimeKind.Utc);

        // Act
        var result = date.ToFriendlyDateTimeString(null);

        // Assert
        result.Should().Be("Jun 5 00:00");
    }

    [Test]
    public static void ToFriendlyDateTimeString_WithDayOfWeek_IncludesDayNameAndTime()
    {
        // Arrange
        var date = new DateTime(2020, 6, 5, 14, 30, 0, DateTimeKind.Utc);

        // Act
        var result = date.ToFriendlyDateTimeString(new FriendlyDateStringOptions { IncludeDayOfWeek = true });

        // Assert
        result.Should().Be("Fri, Jun 5, 2020 14:30");
    }

    [Test]
    public static void ToFriendlyDateTimeString_WithTimeZone_ConvertsToLocalTime()
    {
        // Arrange - a fixed +5 offset so the conversion is deterministic (no DST); 20:00 UTC rolls to the next day.
        var timeZone = TimeZoneInfo.CreateCustomTimeZone("Test+5", TimeSpan.FromHours(5), "Test+5", "Test+5");
        var date = new DateTime(2020, 6, 5, 20, 0, 0, DateTimeKind.Utc);

        // Act
        var result = date.ToFriendlyDateTimeString(new FriendlyDateStringOptions { TimeZone = timeZone });

        // Assert
        result.Should().Be("Jun 6, 2020 01:00");
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