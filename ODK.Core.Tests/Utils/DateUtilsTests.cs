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

    [Test]
    public static void ChapterTimeZoneLabel_MemberTimeZoneNull_ReturnsEmpty()
    {
        // Arrange - an anonymous / unknown viewer: there's nothing to compare against.
        var chapter = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");

        // Act / Assert
        DateUtils.ChapterTimeZoneLabel(chapter, null, new DateTime(2024, 7, 1)).Should().BeEmpty();
    }

    [Test]
    public static void ChapterTimeZoneLabel_SameTimeZone_ReturnsEmpty()
    {
        // Arrange - the viewer is in the chapter's own timezone, so no label is needed.
        var chapter = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
        var member = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");

        // Act / Assert
        DateUtils.ChapterTimeZoneLabel(chapter, member, new DateTime(2024, 7, 1)).Should().BeEmpty();
    }

    // The member is in UTC (a different zone id from every chapter zone below), so a label is always shown;
    // the offset is the chapter zone's, computed for the date so DST is reflected.
    [TestCase("GMT Standard Time", "2024-07-01", ExpectedResult = "(UTC+1)")]
    [TestCase("GMT Standard Time", "2024-01-01", ExpectedResult = "(UTC+0)")]
    [TestCase("India Standard Time", "2024-07-01", ExpectedResult = "(UTC+5:30)")]
    [TestCase("Pacific Standard Time", "2024-01-01", ExpectedResult = "(UTC-8)")]
    public static string ChapterTimeZoneLabel_MemberInDifferentZone_ReturnsChapterOffset(
        string chapterTimeZoneId, string dateString)
    {
        // Arrange
        var chapter = TimeZoneInfo.FindSystemTimeZoneById(chapterTimeZoneId);
        var member = TimeZoneInfo.Utc;
        var dateUtc = DateTime.ParseExact(dateString, "yyyy-MM-dd", CultureInfo.InvariantCulture);

        // Act
        return DateUtils.ChapterTimeZoneLabel(chapter, member, dateUtc);
    }

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

    [TestCase("en-GB", ExpectedResult = "5 Jun")]
    [TestCase("en-US", ExpectedResult = "Jun 5")]
    public static string ToFriendlyDateString_CurrentYear_OmitsYearInCultureOrder(string culture)
    {
        // Arrange - a current-year date omits the year; the day/month order follows the culture.
        var date = new DateTime(DateTime.UtcNow.Year, 6, 5, 0, 0, 0, DateTimeKind.Utc);

        // Act
        return date.ToFriendlyDateString(new FriendlyDateStringOptions
        {
            Culture = CultureInfo.GetCultureInfo(culture)
        });
    }

    [TestCase("en-GB", ExpectedResult = "5 Jun 2020")]
    [TestCase("en-US", ExpectedResult = "Jun 5, 2020")]
    public static string ToFriendlyDateString_PastYear_IncludesYearInCultureOrder(string culture)
    {
        // Arrange - 5 June 2020 is not the current year, so the year is included.
        var date = new DateTime(2020, 6, 5, 0, 0, 0, DateTimeKind.Utc);

        // Act
        return date.ToFriendlyDateString(new FriendlyDateStringOptions
        {
            Culture = CultureInfo.GetCultureInfo(culture)
        });
    }

    [Test]
    public static void ToFriendlyDateString_ForceIncludeYear_IncludesCurrentYear()
    {
        // Arrange - a current-year date would normally omit the year; ForceIncludeYear overrides that.
        var year = DateTime.UtcNow.Year;
        var date = new DateTime(year, 6, 5, 0, 0, 0, DateTimeKind.Utc);

        // Act
        var result = date.ToFriendlyDateString(new FriendlyDateStringOptions
        {
            ForceIncludeYear = true,
            Culture = CultureInfo.GetCultureInfo("en-GB")
        });

        // Assert
        result.Should().Be($"5 Jun {year}");
    }

    [TestCase("en-GB", ExpectedResult = "5 June")]
    [TestCase("en-US", ExpectedResult = "June 5")]
    public static string ToFriendlyDateString_FullMonthName_UsesFullMonthNameInCultureOrder(string culture)
    {
        // Arrange
        var date = new DateTime(DateTime.UtcNow.Year, 6, 5, 0, 0, 0, DateTimeKind.Utc);

        // Act
        return date.ToFriendlyDateString(new FriendlyDateStringOptions
        {
            FullMonthName = true,
            Culture = CultureInfo.GetCultureInfo(culture)
        });
    }

    [TestCase("en-GB", ExpectedResult = "Fri, 5 Jun 2020")]
    [TestCase("en-US", ExpectedResult = "Fri, Jun 5, 2020")]
    public static string ToFriendlyDateString_IncludeDayOfWeekAndPastYear_PrefixesDayNameInCultureOrder(string culture)
    {
        // Arrange - 5 June 2020 was a Friday; the year is included because it isn't the current year.
        var date = new DateTime(2020, 6, 5, 0, 0, 0, DateTimeKind.Utc);

        // Act
        return date.ToFriendlyDateString(new FriendlyDateStringOptions
        {
            IncludeDayOfWeek = true,
            Culture = CultureInfo.GetCultureInfo(culture)
        });
    }

    [Test]
    public static void ToFriendlyDateString_IncludeTimeAtMidnight_OmitsTime()
    {
        // Arrange
        var date = new DateTime(DateTime.UtcNow.Year, 6, 5, 0, 0, 0, DateTimeKind.Utc);

        // Act - IncludeTime only shows the time when there is a time-of-day.
        var result = date.ToFriendlyDateString(new FriendlyDateStringOptions
        {
            IncludeTime = true,
            Culture = CultureInfo.GetCultureInfo("en-GB")
        });

        // Assert
        result.Should().Be("5 Jun");
    }

    [Test]
    public static void ToFriendlyDateString_IncludeTimeWithTimeOfDay_IncludesTime()
    {
        // Arrange - the time is always 24-hour, independent of the culture.
        var date = new DateTime(DateTime.UtcNow.Year, 6, 5, 14, 30, 0, DateTimeKind.Utc);

        // Act
        var result = date.ToFriendlyDateString(new FriendlyDateStringOptions
        {
            IncludeTime = true,
            Culture = CultureInfo.GetCultureInfo("en-GB")
        });

        // Assert
        result.Should().Be("5 Jun 14:30");
    }

    [Test]
    public static void ToFriendlyDateString_NullOptions_OmitsYearAndTime()
    {
        // Arrange - a current-year date with a time-of-day; with no options neither year nor time show, and
        // the ambient CurrentCulture supplies the order (asserted structurally so the runner's culture doesn't matter).
        var year = DateTime.UtcNow.Year;
        var date = new DateTime(year, 6, 5, 14, 30, 0, DateTimeKind.Utc);

        // Act
        var result = date.ToFriendlyDateString(null);

        // Assert
        result.Should().NotContain(year.ToString()).And.NotContain("14:30");
    }

    [Test]
    public static void ToFriendlyDateTimeString_AtMidnight_ForcesTime()
    {
        // Arrange - unlike ToFriendlyDateString, the DateTime flavour always shows the time.
        var date = new DateTime(DateTime.UtcNow.Year, 6, 5, 0, 0, 0, DateTimeKind.Utc);

        // Act
        var result = date.ToFriendlyDateTimeString(new FriendlyDateStringOptions
        {
            Culture = CultureInfo.GetCultureInfo("en-GB")
        });

        // Assert
        result.Should().Be("5 Jun 00:00");
    }

    [Test]
    public static void ToFriendlyDateTimeString_WithDayOfWeek_IncludesDayNameAndTime()
    {
        // Arrange
        var date = new DateTime(2020, 6, 5, 14, 30, 0, DateTimeKind.Utc);

        // Act
        var result = date.ToFriendlyDateTimeString(new FriendlyDateStringOptions
        {
            IncludeDayOfWeek = true,
            Culture = CultureInfo.GetCultureInfo("en-GB")
        });

        // Assert
        result.Should().Be("Fri, 5 Jun 2020 14:30");
    }

    [Test]
    public static void ToFriendlyDateTimeString_WithTimeZone_ConvertsToLocalTime()
    {
        // Arrange - a fixed +5 offset so the conversion is deterministic (no DST); 20:00 UTC rolls to the next day.
        var timeZone = TimeZoneInfo.CreateCustomTimeZone("Test+5", TimeSpan.FromHours(5), "Test+5", "Test+5");
        var date = new DateTime(2020, 6, 5, 20, 0, 0, DateTimeKind.Utc);

        // Act
        var result = date.ToFriendlyDateTimeString(new FriendlyDateStringOptions
        {
            TimeZone = timeZone,
            Culture = CultureInfo.GetCultureInfo("en-GB")
        });

        // Assert
        result.Should().Be("6 Jun 2020 01:00");
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
        var result = DateUtils.ToRelativeTime(dateUtc, TimeZoneInfo.Utc);

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