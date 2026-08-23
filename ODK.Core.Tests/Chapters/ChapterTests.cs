using System;
using FluentAssertions;
using NUnit.Framework;
using ODK.Core.Chapters;

namespace ODK.Core.Tests.Chapters;

[Parallelizable]
public static class ChapterTests
{
    [Test]
    public static void CanBePublished_AlreadyPublished_ReturnsFalse()
    {
        // Arrange
        var chapter = CreateChapter(approved: true);
        chapter.PublishedUtc = DateTime.UtcNow;

        // Act
        var result = chapter.CanBePublished(hasImage: true);

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public static void CanBePublished_ApprovedWithImage_ReturnsTrue()
    {
        // Arrange
        var chapter = CreateChapter(approved: true);

        // Act
        var result = chapter.CanBePublished(hasImage: true);

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    public static void CanBePublished_ApprovedWithNoImage_ReturnsFalse()
    {
        // Arrange - the picture is shown wherever the group is listed, so there is nothing to list
        // without one.
        var chapter = CreateChapter(approved: true);

        // Act
        var result = chapter.CanBePublished(hasImage: false);

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public static void CanBePublished_NotApproved_ReturnsFalse()
    {
        // Arrange
        var chapter = CreateChapter(approved: false);

        // Act
        var result = chapter.CanBePublished(hasImage: true);

        // Assert
        result.Should().BeFalse();
    }

    private static Chapter CreateChapter(bool approved) => new()
    {
        ApprovedUtc = approved ? DateTime.UtcNow : null,
        Id = Guid.NewGuid(),
        Name = "Test",
        Slug = "test"
    };
}
