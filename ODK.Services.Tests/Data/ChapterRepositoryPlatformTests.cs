using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using ODK.Core.Chapters;
using ODK.Core.Platforms;
using ODK.Services.Tests.Helpers;

namespace ODK.Services.Tests.Data;

/// <summary>
/// The three questions a chapter query can ask about a platform, which used to be two meanings of
/// <see cref="PlatformType.GroupSquirrel"/>: what a platform shows, what it owns, and every chapter regardless.
/// </summary>
[Parallelizable]
public static class ChapterRepositoryPlatformTests
{
    [Test]
    public static async Task GetAll_GroupSquirrel_ShowsDrunkenKnitwitsChaptersToo()
    {
        /* Arrange - the behaviour the whole two-platform arrangement rests on: a Drunken Knitwits group is
           reachable from Group Squirrel. Pinned here because reading Default as "Group Squirrel's own" is
           the obvious-looking simplification that breaks it. */
        using var context = CreateMockOdkContext();

        var groupSquirrel = context.CreateChapter(
            name: "Squirrels", platform: PlatformType.GroupSquirrel, afterCreate: Publish);
        var drunkenKnitwits = context.CreateChapter(
            name: "Bristol", platform: PlatformType.DrunkenKnitwits, afterCreate: Publish);

        var unitOfWork = MockUnitOfWorkFactory.Create(context);

        // Act
        var result = await unitOfWork.ChapterRepository
            .GetAll(PlatformType.GroupSquirrel, includeUnpublished: false)
            .Run();

        // Assert
        result.Select(x => x.Id).Should().BeEquivalentTo([groupSquirrel.Id, drunkenKnitwits.Id]);
    }

    [Test]
    public static async Task GetAll_GroupSquirrel_HidesUnpublishedChapters()
    {
        // Arrange
        using var context = CreateMockOdkContext();

        var published = context.CreateChapter(name: "Published", afterCreate: Publish);
        context.CreateChapter(name: "Draft");

        var unitOfWork = MockUnitOfWorkFactory.Create(context);

        // Act
        var result = await unitOfWork.ChapterRepository
            .GetAll(PlatformType.GroupSquirrel, includeUnpublished: false)
            .Run();

        // Assert
        result.Select(x => x.Id).Should().Equal(published.Id);
    }

    [Test]
    public static async Task GetAll_DrunkenKnitwits_ShowsItsOwnChaptersPublishedOrNot()
    {
        /* Arrange - Drunken Knitwits shows every one of its chapters; an unpublished one carries a
           RedirectUrl by convention. It shows no other platform's. */
        using var context = CreateMockOdkContext();

        var published = context.CreateChapter(
            name: "Bristol", platform: PlatformType.DrunkenKnitwits, afterCreate: Publish);
        var draft = context.CreateChapter(
            name: "Leeds", platform: PlatformType.DrunkenKnitwits);
        context.CreateChapter(name: "Squirrels", platform: PlatformType.GroupSquirrel, afterCreate: Publish);

        var unitOfWork = MockUnitOfWorkFactory.Create(context);

        // Act
        var result = await unitOfWork.ChapterRepository
            .GetAll(PlatformType.DrunkenKnitwits, includeUnpublished: false)
            .Run();

        // Assert
        result.Select(x => x.Id).Should().BeEquivalentTo([published.Id, draft.Id]);
    }

    [Test]
    public static async Task GetOwnedByPlatform_GroupSquirrel_ExcludesDrunkenKnitwitsChapters()
    {
        // Arrange - what a platform owns, unlike what it shows, is one platform's and never both.
        using var context = CreateMockOdkContext();

        var groupSquirrel = context.CreateChapter(
            name: "Squirrels", platform: PlatformType.GroupSquirrel, afterCreate: Publish);
        context.CreateChapter(
            name: "Bristol", platform: PlatformType.DrunkenKnitwits, afterCreate: Publish);
        context.CreateChapter(name: "Draft", platform: PlatformType.GroupSquirrel);

        var unitOfWork = MockUnitOfWorkFactory.Create(context);

        // Act
        var result = await unitOfWork.ChapterRepository
            .GetOwnedByPlatform(PlatformType.GroupSquirrel)
            .Run();

        // Assert
        result.Select(x => x.Id).Should().Equal(groupSquirrel.Id);
    }

    [Test]
    public static async Task QueryPublished_NoPlatformNamed_ReturnsEveryPlatformsChapters()
    {
        /* Arrange - the unfiltered query, which is how platform-agnostic work asks: the Instagram sweep
           reads a group's own account and stores what it finds against that group, so which site triggered
           it decides nothing. Pinned because scoping it to a platform would silently halve what it does. */
        using var context = CreateMockOdkContext();

        var groupSquirrel = context.CreateChapter(
            name: "Squirrels", platform: PlatformType.GroupSquirrel, afterCreate: Publish);
        var drunkenKnitwits = context.CreateChapter(
            name: "Bristol", platform: PlatformType.DrunkenKnitwits, afterCreate: Publish);
        context.CreateChapter(name: "Draft", platform: PlatformType.DrunkenKnitwits);

        var unitOfWork = MockUnitOfWorkFactory.Create(context);

        // Act
        var result = await unitOfWork.ChapterRepository
            .Query()
            .Published()
            .GetAll()
            .Run();

        // Assert
        result.Select(x => x.Id).Should().BeEquivalentTo([groupSquirrel.Id, drunkenKnitwits.Id]);
    }

    [Test]
    public static async Task NameExists_NameTakenOnAnotherPlatform_ReturnsTrue()
    {
        // Arrange - a name is unique across every platform, so the check names no site at all.
        using var context = CreateMockOdkContext();

        context.CreateChapter(name: "Bristol", platform: PlatformType.DrunkenKnitwits);

        var unitOfWork = MockUnitOfWorkFactory.Create(context);

        // Act
        var result = await unitOfWork.ChapterRepository.NameExists("Bristol").Run();

        // Assert
        result.Should().BeTrue();
    }

    private static MockOdkContext CreateMockOdkContext() => new();

    private static void Publish(Chapter chapter)
        => chapter.PublishedUtc = DateTime.UtcNow;
}
