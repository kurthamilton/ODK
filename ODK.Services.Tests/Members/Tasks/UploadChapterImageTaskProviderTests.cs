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
public static class UploadChapterImageTaskProviderTests
{
    [Test]
    public static void GetTasks_OwnedChapterWithoutImage_ReturnsUploadTask()
    {
        // Arrange
        var chapter = CreateChapter();
        var context = CreateContext(PlatformType.Default, [chapter], chaptersWithImage: []);

        // Act
        var tasks = new UploadChapterImageTaskProvider().GetTasks(context);

        // Assert
        var task = tasks.Should().ContainSingle().Subject;
        task.Type.Should().Be(MemberTaskType.UploadChapterImage);
        task.Chapter.Should().Be(chapter);
    }

    [Test]
    public static void GetTasks_OwnedChapterWithImage_ReturnsNoTask()
    {
        // Arrange
        var chapter = CreateChapter();
        var context = CreateContext(PlatformType.Default, [chapter], chaptersWithImage: [chapter.Id]);

        // Act
        var tasks = new UploadChapterImageTaskProvider().GetTasks(context);

        // Assert
        tasks.Should().BeEmpty();
    }

    [Test]
    public static void GetTasks_DrunkenKnitwits_ReturnsNoTask()
    {
        // Arrange - Drunken Knitwits never displays a group image, so asking for one there would be asking
        // for something the platform then ignores. This is the whole reason for the platform check.
        var context = CreateContext(PlatformType.DrunkenKnitwits, [CreateChapter()], chaptersWithImage: []);

        // Act
        var tasks = new UploadChapterImageTaskProvider().GetTasks(context);

        // Assert
        tasks.Should().BeEmpty();
    }

    [Test]
    public static void GetTasks_OnlyChaptersTheMemberOwns_AreConsidered()
    {
        // Arrange - belonging to a group doesn't make its picture the member's responsibility.
        var context = new MemberTaskContext
        {
            Chapters = [CreateChapter()],
            ChapterProperties = [],
            ChaptersWithImage = [],
            HasAvatar = true,
            Member = CreateMember(),
            MemberProperties = [],
            OwnedChapters = [],
            Platform = PlatformType.Default
        };

        // Act
        var tasks = new UploadChapterImageTaskProvider().GetTasks(context);

        // Assert
        tasks.Should().BeEmpty();
    }

    [Test]
    public static void GetTasks_SomeOwnedChaptersHaveImages_ReturnsTasksOnlyForThoseWithout()
    {
        // Arrange
        var withImage = CreateChapter();
        var withoutImage = CreateChapter();
        var context = CreateContext(
            PlatformType.Default, [withImage, withoutImage], chaptersWithImage: [withImage.Id]);

        // Act
        var tasks = new UploadChapterImageTaskProvider().GetTasks(context);

        // Assert
        tasks.Select(x => x.Chapter).Should().BeEquivalentTo([withoutImage]);
    }

    private static Chapter CreateChapter() => new()
    {
        Id = Guid.NewGuid(),
        Name = $"Chapter {Guid.NewGuid():N}",
        Slug = $"chapter-{Guid.NewGuid():N}"
    };

    private static MemberTaskContext CreateContext(
        PlatformType platform,
        IReadOnlyCollection<Chapter> ownedChapters,
        IReadOnlyCollection<Guid> chaptersWithImage) => new()
    {
        Chapters = [],
        ChapterProperties = [],
        ChaptersWithImage = chaptersWithImage,
        HasAvatar = true,
        Member = CreateMember(),
        MemberProperties = [],
        OwnedChapters = ownedChapters,
        Platform = platform
    };

    private static Member CreateMember() => new() { Id = Guid.NewGuid(), TimeZone = TimeZoneInfo.Utc };
}
