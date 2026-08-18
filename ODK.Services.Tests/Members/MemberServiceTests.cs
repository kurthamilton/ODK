using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using ODK.Core.Workflows;
using ODK.Services.Members.Workflows;
using ODK.Services.Workflows;
using ODK.Data.Core;
using NUnit.Framework;
using ODK.Core.Chapters;
using ODK.Core.DataTypes;
using ODK.Core.Countries;
using ODK.Core.Features;
using ODK.Core.Members;
using ODK.Core.Platforms;
using ODK.Core.Referrals;
using ODK.Core.Subscriptions;
using ODK.Core.Web;
using ODK.Services.Authentication.OAuth;
using ODK.Services.Authorization;
using ODK.Services.Emails;
using ODK.Services.Emails.Validation;
using ODK.Services.Geolocation;
using ODK.Services.Logging;
using ODK.Services.Members;
using ODK.Services.Members.Models;
using ODK.Services.Notifications;
using ODK.Services.Payments;
using ODK.Services.Recaptcha;
using ODK.Services.Subscriptions;
using ODK.Services.Tests.Helpers;
using ODK.Services.Topics;

namespace ODK.Services.Tests.Members;

[Parallelizable]
public static class MemberServiceTests
{
    [Test]
    public static async Task CreateAccount_ExistingActivatedMember_SendsDuplicateEmail()
    {
        // Arrange
        using var context = CreateMockOdkContext();
        SeedDefaultSiteSubscription(context);

        var existing = context.CreateMember(activated: true, afterCreate: x => x.EmailAddress = "existing@example.com");

        var emailService = new Mock<IMemberEmailService>();
        var service = CreateMemberService(context, emailService.Object);
        var request = Mock.Of<IServiceRequest>(x =>
            x.Platform == PlatformType.Default &&
            x.HttpRequestContext == Mock.Of<IHttpRequestContext>());

        // Act
        var result = await service.CreateAccount(request, CreateModel("existing@example.com", firstName: "New"));

        // Assert
        result.Success.Should().BeTrue();
        emailService.Verify(
            x => x.SendDuplicateMemberEmail(request, null, existing),
            Times.Once);
        emailService.Verify(
            x => x.SendActivationEmail(It.IsAny<IServiceRequest>(), It.IsAny<Chapter?>(), It.IsAny<Member>(), It.IsAny<string>()),
            Times.Never);
    }

    [Test]
    public static async Task CreateAccount_RejectedEmailAddress_FailsWithTheReasonRatherThanReportingSuccess()
    {
        // Arrange - the pair to the test above, and the distinction the web layer keys off. An address
        // that already holds an account reports success so it can't be probed for; an address rejected on
        // its own merits has to report failure, or the member is sent to wait on an email nobody sent.
        using var context = CreateMockOdkContext();
        SeedDefaultSiteSubscription(context);

        var verifier = new Mock<IEmailVerifier>();
        verifier.Setup(x => x.Verify(It.IsAny<string>())).ReturnsAsync(EmailVerificationResult.Invalid);

        var emailService = new Mock<IMemberEmailService>();
        var service = CreateMemberService(context, emailService.Object, verifier.Object);
        var request = Mock.Of<IServiceRequest>(x =>
            x.Platform == PlatformType.Default &&
            x.HttpRequestContext == Mock.Of<IHttpRequestContext>());

        // Act
        var result = await service.CreateAccount(request, CreateModel("rejected@example.com", firstName: "New"));

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Email address could not be verified");
        emailService.Verify(
            x => x.SendActivationEmail(It.IsAny<IServiceRequest>(), It.IsAny<Chapter?>(), It.IsAny<Member>(), It.IsAny<string>()),
            Times.Never);
    }

