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
    private const int ResendCooldownHours = 24;

    private const int RetentionDays = 90;

    [Test]
    public static async Task PurgeExpiredInvites_ActivatedMember_KeepsTheAccount()
    {
        /* Arrange - somebody chose to have an account that can sign in, so being invited somewhere and not
           replying is no reason to take it away. */
        using var context = CreateMockOdkContext();

        var chapter = context.CreateChapter(platform: PlatformType.GroupSquirrel);
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

        var chapter = context.CreateChapter(platform: PlatformType.GroupSquirrel);
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

        var expiredChapter = context.CreateChapter(platform: PlatformType.GroupSquirrel);
        var liveChapter = context.CreateChapter(platform: PlatformType.GroupSquirrel);
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

        var chapter = context.CreateChapter(platform: PlatformType.GroupSquirrel);
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

        var chapter = context.CreateChapter(platform: PlatformType.GroupSquirrel);
        var otherChapter = context.CreateChapter(platform: PlatformType.GroupSquirrel);
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

        var chapter = context.CreateChapter(platform: PlatformType.GroupSquirrel);

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

        var chapter = context.CreateChapter(platform: PlatformType.GroupSquirrel);
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

        var chapter = context.CreateChapter(platform: PlatformType.GroupSquirrel);
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

    [Test]
    public static async Task RequestInviteResend_SentInvitePastTheCooldown_SendsItAgain()
    {
        // Arrange
        using var context = CreateMockOdkContext();

        var chapter = context.CreateChapter();
        var member = context.CreateMember(activated: false);
        member.EmailAddress = "invited@example.com";
        context.SaveChanges();

        var receivedUtc = DateTime.UtcNow.AddDays(-10);
        CreateInvite(
            context,
            chapter.Id,
            member.Id,
            receivedUtc,
            sentUtc: DateTime.UtcNow.AddHours(-(ResendCooldownHours + 1)));

        var emailService = new Mock<IMemberEmailService>();
        var service = CreateService(context, emailService.Object);

        // Act
        var result = await service.RequestInviteResend(
            CreateChapterRequest(chapter), "invited@example.com");

        // Assert
        result.Success.Should().BeTrue();

        // The retention clock runs from when the details arrived, so only the send moves.
        var invite = context.Set<MemberChapterInvite>().Single(x => x.MemberId == member.Id);
        invite.CreatedUtc.Should().BeCloseTo(receivedUtc, TimeSpan.FromSeconds(1));
        invite.SentUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        emailService.Verify(
            x => x.SendMemberImportInviteEmail(
                It.IsAny<IChapterServiceRequest>(),
                It.Is<Member>(m => m.Id == member.Id),
                It.IsAny<string>()),
            Times.Once);
    }

    [Test]
    public static async Task RequestInviteResend_UnknownAddress_SucceedsWithoutSendingAnything()
    {
        /* Arrange - anyone can put an address into the form this comes from, so a miss has to be
           indistinguishable from a hit: same result, and the caller renders one wording for both. */
        using var context = CreateMockOdkContext();

        var chapter = context.CreateChapter();

        var emailService = new Mock<IMemberEmailService>();
        var service = CreateService(context, emailService.Object);

        // Act
        var result = await service.RequestInviteResend(
            CreateChapterRequest(chapter), "stranger@example.com");

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().BeNull();

        emailService.Verify(
            x => x.SendMemberImportInviteEmail(
                It.IsAny<IChapterServiceRequest>(), It.IsAny<Member>(), It.IsAny<string>()),
            Times.Never);
    }

    [Test]
    public static async Task RequestInviteResend_AddressWithNoInviteToThisGroup_SucceedsWithoutSendingAnything()
    {
        // Arrange - a member of the site who this group never invited is a new member, not a lost one.
        using var context = CreateMockOdkContext();

        var chapter = context.CreateChapter();
        var member = context.CreateMember();
        member.EmailAddress = "member@example.com";
        context.SaveChanges();

        var emailService = new Mock<IMemberEmailService>();
        var service = CreateService(context, emailService.Object);

        // Act
        var result = await service.RequestInviteResend(
            CreateChapterRequest(chapter), "member@example.com");

        // Assert
        result.Success.Should().BeTrue();

        emailService.Verify(
            x => x.SendMemberImportInviteEmail(
                It.IsAny<IChapterServiceRequest>(), It.IsAny<Member>(), It.IsAny<string>()),
            Times.Never);
    }

    [Test]
    public static async Task RequestInviteResend_HeldInvite_SendsNothing()
    {
        /* Arrange - an invite the group has never emailed is released by publishing the group. Letting a
           stranger's guess release it would take that decision away from the organisers. */
        using var context = CreateMockOdkContext();

        var chapter = context.CreateChapter();
        var member = context.CreateMember(activated: false);
        member.EmailAddress = "held@example.com";
        context.SaveChanges();

        CreateInvite(context, chapter.Id, member.Id, DateTime.UtcNow.AddDays(-1), sentUtc: null);

        var emailService = new Mock<IMemberEmailService>();
        var service = CreateService(context, emailService.Object);

        // Act
        var result = await service.RequestInviteResend(
            CreateChapterRequest(chapter), "held@example.com");

        // Assert
        result.Success.Should().BeTrue();

        context.Set<MemberChapterInvite>().Single(x => x.MemberId == member.Id).SentUtc.Should().BeNull();

        emailService.Verify(
            x => x.SendMemberImportInviteEmail(
                It.IsAny<IChapterServiceRequest>(), It.IsAny<Member>(), It.IsAny<string>()),
            Times.Never);
    }

    [Test]
    public static async Task RequestInviteResend_WithinTheCooldown_SendsNothing()
    {
        // Arrange - the cooldown is what stops the form being used to send somebody repeated invitations.
        using var context = CreateMockOdkContext();

        var chapter = context.CreateChapter();
        var member = context.CreateMember(activated: false);
        member.EmailAddress = "recent@example.com";
        context.SaveChanges();

        var sentUtc = DateTime.UtcNow.AddHours(-1);
        CreateInvite(context, chapter.Id, member.Id, DateTime.UtcNow.AddDays(-5), sentUtc: sentUtc);

        var emailService = new Mock<IMemberEmailService>();
        var service = CreateService(context, emailService.Object);

        // Act
        var result = await service.RequestInviteResend(
            CreateChapterRequest(chapter), "recent@example.com");

        // Assert
        result.Success.Should().BeTrue();

        context.Set<MemberChapterInvite>().Single(x => x.MemberId == member.Id)
            .SentUtc.Should().BeCloseTo(sentUtc, TimeSpan.FromSeconds(1));

        emailService.Verify(
            x => x.SendMemberImportInviteEmail(
                It.IsAny<IChapterServiceRequest>(), It.IsAny<Member>(), It.IsAny<string>()),
            Times.Never);
    }

    private static IChapterServiceRequest CreateChapterRequest(
        Chapter chapter, PlatformType platform = PlatformType.GroupSquirrel) =>
        Mock.Of<IChapterServiceRequest>(x =>
            x.Platform == platform &&
            x.Chapter == chapter &&
            x.HttpRequestContext == Mock.Of<IHttpRequestContext>());

    private static MemberChapterInvite CreateInvite(
        MockOdkContext context,
        Guid chapterId,
        Guid memberId,
        DateTime createdUtc,
        DateTime? sentUtc = null) => context.Create(
        new MemberChapterInvite
        {
            ChapterId = chapterId,
            CreatedUtc = createdUtc,
            Id = Guid.NewGuid(),
            MemberId = memberId,
            SentUtc = sentUtc,
            Token = Guid.NewGuid().ToString()
        });

    /* The platform is stated because the purge is scoped by it - an unset one is PlatformType.None, which
       matches nothing, so a member created without one would be skipped for the wrong reason. */
    private static Member CreateInvitedMember(MockOdkContext context, bool activated) =>
        context.CreateMember(activated: activated);

    /* Without tracking, the way the app reads: the purge deletes many rows at once, and a tracking context
       resolves two instances of one row to a single instance, which would hide a write the real one rejects. */
    private static MockOdkContext CreateMockOdkContext() => new MockOdkContext(noTracking: true);

    private static MemberInviteService CreateService(
        MockOdkContext context,
        IMemberEmailService? memberEmailService = null) => new MemberInviteService(
        MockUnitOfWorkFactory.Create(context),
        memberEmailService ?? Mock.Of<IMemberEmailService>(),
        new MockBackgroundTaskService(),
        new MockServiceRequestFactory(context),
        new MemberInviteServiceSettings
        {
            ResendCooldownHours = ResendCooldownHours,
            RetentionDays = RetentionDays
        });
}
