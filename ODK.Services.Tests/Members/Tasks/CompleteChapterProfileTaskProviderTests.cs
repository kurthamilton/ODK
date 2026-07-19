using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using ODK.Core.Chapters;
using ODK.Core.DataTypes;
using ODK.Core.Members;
using ODK.Core.Platforms;
using ODK.Services.Members.Tasks;
using ODK.Services.Members.Tasks.Providers;

namespace ODK.Services.Tests.Members.Tasks;

[Parallelizable]
public static class CompleteChapterProfileTaskProviderTests
{
    [Test]
    public static void GetTasks_MissingApplicationOnlyProperty_ReturnsNoTask()
    {
        // Arrange - application-only properties are only required when applying to join, not afterwards.
        var chapter = CreateChapter();
        var property = CreateProperty(chapter.Id, required: true, applicationOnly: true);
        var context = CreateContext(chapter, [property], []);

        // Act
        var tasks = new CompleteChapterProfileTaskProvider().GetTasks(context);

        // Assert
        tasks.Should().BeEmpty();
    }

    [Test]
    public static void GetTasks_MissingRequiredProperty_ReturnsTaskForChapter()
    {
        // Arrange
        var chapter = CreateChapter();
        var property = CreateProperty(chapter.Id, required: true, applicationOnly: false);
        var context = CreateContext(chapter, [property], []);

        // Act
        var tasks = new CompleteChapterProfileTaskProvider().GetTasks(context);

        // Assert
        var task = tasks.Should().ContainSingle().Subject;
        task.Type.Should().Be(MemberTaskType.CompleteChapterProfile);
        task.Chapter.Should().Be(chapter);
    }

    [Test]
    public static void GetTasks_RequiredPropertyAnswered_ReturnsNoTask()
    {
        // Arrange
        var chapter = CreateChapter();
        var property = CreateProperty(chapter.Id, required: true, applicationOnly: false);
        var answer = new MemberProperty { ChapterPropertyId = property.Id, Value = "answered" };
        var context = CreateContext(chapter, [property], [answer]);

        // Act
        var tasks = new CompleteChapterProfileTaskProvider().GetTasks(context);

        // Assert
        tasks.Should().BeEmpty();
    }

    private static Chapter CreateChapter() => new Chapter
    {
        Id = Guid.NewGuid(),
        Name = "Test",
        Slug = "test"
    };

    private static MemberTaskContext CreateContext(
        Chapter chapter,
        IReadOnlyCollection<ChapterProperty> chapterProperties,
        IReadOnlyCollection<MemberProperty> memberProperties) => new MemberTaskContext
    {
        Chapters = [chapter],
        ChapterProperties = chapterProperties,
        HasAvatar = true,
        Member = new Member { Id = Guid.NewGuid() },
        MemberProperties = memberProperties,
        Platform = PlatformType.Default
    };

    private static ChapterProperty CreateProperty(Guid chapterId, bool required, bool applicationOnly) => new ChapterProperty
    {
        Id = Guid.NewGuid(),
        ChapterId = chapterId,
        Required = required,
        ApplicationOnly = applicationOnly,
        DataType = DataType.Text,
        Label = "Bio"
    };
}