    [Test]
    public static async Task CreateAccount_ExistingUnactivatedMember_RecreatesWithLatestInfoAndReusesActivationToken()
    {
        // Arrange - an unactivated account with an activation token already emailed to the member.
        using var context = CreateMockOdkContext();
        SeedDefaultSiteSubscription(context);

        var existing = context.CreateMember(activated: false, afterCreate: x =>
        {
            x.EmailAddress = "existing@example.com";
            x.FirstName = "Old";
        });
        context.Create(new MemberActivationToken
        {
            ActivationToken = "original-token",
            MemberId = existing.Id
        });

        var emailService = new Mock<IMemberEmailService>();
        var service = CreateMemberService(context, emailService.Object);
        var request = Mock.Of<IServiceRequest>(x =>
            x.Platform == PlatformType.Default &&
            x.HttpRequestContext == Mock.Of<IHttpRequestContext>());

        // Act
        var result = await service.CreateAccount(request, CreateModel("existing@example.com", firstName: "New"));

        // Assert - the account is recreated from the latest details, keeping the original token so the
        // already-emailed activation link still works.
        result.Success.Should().BeTrue();

        var member = context.Set<Member>().Single(x => x.EmailAddress == "existing@example.com");
        member.FirstName.Should().Be("New");
        context.Set<MemberActivationToken>()
            .Should().Contain(x => x.ActivationToken == "original-token" && x.MemberId == member.Id);

        emailService.Verify(
            x => x.SendActivationEmail(request, null, It.Is<Member>(m => m.FirstName == "New"), "original-token"),
            Times.Once);
        emailService.Verify(
            x => x.SendDuplicateMemberEmail(It.IsAny<IServiceRequest>(), It.IsAny<Chapter?>(), It.IsAny<Member>()),
            Times.Never);
    }

    [Test]
    public static async Task CreateAccount_ExistingUnactivatedMemberWithoutToken_RecreatesAndSendsFreshActivationEmail()
    {
        // Arrange - an unactivated account whose activation token is missing (edge case).
        using var context = CreateMockOdkContext();
        SeedDefaultSiteSubscription(context);

        context.CreateMember(activated: false, afterCreate: x =>
        {
            x.EmailAddress = "existing@example.com";
            x.FirstName = "Old";
        });

        var emailService = new Mock<IMemberEmailService>();
        var service = CreateMemberService(context, emailService.Object);
        var request = Mock.Of<IServiceRequest>(x =>
            x.Platform == PlatformType.Default &&
            x.HttpRequestContext == Mock.Of<IHttpRequestContext>());

        // Act
        var result = await service.CreateAccount(request, CreateModel("existing@example.com", firstName: "New"));

        // Assert - a fresh token is generated and the activation email is sent with it.
        result.Success.Should().BeTrue();
        var member = context.Set<Member>().Single(x => x.EmailAddress == "existing@example.com");
        member.FirstName.Should().Be("New");
        emailService.Verify(
            x => x.SendActivationEmail(request, null, It.IsAny<Member>(), It.Is<string>(t => !string.IsNullOrEmpty(t))),
            Times.Once);
    }

    [Test]
    public static async Task CreateAccount_KnownReferralId_AttributesTheSignup()
    {
        // Arrange
        using var context = CreateMockOdkContext();
        SeedDefaultSiteSubscription(context);
        var referral = CreateReferral(context);

        var service = CreateMemberService(context, new Mock<IMemberEmailService>().Object);
        var request = CreateSiteRequest();

        // Act
        var result = await service.CreateAccount(
            request, CreateModel("new@example.com", firstName: "New", referralId: referral.Id));

        // Assert
        result.Success.Should().BeTrue();
        context.Set<Member>().Single(x => x.EmailAddress == "new@example.com")
            .ReferralId.Should().Be(referral.Id);
    }

