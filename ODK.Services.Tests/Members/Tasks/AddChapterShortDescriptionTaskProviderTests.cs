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
public static class AddChapterShortDescriptionTaskProviderTests
{
    [Test]
    public static void GetTasks_OwnedChapterWithoutShortDescription_ReturnsTask()
    {
        // Arrange
        var chapter = CreateChapter();
        var context = CreateContext([chapter], chaptersWithShortDescription: []);

        // Act
        var tasks = new AddChapterShortDescriptionTaskProvider().GetTasks(context);

        // Assert
        var task = tasks.Should().ContainSingle().Subject;
        task.Type.Should().Be(MemberTaskType.AddChapterShortDescription);
        task.Chapter.Should().Be(chapter);
    }

    [Test]
    public static void GetTasks_OwnedChapterWithShortDescription_ReturnsNoTask()
    {
        // Arrange
        var chapter = CreateChapter();
        var context = CreateContext([chapter], chaptersWithShortDescription: [chapter.Id]);

        // Act
        var tasks = new AddChapterShortDescriptionTaskProvider().GetTasks(context);

        // Assert
        tasks.Should().BeEmpty();
    }

    [Test]
    public static void GetTasks_DrunkenKnitwits_ReturnsNoTask()
    {
        // Arrange - Drunken Knitwits does not list groups, so it never shows a short description and
        // asking for one there would be asking for something the platform then ignores.
        var context = CreateContext(
            [CreateChapter()], chaptersWithShortDescription: [], platform: PlatformType.DrunkenKnitwits);

        // Act
        var tasks = new AddChapterShortDescriptionTaskProvider().GetTasks(context);

        // Assert
        tasks.Should().BeEmpty();
    }

    [Test]
    public static void GetTasks_OnlyChaptersTheMemberOwns_AreConsidered()
    {
        // Arrange - belonging to a group doesn't make its summary the member's responsibility.
        var context = new MemberTaskContext
        {
            Chapters = [CreateChapter()],
            ChapterProperties = [],
            ChaptersWithShortDescription = [],
            ChaptersWithImage = [],
            HasAvatar = true,
            Member = CreateMember(),
            MemberProperties = [],
            OwnedChapters = [],
            Platform = PlatformType.GroupSquirrel
        };

        // Act
        var tasks = new AddChapterShortDescriptionTaskProvider().GetTasks(context);

        // Assert
        tasks.Should().BeEmpty();
    }

    [Test]
    public static void GetTasks_SomeOwnedChaptersHaveShortDescriptions_ReturnsTasksOnlyForThoseWithout()
    {
        // Arrange
        var withShortDescription = CreateChapter();
        var withoutShortDescription = CreateChapter();
        var context = CreateContext(
            [withShortDescription, withoutShortDescription],
            chaptersWithShortDescription: [withShortDescription.Id]);

        // Act
        var tasks = new AddChapterShortDescriptionTaskProvider().GetTasks(context);

        // Assert
        tasks.Select(x => x.Chapter).Should().BeEquivalentTo([withoutShortDescription]);
    }

    private static Chapter CreateChapter() => new()
    {
        Id = Guid.NewGuid(),
        Name = $"Chapter {Guid.NewGuid():N}",
        Slug = $"chapter-{Guid.NewGuid():N}"
    };

    private static MemberTaskContext CreateContext(
        IReadOnlyCollection<Chapter> ownedChapters,
        IReadOnlyCollection<Guid> chaptersWithShortDescription,
        PlatformType platform = PlatformType.GroupSquirrel) => new()
    {
        Chapters = [],
        ChapterProperties = [],
        ChaptersWithShortDescription = chaptersWithShortDescription,
        ChaptersWithImage = [],
        HasAvatar = true,
        Member = CreateMember(),
        MemberProperties = [],
        OwnedChapters = ownedChapters,
        Platform = platform
    };

    private static Member CreateMember() => new() { Id = Guid.NewGuid(), TimeZone = TimeZoneInfo.Utc };
}
