using System;
using FluentAssertions;
using NUnit.Framework;
using ODK.Core.Chapters;

namespace ODK.Core.Tests.Chapters;

[Parallelizable]
public static class ChapterMigrationTests
{
    private static readonly DateTime UtcNow = new(2026, 9, 13, 0, 0, 0, DateTimeKind.Utc);

    [Test]
    public static void MovedRecently_MoveInsideTheWindow_ReturnsTrue()
    {
        // Arrange
        var migration = new ChapterMigration { MovedUtc = UtcNow.AddDays(-29) };

        // Act
        var result = migration.MovedRecently(withinDays: 30, UtcNow);

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    public static void MovedRecently_MoveOnTheBoundary_ReturnsFalse()
    {
        // Arrange
        var migration = new ChapterMigration { MovedUtc = UtcNow.AddDays(-30) };

        // Act
        var result = migration.MovedRecently(withinDays: 30, UtcNow);

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public static void MovedRecently_NoMove_ReturnsFalse()
    {
        // Arrange - a moved page that was never switched on, which is not a recent move but no move at all.
        var migration = new ChapterMigration { MessageHtml = "<p>Somewhere better</p>" };

        // Act
        var result = migration.MovedRecently(withinDays: 30, UtcNow);

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public static void MovedRecently_NoWindow_ReturnsFalse()
    {
        // Arrange
        var migration = new ChapterMigration { MovedUtc = UtcNow };

        // Act
        var result = migration.MovedRecently(withinDays: 0, UtcNow);

        // Assert
        result.Should().BeFalse();
    }
}
