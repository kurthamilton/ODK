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
using ODK.Core.Subscriptions;
using ODK.Services;
using ODK.Services.Authentication.OAuth;
using ODK.Services.Authorization;
using ODK.Services.Geolocation;
using ODK.Services.Logging;
using ODK.Services.Members;
using ODK.Services.Members.Models;
using ODK.Services.Notifications;
using ODK.Services.Payments;
using ODK.Services.Topics;
using ODK.Services.Tests.Helpers;

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
        var request = Mock.Of<IServiceRequest>(x => x.Platform == PlatformType.Default);

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
        var request = Mock.Of<IServiceRequest>(x => x.Platform == PlatformType.Default);

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
        var request = Mock.Of<IServiceRequest>(x => x.Platform == PlatformType.Default);

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
            x.Chapter == chapter);

    private static MemberCreateProfile CreateChapterProfile(string emailAddress, string firstName) => new MemberCreateProfile
    {
        EmailAddress = emailAddress,
        FirstName = firstName,
        LastName = "Member",
        ImageData = [1, 2, 3]
    };

    private static AccountCreateModel CreateModel(string emailAddress, string firstName) => new AccountCreateModel
    {
        EmailAddress = emailAddress,
        FirstName = firstName,
        LastName = "Member",
        Location = null,
        LocationName = "",
        NewTopics = [],
        OAuthProviderType = null,
        OAuthToken = null,
        TopicIds = []
    };

    private static MemberService CreateMemberService(MockOdkContext context, IMemberEmailService memberEmailService)
    {
        var memberImageService = new Mock<IMemberImageService>();
        memberImageService
            .Setup(x => x.UpdateMemberImage(It.IsAny<MemberAvatar>(), It.IsAny<byte[]>()))
            .Returns(ServiceResult.Successful());

        return new MemberService(
            MockUnitOfWork.Create(context),
            Mock.Of<IAuthorizationService>(),
            memberImageService.Object,
            memberEmailService,
            Mock.Of<INotificationService>(),
            Mock.Of<IOAuthProviderFactory>(),
            Mock.Of<ITopicService>(),
            Mock.Of<IPaymentProviderFactory>(),
            Mock.Of<IGeolocationService>(),
            Mock.Of<ILoggingService>(),
            new DistanceUnitFactory());
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
