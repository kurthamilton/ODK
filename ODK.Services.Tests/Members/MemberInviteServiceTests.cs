using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Core.Platforms;
using ODK.Core.Web;
using ODK.Services.Members;
using ODK.Services.Tests.Helpers;

namespace ODK.Services.Tests.Members;

[Parallelizable]
public static class MemberInviteServiceTests
{
    private const int RetentionDays = 90;

    [Test]
    public static async Task PurgeExpiredInvites_ActivatedMember_KeepsTheAccount()
    {
        /* Arrange - somebody chose to have an account that can sign in, so being invited somewhere and not
           replying is no reason to take it away. */
        using var context = CreateMockOdkContext();

        var chapter = context.CreateChapter(platform: PlatformType.Default);
        var invited = CreateInvitedMember(context, activated: true);
        CreateInvite(context, chapter.Id, invited.Id, DateTime.UtcNow.AddDays(-100));

        var service = CreateService(context);

        // Act
        var purged = await service.PurgeExpiredInvites();

        // Assert
        purged.Should().Be(1);
        context.Set<MemberChapterInvite>().Any(x => x.MemberId == invited.Id).Should().BeFalse();
        context.Set<Member>().Any(x => x.Id == invited.Id).Should().BeTrue();
    }

    [Test]
    public static async Task PurgeExpiredInvites_ExpiredInvite_DeletesItAndTheAccountItRaised()
    {
        // Arrange
        using var context = CreateMockOdkContext();

        var chapter = context.CreateChapter(platform: PlatformType.Default);
        var invited = CreateInvitedMember(context, activated: false);
        CreateInvite(context, chapter.Id, invited.Id, DateTime.UtcNow.AddDays(-100));

        var service = CreateService(context);

        // Act
        var purged = await service.PurgeExpiredInvites();

        // Assert
        purged.Should().Be(1);
        context.Set<MemberChapterInvite>().Any(x => x.MemberId == invited.Id).Should().BeFalse();
        context.Set<Member>().Any(x => x.Id == invited.Id).Should().BeFalse();
    }

    [Test]
    public static async Task PurgeExpiredInvites_OneInviteStillOutstanding_KeepsTheAccount()
    {
        /* Arrange - the account goes only once nothing is still asking that member to join, or a member
           invited to two groups loses the account the second invite is waiting on. */
        using var context = CreateMockOdkContext();

        var expiredChapter = context.CreateChapter(platform: PlatformType.Default);
        var liveChapter = context.CreateChapter(platform: PlatformType.Default);
        var invited = CreateInvitedMember(context, activated: false);

        CreateInvite(context, expiredChapter.Id, invited.Id, DateTime.UtcNow.AddDays(-100));
        var live = CreateInvite(context, liveChapter.Id, invited.Id, DateTime.UtcNow.AddDays(-1));

        var service = CreateService(context);

        // Act
        var purged = await service.PurgeExpiredInvites();

        // Assert
        purged.Should().Be(1);
        context.Set<MemberChapterInvite>()
            .Where(x => x.MemberId == invited.Id)
            .Select(x => x.Id)
            .Should()
            .BeEquivalentTo(new[] { live.Id });
        context.Set<Member>().Any(x => x.Id == invited.Id).Should().BeTrue();
    }

    [Test]
    public static async Task PurgeExpiredInvites_WithinTheRetention_LeavesItAlone()
    {
        // Arrange
        using var context = CreateMockOdkContext();

        var chapter = context.CreateChapter(platform: PlatformType.Default);
        var invited = CreateInvitedMember(context, activated: false);
        CreateInvite(context, chapter.Id, invited.Id, DateTime.UtcNow.AddDays(-89));

        var service = CreateService(context);

        // Act
        var purged = await service.PurgeExpiredInvites();

        // Assert
        purged.Should().Be(0);
        context.Set<MemberChapterInvite>().Any(x => x.MemberId == invited.Id).Should().BeTrue();
        context.Set<Member>().Any(x => x.Id == invited.Id).Should().BeTrue();
    }

