using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Core.Platforms;
using ODK.Services.Members.Tasks;
using ODK.Services.Members.Tasks.Providers;

namespace ODK.Services.Tests.Members.Tasks;

[Parallelizable]
public static class PublishChapterTaskProviderTests
{
    [Test]
    public static void GetTasks_ApprovedAndUnpublished_ReturnsPublishTask()
    {
        // Arrange
        var chapter = CreateChapter(approved: true, published: false);
        var context = CreateContext(chapter);

        // Act
        var tasks = new PublishChapterTaskProvider().GetTasks(context);

        // Assert
        var task = tasks.Should().ContainSingle().Subject;
        task.Type.Should().Be(MemberTaskType.PublishChapter);
        task.Chapter.Should().Be(chapter);
    }

    [Test]
    public static void GetTasks_ApprovedWithNoImage_ReturnsNoTask()
    {
        // Arrange - a group without a picture is not ready to publish, and UploadChapterImageTaskProvider
        // is what asks for the picture, so prompting to publish here would be the wrong next step.
        var context = CreateContext(chaptersWithImage: [], CreateChapter(approved: true, published: false));

        // Act
        var tasks = new PublishChapterTaskProvider().GetTasks(context);

        // Assert
        tasks.Should().BeEmpty();
    }

    [Test]
    public static void GetTasks_NotYetApproved_ReturnsNoTask()
    {
        // Arrange - nothing the owner can do until a site admin approves it, so prompting would be noise.
        var context = CreateContext(CreateChapter(approved: false, published: false));

        // Act
        var tasks = new PublishChapterTaskProvider().GetTasks(context);

        // Assert
        tasks.Should().BeEmpty();
    }

    [Test]
    public static void GetTasks_AlreadyPublished_ReturnsNoTask()
    {
        // Arrange
        var context = CreateContext(CreateChapter(approved: true, published: true));

        // Act
        var tasks = new PublishChapterTaskProvider().GetTasks(context);

        // Assert
        tasks.Should().BeEmpty();
    }

    [Test]
    public static void GetTasks_OnlyChaptersTheMemberOwns_AreConsidered()
    {
        // Arrange - Chapters is every chapter the member belongs to; only OwnedChapters can be published
        // by them, so membership alone must not raise the task.
        var context = new MemberTaskContext
        {
            Chapters = [CreateChapter(approved: true, published: false)],
            ChapterProperties = [],
            ChaptersWithImage = [],
            HasAvatar = true,
            Member = CreateMember(),
            MemberProperties = [],
            OwnedChapters = [],
            Platform = PlatformType.Default
        };

        // Act
        var tasks = new PublishChapterTaskProvider().GetTasks(context);

        // Assert
        tasks.Should().BeEmpty();
    }

    [Test]
    public static void GetTasks_SeveralOwnedChapters_ReturnsOneTaskEach()
    {
        // Arrange
        var first = CreateChapter(approved: true, published: false);
        var second = CreateChapter(approved: true, published: false);
        var context = CreateContext(first, CreateChapter(approved: true, published: true), second);

        // Act
        var tasks = new PublishChapterTaskProvider().GetTasks(context);

        // Assert
        tasks.Select(x => x.Chapter).Should().BeEquivalentTo([first, second]);
    }

    private static Chapter CreateChapter(bool approved, bool published) => new()
    {
        ApprovedUtc = approved ? DateTime.UtcNow : null,
        Id = Guid.NewGuid(),
        Name = $"Chapter {Guid.NewGuid():N}",
        PublishedUtc = published ? DateTime.UtcNow : null,
        Slug = $"chapter-{Guid.NewGuid():N}"
    };

    // Publishing needs a picture, so the owned chapters have one unless a test says otherwise - the
    // picture is not what the other cases are about.
    private static MemberTaskContext CreateContext(params Chapter[] ownedChapters)
        => CreateContext(ownedChapters.Select(x => x.Id).ToArray(), ownedChapters);

    private static MemberTaskContext CreateContext(
        IReadOnlyCollection<Guid> chaptersWithImage,
        params Chapter[] ownedChapters) => new()
    {
        Chapters = [],
        ChapterProperties = [],
        ChaptersWithImage = chaptersWithImage,
        HasAvatar = true,
        Member = CreateMember(),
        MemberProperties = [],
        OwnedChapters = ownedChapters,
        Platform = PlatformType.Default
    };

    private static Member CreateMember() => new() { Id = Guid.NewGuid(), TimeZone = TimeZoneInfo.Utc };
}
