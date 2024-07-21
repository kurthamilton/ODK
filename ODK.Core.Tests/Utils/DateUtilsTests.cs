using System;
using NUnit.Framework;
using ODK.Core.Utils;

namespace ODK.Core.Tests.Utils;
[Parallelizable]
public static class DateUtilsTests
{
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
}
