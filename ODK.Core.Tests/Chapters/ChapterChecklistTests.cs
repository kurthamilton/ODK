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
        var submittedUtc = DateTime.UtcNow.AddDays(-75);
        var publishedUtc = DateTime.UtcNow.AddDays(-30);
        var firstEventUtc = DateTime.UtcNow.AddDays(-10);

        var chapter = CreateChapter();
        chapter.CreatedUtc = createdUtc;
        chapter.SubmittedForApprovalUtc = submittedUtc;
        chapter.ApprovedUtc = DateTime.UtcNow.AddDays(-60);
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
        // The step is the owner asking, so it is dated by the ask rather than by the answer.
        Step(result, ChecklistItemType.SubmitForApproval).CompletedUtc.Should().Be(submittedUtc);
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

    [Test]
    public static void PrecedingStepsResolved_FirstStep_IsTrue()
    {
        // Arrange - nothing above it, so there is nothing to be waiting on.
        var chapter = CreateChapter();

        var result = ChapterChecklist.Resolve(
            chapter, Blueprint(), [], CreateFacts(), DateTime.UtcNow);

        // Act / Assert
        result.PrecedingStepsResolved(ChecklistItemType.CreateGroup).Should().BeTrue();
    }

    [Test]
    public static void PrecedingStepsResolved_EverythingAboveIsDone_IsTrue()
    {
        // Arrange - every step above submission behind the group, the two that only a page view can
        // settle among them.
        var chapter = CreateChapter();

        var result = ChapterChecklist.Resolve(
            chapter, Blueprint(), Reviewed(chapter), EverythingDone(), DateTime.UtcNow);

        // Act / Assert
        result.PrecedingStepsResolved(ChecklistItemType.SubmitForApproval).Should().BeTrue();
    }

    [Test]
    public static void PrecedingStepsResolved_SomethingAboveIsOutstanding_IsFalse()
    {
        // Arrange - the same, bar the picture.
        var chapter = CreateChapter();

        var facts = EverythingDone(hasImage: false);

        var result = ChapterChecklist.Resolve(
            chapter, Blueprint(), Reviewed(chapter), facts, DateTime.UtcNow);

        // Act / Assert
        result.PrecedingStepsResolved(ChecklistItemType.SubmitForApproval).Should().BeFalse();
    }

    [Test]
    public static void PrecedingStepsResolved_StepAboveWasSkipped_IsTrue()
    {
        /* Arrange - topics never added but skipped instead, which is what lets the order stand in for a
           list of which steps are required: an optional one is dealt with by being declined. */
        var chapter = CreateChapter();

        var recorded = Reviewed(chapter)
            .Append(new ChapterChecklistItem
            {
                ChapterId = chapter.Id,
                ChecklistItemType = ChecklistItemType.Topics,
                DismissedUtc = DateTime.UtcNow
            })
            .ToArray();

        var facts = EverythingDone(hasTopics: false);

        var result = ChapterChecklist.Resolve(chapter, Blueprint(), recorded, facts, DateTime.UtcNow);

        // Act / Assert
        result.PrecedingStepsResolved(ChecklistItemType.SubmitForApproval).Should().BeTrue();
    }

    [Test]
    public static void PrecedingStepsResolved_StepTheGroupDoesNotHave_IsFalse()
    {
        // Arrange - a step this group's checklist does not include is not one it can be waiting to reach.
        var chapter = CreateChapter();

        var blueprint = Blueprint()
            .Where(x => x.Type != ChecklistItemType.SubmitForApproval)
            .ToArray();

        var result = ChapterChecklist.Resolve(
            chapter, blueprint, Reviewed(chapter), EverythingDone(), DateTime.UtcNow);

        // Act / Assert
        result.PrecedingStepsResolved(ChecklistItemType.SubmitForApproval).Should().BeFalse();
    }

    /// <summary>
    /// Rows for the two steps nothing can derive - membership and privacy settings are only ever settled
    /// by somebody opening the page.
    /// </summary>
    private static ChapterChecklistItem[] Reviewed(Chapter chapter) =>
    [
        new ChapterChecklistItem
        {
            ChapterId = chapter.Id,
            ChecklistItemType = ChecklistItemType.MembershipSettings,
            CompletedUtc = DateTime.UtcNow
        },
        new ChapterChecklistItem
        {
            ChapterId = chapter.Id,
            ChecklistItemType = ChecklistItemType.PrivacySettings,
            CompletedUtc = DateTime.UtcNow
        }
    ];

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
        bool hasDescription = false,
        bool hasMemberProperties = false,
        bool hasQuestions = false,
        bool hasShortDescription = false,
        bool hasTopics = false,
        DateTime? firstEventCreatedUtc = null) => new()
    {
        FirstEventCreatedUtc = firstEventCreatedUtc,
        HasDescription = hasDescription,
        HasImage = hasImage,
        HasMemberProperties = hasMemberProperties,
        HasQuestions = hasQuestions,
        HasShortDescription = hasShortDescription,
        HasTopics = hasTopics
    };

    /// <summary>
    /// A group that has done everything the checklist can detect, so a test about one missing step says
    /// which by turning that one off.
    /// </summary>
    private static ChapterChecklistFacts EverythingDone(bool hasImage = true, bool hasTopics = true)
        => CreateFacts(
            hasImage: hasImage,
            hasDescription: true,
            hasMemberProperties: true,
            hasQuestions: true,
            hasShortDescription: true,
            hasTopics: hasTopics,
            firstEventCreatedUtc: DateTime.UtcNow);

    private static ChecklistItemState Step(ChapterChecklistResolution result, ChecklistItemType type)
        => result.Items.Single(x => x.Type == type);
}
