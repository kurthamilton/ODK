using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using ODK.Core.Chapters;
using ODK.Core.Countries;
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

    private static IChapterServiceRequest CreateChapterRequest(Chapter chapter) =>
        Mock.Of<IChapterServiceRequest>(x =>
            x.Platform == PlatformType.DrunkenKnitwits &&
            x.Chapter == chapter &&
            x.HttpRequestContext == Mock.Of<IHttpRequestContext>());

    private static MemberCreateProfile CreateChapterProfile(string emailAddress, string firstName) => new MemberCreateProfile
    {
        EmailAddress = emailAddress,
        FirstName = firstName,
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
        IEmailVerifier? emailVerifier = null)
    {
        var memberImageService = new Mock<IMemberImageService>();
        memberImageService
            .Setup(x => x.UpdateMemberImage(It.IsAny<MemberAvatar>(), It.IsAny<byte[]>()))
            .Returns(ServiceResult.Successful());

        var unitOfWork = MockUnitOfWork.Create(context);
        return new MemberService(
            unitOfWork,
            Mock.Of<IAuthorizationService>(),
            memberImageService.Object,
            memberEmailService,
            Mock.Of<INotificationService>(),
            Mock.Of<IOAuthProviderFactory>(),
            Mock.Of<ITopicService>(),
            Mock.Of<IPaymentProviderFactory>(),
            Mock.Of<IGeolocationService>(),
            Mock.Of<ILoggingService>(),
            new DistanceUnitFactory(),
            new MemberChapterSubscriptionWriter(unitOfWork),
            new MemberSiteSubscriptionWriter(unitOfWork),
            CreateMockRecaptchaService(),
            new EmailValidationService(emailVerifier ?? new InconclusiveEmailVerifier()));
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
