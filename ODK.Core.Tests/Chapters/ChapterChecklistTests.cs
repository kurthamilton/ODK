using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using ODK.Core.Chapters;
using ODK.Core.Platforms;

namespace ODK.Core.Tests.Chapters;

[Parallelizable]
public static class ChapterChecklistTests
{
    [Test]
    public static void Resolve_RecordedStep_WinsOverWhatTheGroupLooksLike()
    {
        // Arrange - the picture has since been removed, but the step was taken and the record says so.
        var chapter = CreateChapter();
        var dismissedUtc = DateTime.UtcNow.AddDays(-2);

        var recorded = new[]
        {
            new ChapterChecklistItem
            {
                ChapterId = chapter.Id,
                ChecklistItemType = ChecklistItemType.Picture,
                DismissedUtc = dismissedUtc
            }
        };

        // Act
        var result = ChapterChecklist.Resolve(
            chapter, Blueprint(), recorded, CreateFacts(), DateTime.UtcNow);

        // Assert
        var picture = Step(result, ChecklistItemType.Picture);
        picture.DismissedUtc.Should().Be(dismissedUtc);
        picture.CompletedUtc.Should().BeNull();

        // Nothing to write: the step already has a row.
        result.Unrecorded.Should().NotContain(x => x.ChecklistItemType == ChecklistItemType.Picture);
    }

    [Test]
    public static void Resolve_StepCompletedSinceLastLook_IsReturnedForRecording()
    {
        // Arrange
        var chapter = CreateChapter();
        var utcNow = DateTime.UtcNow;

        // Act
        var result = ChapterChecklist.Resolve(
            chapter, Blueprint(), [], CreateFacts(hasImage: true), utcNow);

        // Assert
        Step(result, ChecklistItemType.Picture).CompletedUtc.Should().Be(utcNow);

        var unrecorded = result.Unrecorded.Single(x => x.ChecklistItemType == ChecklistItemType.Picture);
        unrecorded.ChapterId.Should().Be(chapter.Id);
        unrecorded.CompletedUtc.Should().Be(utcNow);
    }

    [Test]
    public static void Resolve_StepWithATimestampOfItsOwn_KeepsIt()
    {
        // Arrange - the group was created, approved and published long before anything read a checklist.
        var createdUtc = DateTime.UtcNow.AddDays(-90);
        var approvedUtc = DateTime.UtcNow.AddDays(-60);
        var publishedUtc = DateTime.UtcNow.AddDays(-30);
        var firstEventUtc = DateTime.UtcNow.AddDays(-10);

        var chapter = CreateChapter();
        chapter.CreatedUtc = createdUtc;
        chapter.ApprovedUtc = approvedUtc;
        chapter.PublishedUtc = publishedUtc;

        // Act
        var result = ChapterChecklist.Resolve(
            chapter,
            Blueprint(),
            [],
            CreateFacts(firstEventCreatedUtc: firstEventUtc),
            DateTime.UtcNow);

        // Assert
        Step(result, ChecklistItemType.CreateGroup).CompletedUtc.Should().Be(createdUtc);
        Step(result, ChecklistItemType.SubmitForApproval).CompletedUtc.Should().Be(approvedUtc);
        Step(result, ChecklistItemType.Publish).CompletedUtc.Should().Be(publishedUtc);
        Step(result, ChecklistItemType.FirstEvent).CompletedUtc.Should().Be(firstEventUtc);
    }

    [TestCase(ChecklistItemType.MembershipSettings)]
    [TestCase(ChecklistItemType.PrivacySettings)]
    public static void Resolve_StepNothingCanDetect_StaysOutstanding(ChecklistItemType type)
    {
        // Arrange - settings with defaults that are right for most groups say nothing about being read.
        var chapter = CreateChapter();

        // Act
        var result = ChapterChecklist.Resolve(
            chapter, Blueprint(), [], CreateFacts(), DateTime.UtcNow);

        // Assert
        Step(result, type).IsResolved().Should().BeFalse();
        result.Unrecorded.Should().NotContain(x => x.ChecklistItemType == type);
    }

    [Test]
    public static void Resolve_EveryStepResolved_IsFinished()
    {
        // Arrange
        var chapter = CreateChapter();

        var recorded = Blueprint()
            .Select(x => new ChapterChecklistItem
            {
                ChapterId = chapter.Id,
                ChecklistItemType = x.Type,
                CompletedUtc = DateTime.UtcNow
            })
            .ToArray();

        // Act
        var result = ChapterChecklist.Resolve(
            chapter, Blueprint(), recorded, CreateFacts(), DateTime.UtcNow);

        // Assert
        result.IsFinished().Should().BeTrue();
    }

    [Test]
    public static void Resolve_OneStepOutstanding_IsNotFinished()
    {
        // Arrange - everything recorded bar the first event, which is what outlives publication.
        var chapter = CreateChapter();

        var recorded = Blueprint()
            .Where(x => x.Type != ChecklistItemType.FirstEvent)
            .Select(x => new ChapterChecklistItem
            {
                ChapterId = chapter.Id,
                ChecklistItemType = x.Type,
                CompletedUtc = DateTime.UtcNow
            })
            .ToArray();

        // Act
        var result = ChapterChecklist.Resolve(
            chapter, Blueprint(), recorded, CreateFacts(), DateTime.UtcNow);

        // Assert
        result.IsFinished().Should().BeFalse();
    }

    [Test]
    public static void Resolve_ReturnsStepsInBlueprintOrder()
    {
        // Arrange - handed to it out of order, because nothing about a collection guarantees one.
        var chapter = CreateChapter();
        var blueprint = Blueprint().OrderByDescending(x => x.DisplayOrder).ToArray();

        // Act
        var result = ChapterChecklist.Resolve(
            chapter, blueprint, [], CreateFacts(), DateTime.UtcNow);

        // Assert
        result.Items
            .Select(x => x.Type)
            .Should()
            .ContainInOrder(ChecklistItemType.CreateGroup, ChecklistItemType.Publish, ChecklistItemType.FirstEvent);
    }

    private static IReadOnlyCollection<ChecklistItem> Blueprint()
        => Enum.GetValues<ChecklistItemType>()
            .Where(x => x != ChecklistItemType.None)
            .Select(x => new ChecklistItem
            {
                Dismissable = false,
                DisplayOrder = (int)x,
                Name = x.ToString(),
                Type = x
            })
            .ToArray();

    private static Chapter CreateChapter() => new()
    {
        CreatedUtc = DateTime.UtcNow,
        Id = Guid.NewGuid(),
        Name = "Test group",
        Platform = PlatformType.GroupSquirrel,
        Slug = "test-group"
    };

    private static ChapterChecklistFacts CreateFacts(
        bool hasImage = false,
        DateTime? firstEventCreatedUtc = null) => new()
    {
        FirstEventCreatedUtc = firstEventCreatedUtc,
        HasDescription = false,
        HasImage = hasImage,
        HasMemberProperties = false,
        HasQuestions = false,
        HasShortDescription = false,
        HasTopics = false
    };

    private static ChecklistItemState Step(ChapterChecklistResolution result, ChecklistItemType type)
        => result.Items.Single(x => x.Type == type);
}