    [Test]
    public static async Task CreateAccount_AlreadyCompletedReferralId_SignsUpUnattributed()
    {
        // Arrange - the referral already brought in a member, so it cannot bring in another. Without this
        // an id left in local storage from an earlier signup would attribute a later one too.
        using var context = CreateMockOdkContext();
        SeedDefaultSiteSubscription(context);
        var referral = CreateReferral(context, completed: true);

        var service = CreateMemberService(context, new Mock<IMemberEmailService>().Object);
        var request = CreateSiteRequest();

        // Act
        var result = await service.CreateAccount(
            request, CreateModel("new@example.com", firstName: "New", referralId: referral.Id));

        // Assert
        result.Success.Should().BeTrue();
        context.Set<Member>().Single(x => x.EmailAddress == "new@example.com")
            .ReferralId.Should().BeNull();
    }

    [Test]
    public static async Task CreateAccount_UnknownReferralId_SignsUpUnattributedRatherThanFailing()
    {
        // Arrange - the id reaches the server from a hidden form field, so a stale or tampered value must
        // be discarded. Storing it blindly would fail the signup on a foreign key violation.
        using var context = CreateMockOdkContext();
        SeedDefaultSiteSubscription(context);

        var service = CreateMemberService(context, new Mock<IMemberEmailService>().Object);
        var request = CreateSiteRequest();

        // Act
        var result = await service.CreateAccount(
            request, CreateModel("new@example.com", firstName: "New", referralId: Guid.NewGuid()));

        // Assert
        result.Success.Should().BeTrue();
        context.Set<Member>().Single(x => x.EmailAddress == "new@example.com")
            .ReferralId.Should().BeNull();
    }

    [Test]
    public static async Task CreateAccount_NewMember_SavesTheRequestLocale()
    {
        // Arrange - a brand new sign-up whose request resolves to the fr-FR locale.
        using var context = CreateMockOdkContext();
        SeedDefaultSiteSubscription(context);

        var service = CreateMemberService(context, new Mock<IMemberEmailService>().Object);
        var request = Mock.Of<IServiceRequest>(x =>
            x.Platform == PlatformType.Default &&
            x.HttpRequestContext == Mock.Of<IHttpRequestContext>(c => c.Locale == "fr-FR"));

        // Act
        var result = await service.CreateAccount(request, CreateModel("new@example.com", firstName: "New"));

        // Assert - the request locale is stored on the member's preferences at creation.
        result.Success.Should().BeTrue();
        var member = context.Set<Member>().Single(x => x.EmailAddress == "new@example.com");
        context.Set<MemberPreferences>().Single(x => x.MemberId == member.Id).Locale.Should().Be("fr-FR");
    }

    [Test]
    public static async Task CreateChapterAccount_ExistingActivatedMember_SendsDuplicateEmail()
    {
        // Arrange
        using var context = CreateMockOdkContext();
        SeedDefaultSiteSubscription(context, PlatformType.DrunkenKnitwits);
        var chapter = context.CreateChapter();

        var existing = context.CreateMember(activated: true, afterCreate: x => x.EmailAddress = "existing@example.com");

        var emailService = new Mock<IMemberEmailService>();
        var service = CreateMemberService(context, emailService.Object);
        var request = CreateChapterRequest(chapter);

        // Act
        var result = await service.CreateChapterAccount(request, CreateChapterProfile("existing@example.com", firstName: "New"));

        // Assert
        result.Success.Should().BeTrue();
        emailService.Verify(
            x => x.SendDuplicateMemberEmail(request, chapter, existing),
            Times.Once);
        emailService.Verify(
            x => x.SendActivationEmail(It.IsAny<IServiceRequest>(), It.IsAny<Chapter?>(), It.IsAny<Member>(), It.IsAny<string>()),
            Times.Never);
    }

