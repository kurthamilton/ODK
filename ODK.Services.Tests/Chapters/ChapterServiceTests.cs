using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Core.Platforms;
using ODK.Services.Chapters;
using ODK.Services.Payments;
using ODK.Services.Tests.Helpers;

namespace ODK.Services.Tests.Chapters;

[Parallelizable]
public static class ChapterServiceTests
{
    [Test]
    public static async Task GetDefaultChapter_WhenMemberInMultipleChapters_ReturnsTheEarliestJoined()
    {
        // Arrange
        using var context = new MockOdkContext();

        var member = context.CreateMember();
        var first = CreateDrunkenKnitwitsChapter(context, member);
        var second = CreateDrunkenKnitwitsChapter(context, member);

        SetJoinedDate(member, first, DateTime.UtcNow.AddDays(-2));
        SetJoinedDate(member, second, DateTime.UtcNow.AddDays(-1));

        var service = CreateChapterService(context);

        // Act
        var result = await service.GetDefaultChapter(
            CreateMemberServiceRequest(member, PlatformType.DrunkenKnitwits));

        // Assert
        result!.Id.Should().Be(first.Id);
    }

    [Test]
    public static async Task GetSoleChapter_WhenMemberInSinglePublishedChapter_ReturnsTheChapter()
    {
        // Arrange
        using var context = new MockOdkContext();

        var member = context.CreateMember();
        var chapter = CreateDrunkenKnitwitsChapter(context, member);

        var service = CreateChapterService(context);

        // Act
        var result = await service.GetSoleChapter(
            CreateMemberServiceRequest(member, PlatformType.DrunkenKnitwits));

        // Assert
        result!.Id.Should().Be(chapter.Id);
    }

    [Test]
    public static async Task GetSoleChapter_WhenMemberInMultipleChapters_ReturnsNull()
    {
        // Arrange
        using var context = new MockOdkContext();

        var member = context.CreateMember();
        CreateDrunkenKnitwitsChapter(context, member);
        CreateDrunkenKnitwitsChapter(context, member);

        var service = CreateChapterService(context);

        // Act
        var result = await service.GetSoleChapter(
            CreateMemberServiceRequest(member, PlatformType.DrunkenKnitwits));

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public static async Task GetSoleChapter_WhenMemberInNoChapters_ReturnsNull()
    {
        // Arrange
        using var context = new MockOdkContext();

        var member = context.CreateMember();

        var service = CreateChapterService(context);

        // Act
        var result = await service.GetSoleChapter(
            CreateMemberServiceRequest(member, PlatformType.DrunkenKnitwits));

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public static async Task GetSoleChapter_WhenSoleChapterUnpublished_ReturnsNull()
    {
        // Arrange - Drunken Knitwits chapters are read regardless of publication state, so an
        // unpublished chapter reaches the service and has to be excluded there.
        using var context = new MockOdkContext();

        var member = context.CreateMember();
        CreateDrunkenKnitwitsChapter(context, member, published: false);

        var service = CreateChapterService(context);

        // Act
        var result = await service.GetSoleChapter(
            CreateMemberServiceRequest(member, PlatformType.DrunkenKnitwits));

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public static async Task GetSoleChapter_WhenSoleChapterOnAnotherPlatform_ReturnsNull()
    {
        // Arrange
        using var context = new MockOdkContext();

        var member = context.CreateMember();
        context.CreateChapter(members: [member], afterCreate: x => x.PublishedUtc = DateTime.UtcNow);

        var service = CreateChapterService(context);

        // Act
        var result = await service.GetSoleChapter(
            CreateMemberServiceRequest(member, PlatformType.DrunkenKnitwits));

        // Assert
        result.Should().BeNull();
    }

    private static ChapterService CreateChapterService(MockOdkContext context)
        => new(MockUnitOfWorkFactory.Create(context), Mock.Of<IPaymentProviderFactory>());

    private static Chapter CreateDrunkenKnitwitsChapter(
        MockOdkContext context,
        Member member,
        bool published = true)
        => context.CreateChapter(members: [member], afterCreate: x =>
        {
            x.Platform = PlatformType.DrunkenKnitwits;
            x.PublishedUtc = published ? DateTime.UtcNow : null;
        });

    private static IMemberServiceRequest CreateMemberServiceRequest(Member currentMember, PlatformType platform)
    {
        var mock = new Mock<IMemberServiceRequest>();

        mock.Setup(x => x.CurrentMember)
            .Returns(currentMember);

        mock.Setup(x => x.Platform)
            .Returns(platform);

        return mock.Object;
    }

    private static void SetJoinedDate(Member member, Chapter chapter, DateTime joinedUtc)
        => member.Chapters.Single(x => x.ChapterId == chapter.Id).CreatedUtc = joinedUtc;
}