    [Test]
    public static async Task RefuseInvite_AnotherGroupsToken_Fails()
    {
        /* Arrange - the token names the invite being spent, and the page it was posted from belongs to
           this group, so one for somewhere else is refused rather than honoured. */
        using var context = CreateMockOdkContext();

        var chapter = context.CreateChapter(platform: PlatformType.Default);
        var otherChapter = context.CreateChapter(platform: PlatformType.Default);
        var invited = CreateInvitedMember(context, activated: false);
        var invite = CreateInvite(context, otherChapter.Id, invited.Id, DateTime.UtcNow);

        var service = CreateService(context);

        // Act
        var result = await service.RefuseInvite(CreateChapterRequest(chapter), invite.Token);

        // Assert
        result.Success.Should().BeFalse();
        context.Set<MemberChapterInvite>().Any(x => x.Id == invite.Id).Should().BeTrue();
        context.Set<Member>().Any(x => x.Id == invited.Id).Should().BeTrue();
    }

    [Test]
    public static async Task RefuseInvite_UnknownToken_Fails()
    {
        // Arrange
        using var context = CreateMockOdkContext();

        var chapter = context.CreateChapter(platform: PlatformType.Default);

        var service = CreateService(context);

        // Act
        var result = await service.RefuseInvite(CreateChapterRequest(chapter), "not-a-token");

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("The link you followed is no longer valid");
    }

    [Test]
    public static async Task RefuseInvite_ValidToken_DeletesTheInviteAndTheAccountItRaised()
    {
        // Arrange
        using var context = CreateMockOdkContext();

        var chapter = context.CreateChapter(platform: PlatformType.Default);
        var invited = CreateInvitedMember(context, activated: false);
        var invite = CreateInvite(context, chapter.Id, invited.Id, DateTime.UtcNow);

        var service = CreateService(context);

        // Act
        var result = await service.RefuseInvite(CreateChapterRequest(chapter), invite.Token);

        // Assert
        result.Success.Should().BeTrue();
        context.Set<MemberChapterInvite>().Any(x => x.Id == invite.Id).Should().BeFalse();
        context.Set<Member>().Any(x => x.Id == invited.Id).Should().BeFalse();
    }

    [Test]
    public static async Task RefuseInvite_ValidTokenForActivatedMember_KeepsTheAccount()
    {
        // Arrange
        using var context = CreateMockOdkContext();

        var chapter = context.CreateChapter(platform: PlatformType.Default);
        var invited = CreateInvitedMember(context, activated: true);
        var invite = CreateInvite(context, chapter.Id, invited.Id, DateTime.UtcNow);

        var service = CreateService(context);

        // Act
        var result = await service.RefuseInvite(CreateChapterRequest(chapter), invite.Token);

        // Assert
        result.Success.Should().BeTrue();
        context.Set<MemberChapterInvite>().Any(x => x.Id == invite.Id).Should().BeFalse();
        context.Set<Member>().Any(x => x.Id == invited.Id).Should().BeTrue();
    }

    private static IChapterServiceRequest CreateChapterRequest(
        Chapter chapter, PlatformType platform = PlatformType.Default) =>
        Mock.Of<IChapterServiceRequest>(x =>
            x.Platform == platform &&
            x.Chapter == chapter &&
            x.HttpRequestContext == Mock.Of<IHttpRequestContext>());

    private static MemberChapterInvite CreateInvite(
        MockOdkContext context, Guid chapterId, Guid memberId, DateTime createdUtc) => context.Create(
        new MemberChapterInvite
        {
            ChapterId = chapterId,
            CreatedUtc = createdUtc,
            Id = Guid.NewGuid(),
            MemberId = memberId,
            Token = Guid.NewGuid().ToString()
        });

    /* The platform is stated because the purge is scoped by it - an unset one is PlatformType.None, which
       matches nothing, so a member created without one would be skipped for the wrong reason. */
    private static Member CreateInvitedMember(MockOdkContext context, bool activated) =>
        context.CreateMember(activated: activated);

    /* Without tracking, the way the app reads: the purge deletes many rows at once, and a tracking context
       resolves two instances of one row to a single instance, which would hide a write the real one rejects. */
    private static MockOdkContext CreateMockOdkContext() => new MockOdkContext(noTracking: true);

    private static MemberInviteService CreateService(MockOdkContext context) => new MemberInviteService(
        MockUnitOfWorkFactory.Create(context),
        new MemberInviteServiceSettings { RetentionDays = RetentionDays });
}