    [Test]
    public static async Task CreateChapterAccount_ExistingUnactivatedMember_RecreatesWithLatestInfoAndReusesActivationToken()
    {
        // Arrange
        using var context = CreateMockOdkContext();
        SeedDefaultSiteSubscription(context, PlatformType.DrunkenKnitwits);
        var chapter = context.CreateChapter();

        var existing = context.CreateMember(activated: false, afterCreate: x =>
        {
            x.EmailAddress = "existing@example.com";
            x.FirstName = "Old";
        });
        context.Create(new MemberActivationToken
        {
            ActivationToken = "original-token",
            ChapterId = chapter.Id,
            MemberId = existing.Id
        });

        var emailService = new Mock<IMemberEmailService>();
        var service = CreateMemberService(context, emailService.Object);
        var request = CreateChapterRequest(chapter);

        // Act
        var result = await service.CreateChapterAccount(request, CreateChapterProfile("existing@example.com", firstName: "New"));

        // Assert - recreated from the latest details, keeping the original activation token.
        result.Success.Should().BeTrue();

        var member = context.Set<Member>().Single(x => x.EmailAddress == "existing@example.com");
        member.FirstName.Should().Be("New");
        context.Set<MemberActivationToken>()
            .Should().Contain(x => x.ActivationToken == "original-token" && x.MemberId == member.Id);

        emailService.Verify(
            x => x.SendActivationEmail(request, chapter, It.Is<Member>(m => m.FirstName == "New"), "original-token"),
            Times.Once);
        emailService.Verify(
            x => x.SendDuplicateMemberEmail(It.IsAny<IServiceRequest>(), It.IsAny<Chapter?>(), It.IsAny<Member>()),
            Times.Never);
    }

    [Test]
    public static async Task CreateChapterAccount_InvitedMemberKeepsInvitedAddress_ReturnsActivationTokenUnemailed()
    {
        /* Arrange - an imported member following their invitation link and registering the address it was sent
           to. Holding the token proves they read mail at that address, which is the only thing an activation
           email establishes, so they are handed the token instead of being made to wait for one. */
        using var context = CreateMockOdkContext();
        SeedDefaultSiteSubscription(context, PlatformType.DrunkenKnitwits);
        var chapter = context.CreateChapter();

        var existing = context.CreateMember(activated: false, afterCreate: x =>
        {
            x.EmailAddress = "invited@example.com";
            x.FirstName = "Imported";
        });
        context.Create(new MemberActivationToken
        {
            ActivationToken = "original-token",
            ChapterId = chapter.Id,
            MemberId = existing.Id
        });
        context.Create(new MemberChapterInvite
        {
            ChapterId = chapter.Id,
            CreatedUtc = DateTime.UtcNow,
            Id = Guid.NewGuid(),
            MemberId = existing.Id,
            Token = "invite-token"
        });

        var emailService = new Mock<IMemberEmailService>();
        var service = CreateMemberService(context, emailService.Object);
        var request = CreateChapterRequest(chapter);

        // Act
        var result = await service.CreateChapterAccount(
            request,
            CreateChapterProfile("invited@example.com", firstName: "Invited", inviteToken: "invite-token"));

        // Assert
        result.Success.Should().BeTrue();
        result.ActivationToken.Should().Be("original-token");

        emailService.Verify(
            x => x.SendActivationEmail(It.IsAny<IServiceRequest>(), It.IsAny<Chapter?>(), It.IsAny<Member>(), It.IsAny<string>()),
            Times.Never);

        // Consumed by joining: the membership row is now the record that they accepted.
        context.Set<MemberChapterInvite>()
            .Any(x => x.ChapterId == chapter.Id)
            .Should()
            .BeFalse();
    }

    [Test]
    public static async Task CreateChapterAccount_InvitedMemberChangesEmailAddress_SendsActivationEmail()
    {
        /* Arrange - the same link, but registering a different address. The token says nothing about an address
           it was not sent to, so this falls back to proving the new one the usual way. */
        using var context = CreateMockOdkContext();
        SeedDefaultSiteSubscription(context, PlatformType.DrunkenKnitwits);
        var chapter = context.CreateChapter();

        var existing = context.CreateMember(activated: false, afterCreate: x =>
            x.EmailAddress = "invited@example.com");
        context.Create(new MemberChapterInvite
        {
            ChapterId = chapter.Id,
            CreatedUtc = DateTime.UtcNow,
            Id = Guid.NewGuid(),
            MemberId = existing.Id,
            Token = "invite-token"
        });

        var emailService = new Mock<IMemberEmailService>();
        var service = CreateMemberService(context, emailService.Object);
        var request = CreateChapterRequest(chapter);

        // Act
        var result = await service.CreateChapterAccount(
            request,
            CreateChapterProfile("mistyped@example.com", firstName: "Invited", inviteToken: "invite-token"));

        // Assert
        result.Success.Should().BeTrue();
        result.ActivationToken.Should().BeNull();

        var member = context.Set<Member>().Single(x => x.EmailAddress == "mistyped@example.com");
        emailService.Verify(
            x => x.SendActivationEmail(request, chapter, It.Is<Member>(m => m.Id == member.Id), It.IsAny<string>()),
            Times.Once);
    }

