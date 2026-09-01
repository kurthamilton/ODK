using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using ODK.Core.Workflows;
using ODK.Services.Members.Workflows.Account;
using ODK.Services.Members.Workflows.ChapterMembership;
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
using ODK.Services.Authentication;
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

    [TestCase(PlatformType.Default)]
    [TestCase(PlatformType.DrunkenKnitwits)]
    public static async Task CreateAccount_NewMember_SavesTheRequestPlatform(PlatformType platform)
    {
        // Arrange - an account records the platform it was raised on, taken from the request that raised it.
        using var context = CreateMockOdkContext();
        SeedDefaultSiteSubscription(context, platform);

        var service = CreateMemberService(context, new Mock<IMemberEmailService>().Object);

        // Act
        var result = await service.CreateAccount(
            CreateSiteRequest(platform), CreateModel("new@example.com", firstName: "New"));

        // Assert
        result.Success.Should().BeTrue();
        context.Set<Member>().Single(x => x.EmailAddress == "new@example.com")
            .Platform.Should().Be(platform);
    }

    [Test]
    public static async Task CreateChapterAccount_ExistingActivatedMember_SendsDuplicateEmail()
    {
        // Arrange
        using var context = CreateMockOdkContext();
        SeedDefaultSiteSubscription(context, PlatformType.DrunkenKnitwits);
        var chapter = context.CreateChapter(siteSubscription: context.CreateSiteSubscription());

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
        var chapter = context.CreateChapter(siteSubscription: context.CreateSiteSubscription());

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
        var chapter = context.CreateChapter(siteSubscription: context.CreateSiteSubscription());

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
        var chapter = context.CreateChapter(siteSubscription: context.CreateSiteSubscription());

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
        var chapter = context.CreateChapter(siteSubscription: context.CreateSiteSubscription());
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
    public static async Task CreateChapterAccount_ExistingActivatedMember_DoesNotScoreTheSignUp()
    {
        /* Arrange - nothing is created for an address that already has an account, so the sign-up must not pay
           for an outbound reCAPTCHA call to decide that. Scoring belongs to the step that creates the account. */
        using var context = CreateMockOdkContext();
        SeedDefaultSiteSubscription(context, PlatformType.DrunkenKnitwits);
        var chapter = context.CreateChapter(siteSubscription: context.CreateSiteSubscription());

        context.CreateMember(activated: true, afterCreate: x => x.EmailAddress = "existing@example.com");

        var recaptchaService = CreateMockRecaptchaService();
        var service = CreateMemberService(
            context,
            new Mock<IMemberEmailService>().Object,
            recaptchaService: recaptchaService);

        // Act
        var result = await service.CreateChapterAccount(
            CreateChapterRequest(chapter),
            CreateChapterProfile("existing@example.com", firstName: "New"));

        // Assert
        result.Success.Should().BeTrue();
        recaptchaService.Verify(x => x.Verify(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public static async Task CreateChapterAccount_GroupAtItsMemberLimit_FailsWithoutCreatingAnAccount()
    {
        /* Arrange - signing up to a group on Drunken Knitwits is joining it, so the owner's subscription caps it
           the same way it caps a member who already has an account and joins. */
        using var context = CreateMockOdkContext();
        SeedDefaultSiteSubscription(context, PlatformType.DrunkenKnitwits);

        var chapter = context.CreateChapter(
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

        var service = CreateMemberService(context, new Mock<IMemberEmailService>().Object);

        // Act
        var result = await service.CreateChapterAccount(
            CreateChapterRequest(chapter),
            CreateChapterProfile("new@example.com", firstName: "New"));

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("This group is not able to welcome any new members");
        context.Set<Member>().Any(x => x.EmailAddress == "new@example.com").Should().BeFalse();
    }

    [Test]
    public static async Task CreateChapterAccount_RequiredGroupQuestionNotAnswered_Fails()
    {
        /* Arrange - signing up to a group is the same act as joining one, so it enforces the group's required
           questions the way JoinChapter does. */
        using var context = CreateMockOdkContext();
        SeedDefaultSiteSubscription(context, PlatformType.DrunkenKnitwits);
        var chapter = context.CreateChapter(siteSubscription: context.CreateSiteSubscription());
        context.Create(new ChapterProperty
        {
            ChapterId = chapter.Id,
            DataType = DataType.Text,
            DisplayName = "Favourite yarn",
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

    [TestCase(PlatformType.Default)]
    [TestCase(PlatformType.DrunkenKnitwits)]
    public static async Task CreateChapterAccount_NewMember_SavesTheRequestPlatform(PlatformType platform)
    {
        // Arrange
        using var context = CreateMockOdkContext();
        SeedDefaultSiteSubscription(context, platform);
        var chapter = context.CreateChapter(
            siteSubscription: context.CreateSiteSubscription(),
            platform: platform);

        var service = CreateMemberService(context, new Mock<IMemberEmailService>().Object);

        // Act
        var result = await service.CreateChapterAccount(
            CreateChapterRequest(chapter, platform),
            CreateChapterProfile("new@example.com", firstName: "New"));

        // Assert
        result.Success.Should().BeTrue();
        context.Set<Member>().Single(x => x.EmailAddress == "new@example.com")
            .Platform.Should().Be(platform);
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

    [Test]
    public static async Task AcceptInvitation_InvitedMember_ActivatesTheAccountAndJoinsTheGroup()
    {
        /* Arrange - an imported member following their invitation link on the platform whose join page needs an
           account that can sign in. Holding the token proves they read mail at the invited address, which is
           the only thing an activation email establishes, so the one submit does both. */
        using var context = CreateMockOdkContext();
        SeedDefaultSiteSubscription(context);

        var owner = context.CreateMember();
        var chapter = context.CreateChapter(owner: owner, siteSubscription: context.CreateSiteSubscription());

        var invited = context.CreateMember(activated: false, afterCreate: x =>
        {
            x.EmailAddress = "invited@example.com";
            x.FirstName = "Imported";
            x.LastName = "Name";
        });
        context.Create(new MemberActivationToken
        {
            ActivationToken = "activation-token",
            MemberId = invited.Id
        });
        CreateInvite(context, chapter, invited, "invite-token");

        var emailService = new Mock<IMemberEmailService>();
        var service = CreateMemberService(context, emailService.Object);

        // Act
        var result = await service.AcceptInvitation(
            CreateChapterRequest(chapter, PlatformType.Default),
            CreateInvitationAcceptModel("invite-token", firstName: "Confirmed"));

        // Assert
        result.Success.Should().BeTrue();

        var member = context.Set<Member>().Single(x => x.Id == invited.Id);
        member.Activated.Should().BeTrue();
        member.FirstName.Should().Be("Confirmed");

        context.Set<MemberPassword>().Should().ContainSingle(x => x.MemberId == invited.Id);

        // An invitation is approval, so the group that asked them in is not asked to approve them.
        context.Set<MemberChapter>()
            .Should()
            .ContainSingle(x => x.MemberId == invited.Id && x.ChapterId == chapter.Id)
            .Which
            .Approved
            .Should()
            .BeTrue();

        // Both single-use records are spent: the invitation, and the activation it stood in for.
        context.Set<MemberChapterInvite>().Any(x => x.MemberId == invited.Id).Should().BeFalse();
        context.Set<MemberActivationToken>().Any(x => x.MemberId == invited.Id).Should().BeFalse();
    }

    [Test]
    public static async Task AcceptInvitation_GroupApprovesNewMembers_StillSkipsApproval()
    {
        // Arrange - the group asked them in, so approving them would be asking it to confirm its own invitation.
        using var context = CreateMockOdkContext();
        SeedDefaultSiteSubscription(context);

        var owner = context.CreateMember();
        var chapter = context.CreateChapter(owner: owner, siteSubscription: context.CreateSiteSubscription());
        context.Create(new ChapterMembershipSettings
        {
            ApproveNewMembers = true,
            ChapterId = chapter.Id,
            Enabled = true
        });

        var invited = context.CreateMember(activated: false, afterCreate: x =>
            x.EmailAddress = "invited@example.com");
        context.Create(new MemberActivationToken
        {
            ActivationToken = "activation-token",
            MemberId = invited.Id
        });
        CreateInvite(context, chapter, invited, "invite-token");

        var service = CreateMemberService(
            context,
            new Mock<IMemberEmailService>().Object,
            authorizationService: CreateMockAuthorizationService(chapterHasAccess: true));

        // Act
        var result = await service.AcceptInvitation(
            CreateChapterRequest(chapter, PlatformType.Default),
            CreateInvitationAcceptModel("invite-token"));

        // Assert
        result.Success.Should().BeTrue();
        context.Set<MemberChapter>()
            .Single(x => x.MemberId == invited.Id)
            .Approved
            .Should()
            .BeTrue();
    }

    [Test]
    public static async Task AcceptInvitation_InvitationToAnotherGroup_FailsWithoutWritingAnything()
    {
        /* Arrange - the token names which invitation is being spent, and the page it was posted to names which
           group. A token for somewhere else is a link that is not for this page. */
        using var context = CreateMockOdkContext();
        SeedDefaultSiteSubscription(context);

        var owner = context.CreateMember();
        var chapter = context.CreateChapter(owner: owner, siteSubscription: context.CreateSiteSubscription());
        var other = context.CreateChapter(owner: owner, siteSubscription: context.CreateSiteSubscription());

        var invited = context.CreateMember(activated: false, afterCreate: x =>
            x.EmailAddress = "invited@example.com");
        CreateInvite(context, other, invited, "invite-token");

        var service = CreateMemberService(context, new Mock<IMemberEmailService>().Object);

        // Act
        var result = await service.AcceptInvitation(
            CreateChapterRequest(chapter, PlatformType.Default),
            CreateInvitationAcceptModel("invite-token"));

        // Assert
        result.Success.Should().BeFalse();
        context.Set<Member>().Single(x => x.Id == invited.Id).Activated.Should().BeFalse();
        context.Set<MemberChapter>().Any(x => x.MemberId == invited.Id).Should().BeFalse();
        context.Set<MemberChapterInvite>().Any(x => x.MemberId == invited.Id).Should().BeTrue();
    }

    [Test]
    public static async Task AcceptInvitation_MemberAlreadyActivated_FailsAndTellsThemToSignIn()
    {
        /* Arrange - the machine has no AcceptInvite edge out of Activated, and the page shows such a member a
           sign-in prompt rather than this form, so getting here means they activated between the two requests.
           What matters is that the wording sends them somewhere useful rather than reporting the trigger. */
        using var context = CreateMockOdkContext();
        SeedDefaultSiteSubscription(context);

        var owner = context.CreateMember();
        var chapter = context.CreateChapter(owner: owner, siteSubscription: context.CreateSiteSubscription());

        var invited = context.CreateMember(activated: true, afterCreate: x =>
            x.EmailAddress = "invited@example.com");
        CreateInvite(context, chapter, invited, "invite-token");

        var service = CreateMemberService(context, new Mock<IMemberEmailService>().Object);

        // Act
        var result = await service.AcceptInvitation(
            CreateChapterRequest(chapter, PlatformType.Default),
            CreateInvitationAcceptModel("invite-token"));

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Sign in");
        context.Set<MemberChapter>().Any(x => x.MemberId == invited.Id).Should().BeFalse();
    }

    [Test]
    public static async Task AcceptInvitation_RefusedPassword_LeavesTheInvitationOutstanding()
    {
        /* Arrange - the password check is the first step, so a refusal has to leave the account exactly as it
           was: still unactivated, and still holding the invitation to try again with. */
        using var context = CreateMockOdkContext();
        SeedDefaultSiteSubscription(context);

        var owner = context.CreateMember();
        var chapter = context.CreateChapter(owner: owner, siteSubscription: context.CreateSiteSubscription());

        var invited = context.CreateMember(activated: false, afterCreate: x =>
            x.EmailAddress = "invited@example.com");
        context.Create(new MemberActivationToken
        {
            ActivationToken = "activation-token",
            MemberId = invited.Id
        });
        CreateInvite(context, chapter, invited, "invite-token");

        var passwordService = CreateMockMemberPasswordService();
        passwordService
            .Setup(x => x.Validate(It.IsAny<string>()))
            .ReturnsAsync(ServiceResult.Failure("Password is too short"));

        var service = CreateMemberService(
            context,
            new Mock<IMemberEmailService>().Object,
            memberPasswordService: passwordService);

        // Act
        var result = await service.AcceptInvitation(
            CreateChapterRequest(chapter, PlatformType.Default),
            CreateInvitationAcceptModel("invite-token"));

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Password is too short");

        context.Set<Member>().Single(x => x.Id == invited.Id).Activated.Should().BeFalse();
        context.Set<MemberChapterInvite>().Any(x => x.MemberId == invited.Id).Should().BeTrue();
        context.Set<MemberActivationToken>().Any(x => x.MemberId == invited.Id).Should().BeTrue();
    }

    private static IChapterServiceRequest CreateChapterRequest(
        Chapter chapter, PlatformType platform = PlatformType.DrunkenKnitwits) =>
        Mock.Of<IChapterServiceRequest>(x =>
            x.Platform == platform &&
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

    private static InvitationAcceptModel CreateInvitationAcceptModel(
        string token, string firstName = "Invited") => new InvitationAcceptModel
    {
        FirstName = firstName,
        LastName = "Member",
        Password = "a-good-password",
        Properties = [],
        Token = token
    };

    private static MemberChapterInvite CreateInvite(
        MockOdkContext context, Chapter chapter, Member member, string token) => context.Create(
        new MemberChapterInvite
        {
            ChapterId = chapter.Id,
            CreatedUtc = DateTime.UtcNow,
            Id = Guid.NewGuid(),
            MemberId = member.Id,
            Token = token
        });

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

    private static IServiceRequest CreateSiteRequest(PlatformType platform = PlatformType.Default) =>
        Mock.Of<IServiceRequest>(x =>
            x.Platform == platform &&
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
        IAuthorizationService? authorizationService = null,
        Mock<IRecaptchaService>? recaptchaService = null,
        Mock<IMemberPasswordService>? memberPasswordService = null)
    {
        var memberImageService = new Mock<IMemberImageService>();
        memberImageService
            .Setup(x => x.UpdateMemberImage(It.IsAny<MemberAvatar>(), It.IsAny<byte[]>()))
            .Returns(ServiceResult.Successful());
        memberImageService
            .Setup(x => x.ValidateImage(It.IsAny<byte[]>()))
            .Returns(ServiceResult.Successful());

        var unitOfWork = MockUnitOfWorkFactory.Create(context);
        var resolvedAuthorizationService = authorizationService ?? Mock.Of<IAuthorizationService>();
        var notificationService = Mock.Of<INotificationService>();
        var subscriptionWriter = new MemberChapterSubscriptionWriter(unitOfWork);
        var siteSubscriptionWriter = new MemberSiteSubscriptionWriter(unitOfWork);
        var resolvedRecaptchaService = (recaptchaService ?? CreateMockRecaptchaService()).Object;
        var resolvedMemberPasswordService =
            (memberPasswordService ?? CreateMockMemberPasswordService()).Object;
        var emailValidationService = new EmailValidationService(emailVerifier ?? new InconclusiveEmailVerifier());
        var loggingService = Mock.Of<ILoggingService>();
        var geolocationService = Mock.Of<IGeolocationService>();
        var distanceUnitFactory = new DistanceUnitFactory();
        var topicService = Mock.Of<ITopicService>();
        var oauthProviderFactory = Mock.Of<IOAuthProviderFactory>();

        var workflow = CreateAccountWorkflow(
            unitOfWork,
            resolvedAuthorizationService,
            memberEmailService,
            notificationService,
            subscriptionWriter,
            siteSubscriptionWriter,
            memberImageService.Object,
            resolvedRecaptchaService,
            emailValidationService,
            loggingService,
            geolocationService,
            distanceUnitFactory,
            topicService,
            oauthProviderFactory,
            resolvedMemberPasswordService);

        return new MemberService(
            unitOfWork,
            memberImageService.Object,
            memberEmailService,
            topicService,
            Mock.Of<IPaymentProviderFactory>(),
            geolocationService,
            loggingService,
            distanceUnitFactory,
            emailValidationService,
            workflow.GetRequiredService<IChapterMembershipContextFactory>(),
            workflow.GetRequiredService<StateMachineRunner<
                ChapterMembershipState, ChapterMembershipTrigger, ChapterMembershipContext>>(),
            workflow.GetRequiredService<StateMachineRunner<AccountState, AccountTrigger, AccountContext>>(),
            workflow.GetRequiredService<IAccountContextFactory>(),
            Mock.Of<IPaymentService>());
    }

    /// <summary>
    /// The membership machine wired the way the app wires it, over the same mocks the service under test uses, so
    /// a step resolves here exactly as it does in production. The steps come from the definition, so one added
    /// to a transition needs no change in this helper.
    /// </summary>
    private static IServiceProvider CreateAccountWorkflow(
        IUnitOfWork unitOfWork,
        IAuthorizationService authorizationService,
        IMemberEmailService memberEmailService,
        INotificationService notificationService,
        IMemberChapterSubscriptionWriter subscriptionWriter,
        IMemberSiteSubscriptionWriter siteSubscriptionWriter,
        IMemberImageService memberImageService,
        IRecaptchaService recaptchaService,
        IEmailValidationService emailValidationService,
        ILoggingService loggingService,
        IGeolocationService geolocationService,
        IDistanceUnitFactory distanceUnitFactory,
        ITopicService topicService,
        IOAuthProviderFactory oauthProviderFactory,
        IMemberPasswordService memberPasswordService)
    {
        var membership = ChapterMembershipStateMachine.Create();
        var account = AccountStateMachine.Create();

        var services = new ServiceCollection()
            .AddSingleton(unitOfWork)
            .AddSingleton(authorizationService)
            .AddSingleton(memberEmailService)
            .AddSingleton(notificationService)
            .AddSingleton(subscriptionWriter)
            .AddSingleton(siteSubscriptionWriter)
            .AddSingleton(memberImageService)
            .AddSingleton(recaptchaService)
            .AddSingleton(emailValidationService)
            .AddSingleton(loggingService)
            .AddSingleton(geolocationService)
            .AddSingleton(distanceUnitFactory)
            .AddSingleton(topicService)
            .AddSingleton(oauthProviderFactory)
            .AddSingleton(memberPasswordService)
            .AddSingleton(new SiteSubscriptionCooldown(months: 0))
            .AddSingleton(membership)
            .AddSingleton(account)
            .AddScoped<IChapterMembershipContextFactory, ChapterMembershipContextFactory>()
            .AddScoped<
                IStateResolver<ChapterMembershipState, ChapterMembershipContext>,
                ChapterMembershipStateResolver>()
            .AddScoped<
                IStepFactory<ChapterMembershipContext>,
                ServiceProviderStepFactory<ChapterMembershipContext>>()
            .AddScoped<StateMachineRunner<
                ChapterMembershipState, ChapterMembershipTrigger, ChapterMembershipContext>>()
            .AddScoped<IAccountContextFactory, AccountContextFactory>()
            .AddScoped<IStateResolver<AccountState, AccountContext>, AccountStateResolver>()
            .AddScoped<IStepFactory<AccountContext>, ServiceProviderStepFactory<AccountContext>>()
            .AddScoped<StateMachineRunner<AccountState, AccountTrigger, AccountContext>>();

        foreach (var stepType in membership.StepTypes.Concat(account.StepTypes))
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

    private static Mock<IMemberPasswordService> CreateMockMemberPasswordService()
    {
        var memberPasswordService = new Mock<IMemberPasswordService>();
        memberPasswordService
            .Setup(x => x.Validate(It.IsAny<string>()))
            .ReturnsAsync(ServiceResult.Successful());
        memberPasswordService
            .Setup(x => x.Apply(It.IsAny<MemberPassword?>(), It.IsAny<string>()))
            .Returns((MemberPassword? existing, string password) => existing ?? new MemberPassword
            {
                Algorithm = "test",
                Hash = password,
                Iterations = 1,
                Salt = "salt"
            });
        return memberPasswordService;
    }

    private static Mock<IRecaptchaService> CreateMockRecaptchaService()
    {
        var recaptchaService = new Mock<IRecaptchaService>();
        recaptchaService
            .Setup(x => x.Verify(It.IsAny<string>()))
            .ReturnsAsync(new RecaptchaResult { Score = 1, Success = true });
        return recaptchaService;
    }

    private static MockOdkContext CreateMockOdkContext() => new MockOdkContext();

    private static void SeedDefaultSiteSubscription(MockOdkContext context, PlatformType platform = PlatformType.Default)
    {
        context.Create(new SiteSubscription
        {
            Id = Guid.NewGuid(),
            Name = "Default",
            DescriptionHtml = "",
            GroupLimit = 10,
            Enabled = true,
            Default = true,
            Platform = platform
        });
    }
}