    [Test]
    public static async Task CreateChapterAccount_InvitationToAnotherChapter_CarriesItOverWithItsToken()
    {
        /* Arrange - joining one group discards and recreates the unactivated account, which would cascade away an
           invitation from a second group. It is re-raised with its own token so the link already emailed for that
           group still resolves. */
        using var context = CreateMockOdkContext();
        SeedDefaultSiteSubscription(context, PlatformType.DrunkenKnitwits);
        var chapter = context.CreateChapter();
        var otherChapter = context.CreateChapter();

        var existing = context.CreateMember(activated: false, afterCreate: x =>
            x.EmailAddress = "invited@example.com");
        context.Create(new MemberChapterInvite
        {
            ChapterId = chapter.Id,
            CreatedUtc = DateTime.UtcNow,
            Id = Guid.NewGuid(),
            MemberId = existing.Id,
            Token = "invite-token"
        });
        context.Create(new MemberChapterInvite
        {
            ChapterId = otherChapter.Id,
            CreatedUtc = DateTime.UtcNow,
            Id = Guid.NewGuid(),
            MemberId = existing.Id,
            Token = "other-invite-token"
        });

        var service = CreateMemberService(context, new Mock<IMemberEmailService>().Object);
        var request = CreateChapterRequest(chapter);

        // Act
        var result = await service.CreateChapterAccount(
            request,
            CreateChapterProfile("invited@example.com", firstName: "Invited", inviteToken: "invite-token"));

        // Assert
        result.Success.Should().BeTrue();

        var member = context.Set<Member>().Single(x => x.EmailAddress == "invited@example.com");
        context.Set<MemberChapterInvite>()
            .Should()
            .ContainSingle(x => x.MemberId == member.Id)
            .Which
            .Should()
            .Match<MemberChapterInvite>(x =>
                x.ChapterId == otherChapter.Id &&
                x.Token == "other-invite-token");
    }

    [Test]
    public static async Task CreateChapterAccount_RequiredGroupQuestionNotAnswered_Fails()
    {
        /* Arrange - signing up to a group is the same act as joining one, so it enforces the group's required
           questions the way JoinChapter does. */
        using var context = CreateMockOdkContext();
        SeedDefaultSiteSubscription(context, PlatformType.DrunkenKnitwits);
        var chapter = context.CreateChapter();
        context.Create(new ChapterProperty
        {
            ChapterId = chapter.Id,
            DataType = DataType.Text,
            Id = Guid.NewGuid(),
            Label = "Favourite yarn",
            Name = "yarn",
            Required = true
        });

        var service = CreateMemberService(context, new Mock<IMemberEmailService>().Object);

        // Act - the profile answers nothing.
        var result = await service.CreateChapterAccount(
            CreateChapterRequest(chapter),
            CreateChapterProfile("new@example.com", firstName: "New"));

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("The following properties are required: Favourite yarn");
        context.Set<Member>().Any(x => x.EmailAddress == "new@example.com").Should().BeFalse();
    }

    [Test]
    public static async Task JoinChapter_InvitedMember_SkipsApprovalAndConsumesTheInvitation()
    {
        /* Arrange - a group that approves new members, and a member it has invited. This is how an invitation is
           accepted by someone who already has an account: they sign in and submit the group's questions. Putting
           them in the approvals queue would ask the group to approve someone it asked to join. */
        using var context = CreateMockOdkContext();

        var owner = context.CreateMember();
        var chapter = context.CreateChapter(owner: owner, siteSubscription: context.CreateSiteSubscription());
        context.Create(new ChapterMembershipSettings
        {
            ApproveNewMembers = true,
            ChapterId = chapter.Id,
            Enabled = true
        });

        var member = context.CreateMember(afterCreate: x => x.EmailAddress = "invited@example.com");
        context.Create(new MemberChapterInvite
        {
            ChapterId = chapter.Id,
            CreatedUtc = DateTime.UtcNow,
            Id = Guid.NewGuid(),
            MemberId = member.Id,
            Token = "invite-token"
        });

        var service = CreateMemberService(
            context,
            new Mock<IMemberEmailService>().Object,
            authorizationService: CreateMockAuthorizationService(chapterHasAccess: true));

        // Act
        var result = await service.JoinChapter(CreateMemberChapterRequest(chapter, member), []);

        // Assert
        result.Success.Should().BeTrue();

        context.Set<MemberChapter>()
            .Should()
            .ContainSingle(x => x.MemberId == member.Id && x.ChapterId == chapter.Id)
            .Which
            .Approved
            .Should()
            .BeTrue();

        // Consumed, so they are not listed as invited to a group they are now in.
        context.Set<MemberChapterInvite>()
            .Any(x => x.MemberId == member.Id && x.ChapterId == chapter.Id)
            .Should()
            .BeFalse();
    }

    [Test]
    public static async Task JoinChapter_MemberWithNoInvitation_StillNeedsApproving()
    {
        // Arrange - the pair to the test above: without an invitation the group's own setting decides.
        using var context = CreateMockOdkContext();

        var owner = context.CreateMember();
        var chapter = context.CreateChapter(owner: owner, siteSubscription: context.CreateSiteSubscription());
        context.Create(new ChapterMembershipSettings
        {
            ApproveNewMembers = true,
            ChapterId = chapter.Id,
            Enabled = true
        });

        var member = context.CreateMember(afterCreate: x => x.EmailAddress = "applicant@example.com");

        var service = CreateMemberService(
            context,
            new Mock<IMemberEmailService>().Object,
            authorizationService: CreateMockAuthorizationService(chapterHasAccess: true));

        // Act
        var result = await service.JoinChapter(CreateMemberChapterRequest(chapter, member), []);

        // Assert
        result.Success.Should().BeTrue();

        context.Set<MemberChapter>()
            .Should()
            .ContainSingle(x => x.MemberId == member.Id && x.ChapterId == chapter.Id)
            .Which
            .Approved
            .Should()
            .BeFalse();
    }

    [Test]
    public static async Task JoinChapter_MemberAlreadyInTheGroup_Fails()
    {
        /* Arrange - the machine has no Join edge out of a state that holds a membership, so this is refused by
           the graph rather than by a check. Only the wording belongs to the service. */
        using var context = CreateMockOdkContext();

        var owner = context.CreateMember();
        var chapter = context.CreateChapter(owner: owner, siteSubscription: context.CreateSiteSubscription());

        var member = context.CreateMember(afterCreate: x => x.EmailAddress = "existing@example.com");
        member.Chapters.Add(new MemberChapter
        {
            Approved = true,
            ChapterId = chapter.Id,
            CreatedUtc = DateTime.UtcNow,
            MemberId = member.Id
        });

        var service = CreateMemberService(context, new Mock<IMemberEmailService>().Object);

        // Act
        var result = await service.JoinChapter(CreateMemberChapterRequest(chapter, member), []);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("You are already a member of this group");
    }

    [Test]
    public static async Task JoinChapter_GroupAtItsMemberLimit_FailsWithoutWritingAnything()
    {
        // Arrange - the capacity check is the first step, so nothing after it should have run.
        using var context = CreateMockOdkContext();

        var owner = context.CreateMember();
        var chapter = context.CreateChapter(
            owner: owner,
            siteSubscription: context.CreateSiteSubscription(memberLimit: 1));

        // The one place the subscription allows is already taken.
        var existing = context.CreateMember(afterCreate: x => x.EmailAddress = "taken@example.com");
        context.Create(new MemberChapter
        {
            Approved = true,
            ChapterId = chapter.Id,
            CreatedUtc = DateTime.UtcNow,
            Id = Guid.NewGuid(),
            MemberId = existing.Id
        });

        var member = context.CreateMember(afterCreate: x => x.EmailAddress = "applicant@example.com");

        var service = CreateMemberService(context, new Mock<IMemberEmailService>().Object);

        // Act
        var result = await service.JoinChapter(CreateMemberChapterRequest(chapter, member), []);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("This group is not able to welcome any new members");
        context.Set<MemberChapter>().Any(x => x.MemberId == member.Id).Should().BeFalse();
    }

    private static IChapterServiceRequest CreateChapterRequest(Chapter chapter) =>
        Mock.Of<IChapterServiceRequest>(x =>
            x.Platform == PlatformType.DrunkenKnitwits &&
            x.Chapter == chapter &&
            x.HttpRequestContext == Mock.Of<IHttpRequestContext>());

    private static MemberCreateProfile CreateChapterProfile(
        string emailAddress, string firstName, string? inviteToken = null) => new MemberCreateProfile
    {
        EmailAddress = emailAddress,
        FirstName = firstName,
        InviteToken = inviteToken,
        LastName = "Member",
        ImageData = [1, 2, 3]
    };

    private static Referral CreateReferral(MockOdkContext context, bool completed = false)
    {
        var referrer = context.CreateMember(afterCreate: x => x.EmailAddress = "referrer@example.com");
        var campaign = context.Create(new ReferralCampaign
        {
            CreatedUtc = DateTime.UtcNow.AddDays(-1),
            Id = Guid.NewGuid(),
            Name = "Spring drive"
        });

        return context.Create(new Referral
        {
            CompletedUtc = completed ? DateTime.UtcNow : null,
            CreatedUtc = DateTime.UtcNow,
            EmailAddress = "new@example.com",
            Id = Guid.NewGuid(),
            MemberId = referrer.Id,
            ReferralCampaignId = campaign.Id
        });
    }

    private static IServiceRequest CreateSiteRequest() => Mock.Of<IServiceRequest>(x =>
        x.Platform == PlatformType.Default &&
        x.HttpRequestContext == Mock.Of<IHttpRequestContext>());

    private static AccountCreateModel CreateModel(
        string emailAddress, string firstName, Guid? referralId = null) => new AccountCreateModel
    {
        EmailAddress = emailAddress,
        ReferralId = referralId,
        FirstName = firstName,
        LastName = "Member",
        Location = null,
        LocationName = "",
        NewTopics = [],
        OAuthProviderType = null,
        OAuthToken = null,
        RecaptchaToken = string.Empty,
        TopicIds = []
    };

    private static MemberService CreateMemberService(
        MockOdkContext context,
        IMemberEmailService memberEmailService,
        IEmailVerifier? emailVerifier = null,
        IAuthorizationService? authorizationService = null)
    {
        var memberImageService = new Mock<IMemberImageService>();
        memberImageService
            .Setup(x => x.UpdateMemberImage(It.IsAny<MemberAvatar>(), It.IsAny<byte[]>()))
            .Returns(ServiceResult.Successful());

        var unitOfWork = MockUnitOfWork.Create(context);
        var resolvedAuthorizationService = authorizationService ?? Mock.Of<IAuthorizationService>();
        var notificationService = Mock.Of<INotificationService>();
        var subscriptionWriter = new MemberChapterSubscriptionWriter(unitOfWork);

        var workflow = CreateAccountWorkflow(
            unitOfWork,
            resolvedAuthorizationService,
            memberEmailService,
            notificationService,
            subscriptionWriter);

        return new MemberService(
            unitOfWork,
            resolvedAuthorizationService,
            memberImageService.Object,
            memberEmailService,
            notificationService,
            Mock.Of<IOAuthProviderFactory>(),
            Mock.Of<ITopicService>(),
            Mock.Of<IPaymentProviderFactory>(),
            Mock.Of<IGeolocationService>(),
            Mock.Of<ILoggingService>(),
            new DistanceUnitFactory(),
            subscriptionWriter,
            new MemberSiteSubscriptionWriter(unitOfWork),
            CreateMockRecaptchaService(),
            new EmailValidationService(emailVerifier ?? new InconclusiveEmailVerifier()),
            workflow.GetRequiredService<IAccountContextFactory>(),
            workflow.GetRequiredService<StateMachineRunner<AccountState, AccountTrigger, AccountContext>>());
    }

    /// <summary>
    /// The account machine wired the way the app wires it, over the same mocks the service under test uses, so
    /// a step resolves here exactly as it does in production. The steps come from the definition, so one added
    /// to a transition needs no change in this helper.
    /// </summary>
    private static IServiceProvider CreateAccountWorkflow(
        IUnitOfWork unitOfWork,
        IAuthorizationService authorizationService,
        IMemberEmailService memberEmailService,
        INotificationService notificationService,
        IMemberChapterSubscriptionWriter subscriptionWriter)
    {
        var definition = AccountStateMachine.Create();

        var services = new ServiceCollection()
            .AddSingleton(unitOfWork)
            .AddSingleton(authorizationService)
            .AddSingleton(memberEmailService)
            .AddSingleton(notificationService)
            .AddSingleton(subscriptionWriter)
            .AddSingleton(definition)
            .AddScoped<IAccountContextFactory, AccountContextFactory>()
            .AddScoped<IStateResolver<AccountState, AccountContext>, AccountStateResolver>()
            .AddScoped<IStepFactory<AccountContext>, ServiceProviderStepFactory<AccountContext>>()
            .AddScoped<StateMachineRunner<AccountState, AccountTrigger, AccountContext>>();

        foreach (var stepType in definition.StepTypes)
        {
            services.AddScoped(stepType);
        }

        return services.BuildServiceProvider();
    }

    private static IMemberChapterServiceRequest CreateMemberChapterRequest(Chapter chapter, Member member) =>
        Mock.Of<IMemberChapterServiceRequest>(x =>
            x.Platform == PlatformType.DrunkenKnitwits &&
            x.Chapter == chapter &&
            x.CurrentMember == member &&
            x.HttpRequestContext == Mock.Of<IHttpRequestContext>());

    private static IAuthorizationService CreateMockAuthorizationService(bool chapterHasAccess)
    {
        var authorizationService = new Mock<IAuthorizationService>();
        authorizationService
            .Setup(x => x.ChapterHasAccess(
                It.IsAny<IEnumerable<SiteSubscriptionFeature>>(),
                It.IsAny<SiteFeatureType>()))
            .Returns(chapterHasAccess);
        return authorizationService.Object;
    }

    private static IRecaptchaService CreateMockRecaptchaService()
    {
        var recaptchaService = new Mock<IRecaptchaService>();
        recaptchaService
            .Setup(x => x.Verify(It.IsAny<string>()))
            .ReturnsAsync(new RecaptchaResult { Score = 1, Success = true });
        return recaptchaService.Object;
    }

    private static MockOdkContext CreateMockOdkContext() => new MockOdkContext();

    private static void SeedDefaultSiteSubscription(MockOdkContext context, PlatformType platform = PlatformType.Default)
    {
        context.Create(new SiteSubscription
        {
            Id = Guid.NewGuid(),
            Name = "Default",
            Description = "",
            GroupLimit = 10,
            Enabled = true,
            Default = true,
            Platform = platform,
            SitePaymentSettingId = Guid.NewGuid()
        });
    }
}
