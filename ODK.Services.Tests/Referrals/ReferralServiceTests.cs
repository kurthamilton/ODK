using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using ODK.Core.Chapters;
using ODK.Core.Emails;
using ODK.Core.Members;
using ODK.Core.Platforms;
using ODK.Core.Referrals;
using ODK.Services.Emails;
using ODK.Services.Referrals;
using ODK.Services.Tests.Helpers;
using ODK.Services.Web;

namespace ODK.Services.Tests.Referrals;

[Parallelizable]
public static class ReferralServiceTests
{
    private const string JoinUrl = "https://example.com/account/create";

    [Test]
    public static async Task CreateReferral_AlreadyAMember_RecordsTheReferralButSendsNoEmail()
    {
        // Arrange - the address belongs to an existing member. The referral is still recorded, but there is
        // nothing to invite them to.
        var (context, member) = CreateContext();
        CreateCampaign(context);
        context.CreateMember(afterCreate: x => x.EmailAddress = "friend@example.com");
        var (service, request, emailService) = CreateService(context, member);

        // Act
        var result = await service.CreateReferral(request, "friend@example.com");

        // Assert
        result.Success.Should().BeTrue();
        context.Set<Referral>().Should().ContainSingle(x => x.EmailAddress == "friend@example.com");
        VerifyNoEmailSent(emailService);
    }

    [Test]
    public static async Task CreateReferral_AlreadyAMember_ReportsTheSameResultAsARealReferral()
    {
        // Arrange - the two outcomes must be indistinguishable, or the response reveals whether an address
        // holds an account to anyone who cares to ask.
        var (memberContext, memberMember) = CreateContext();
        CreateCampaign(memberContext);
        memberContext.CreateMember(afterCreate: x => x.EmailAddress = "friend@example.com");
        var (memberService, memberRequest, _) = CreateService(memberContext, memberMember);

        var (freshContext, freshMember) = CreateContext();
        CreateCampaign(freshContext);
        var (freshService, freshRequest, _) = CreateService(freshContext, freshMember);

        // Act
        var existing = await memberService.CreateReferral(memberRequest, "friend@example.com");
        var fresh = await freshService.CreateReferral(freshRequest, "friend@example.com");

        // Assert
        existing.Success.Should().Be(fresh.Success);
        existing.Message.Should().Be(fresh.Message);
    }

    [Test]
    public static async Task CreateReferral_InvalidEmailAddress_Fails()
    {
        // Arrange
        var (context, member) = CreateContext();
        CreateCampaign(context);
        var (service, request, emailService) = CreateService(context, member);

        // Act
        var result = await service.CreateReferral(request, "not-an-email");

        // Assert
        result.Success.Should().BeFalse();
        context.Set<Referral>().Should().BeEmpty();
        VerifyNoEmailSent(emailService);
    }

    [Test]
    public static async Task CreateReferral_NoActiveCampaign_Fails()
    {
        // Arrange - the only campaign expired yesterday.
        var (context, member) = CreateContext();
        CreateCampaign(context, expiresUtc: DateTime.UtcNow.AddDays(-1));
        var (service, request, emailService) = CreateService(context, member);

        // Act
        var result = await service.CreateReferral(request, "friend@example.com");

        // Assert
        result.Success.Should().BeFalse();
        context.Set<Referral>().Should().BeEmpty();
        VerifyNoEmailSent(emailService);
    }

    [Test]
    public static async Task CreateReferral_OwnEmailAddress_Fails()
    {
        // Arrange
        var (context, member) = CreateContext(emailAddress: "me@example.com");
        CreateCampaign(context);
        var (service, request, emailService) = CreateService(context, member);

        // Act - cased differently, since an address is not case sensitive for this purpose.
        var result = await service.CreateReferral(request, "ME@example.com");

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("You cannot refer yourself");
        context.Set<Referral>().Should().BeEmpty();
        VerifyNoEmailSent(emailService);
    }

    [Test]
    public static async Task CreateReferral_UsesTheMostRecentActiveCampaign()
    {
        // Arrange - an expired campaign created most recently must not win over the running one.
        var (context, member) = CreateContext();
        var active = CreateCampaign(context, name: "Active", createdUtc: DateTime.UtcNow.AddDays(-2));
        CreateCampaign(context, name: "Older", createdUtc: DateTime.UtcNow.AddDays(-5));
        CreateCampaign(context, name: "Expired", createdUtc: DateTime.UtcNow.AddDays(-1),
            expiresUtc: DateTime.UtcNow.AddDays(-1));
        var (service, request, _) = CreateService(context, member);

        // Act
        await service.CreateReferral(request, "friend@example.com");

        // Assert
        context.Set<Referral>().Single().ReferralCampaignId.Should().Be(active.Id);
    }

    [Test]
    public static async Task CreateReferral_ValidAddress_SendsTheCampaignEmailWithItsTokens()
    {
        // Arrange
        var (context, member) = CreateContext(firstName: "Ada", lastName: "Lovelace");
        var campaign = CreateCampaign(context);
        var (service, request, emailService) = CreateService(context, member);

        // Act
        var result = await service.CreateReferral(request, "friend@example.com");

        // Assert - the campaign's own subject and body are sent, with the three tokens the campaign text
        // is written against.
        result.Success.Should().BeTrue();
        var referral = context.Set<Referral>().Single();
        referral.MemberId.Should().Be(member.Id);
        referral.CompletedUtc.Should().BeNull();

        emailService.Verify(
            x => x.SendEmail(
                request,
                null,
                It.Is<IEnumerable<EmailAddressee>>(to => to.Single().Address == "friend@example.com"),
                campaign.EmailSubject,
                campaign.EmailTextHtml,
                EmailRecipientType.Members,
                It.Is<IEmailParameters>(p =>
                    p.ToDictionary()["member.fullName"] == "Ada Lovelace" &&
                    p.ToDictionary()["referral.id"] == referral.Id.ToString() &&
                    p.ToDictionary()["group.urls.join"] == JoinUrl)),
            Times.Once);
    }

    private static ReferralCampaign CreateCampaign(
        MockOdkContext context,
        string name = "Spring drive",
        DateTime? createdUtc = null,
        DateTime? expiresUtc = null)
        => context.Create(new ReferralCampaign
        {
            CreatedUtc = createdUtc ?? DateTime.UtcNow.AddDays(-1),
            DescriptionHtml = "<p>Refer a friend</p>",
            EmailSubject = $"{name} subject",
            EmailTextHtml = "<p>Hello {fullName}</p>",
            ExpiresUtc = expiresUtc,
            Id = Guid.NewGuid(),
            Name = name
        });

    // Split from CreateService so rows can be arranged first: MockUnitOfWork.Create saves the context, so
    // anything added afterwards never reaches the in-memory database.
    private static (MockOdkContext Context, Member CurrentMember) CreateContext(
        string emailAddress = "me@example.com", string firstName = "Ada", string lastName = "Lovelace")
    {
        var context = new MockOdkContext();
        var member = context.CreateMember(afterCreate: x =>
        {
            x.EmailAddress = emailAddress;
            x.FirstName = firstName;
            x.LastName = lastName;
        });
        return (context, member);
    }

    private static (IReferralService Service, IMemberServiceRequest Request, Mock<IEmailService> EmailService)
        CreateService(MockOdkContext context, Member currentMember)
    {
        var request = new Mock<IMemberServiceRequest>();
        request.Setup(x => x.CurrentMember).Returns(currentMember);
        request.Setup(x => x.CurrentMemberOrDefault).Returns(currentMember);
        request.Setup(x => x.Platform).Returns(PlatformType.Default);

        var emailService = new Mock<IEmailService>();

        var urlProvider = new Mock<IUrlProvider>();
        urlProvider.Setup(x => x.JoinUrl()).Returns(JoinUrl);

        var urlProviderFactory = new Mock<IUrlProviderFactory>();
        urlProviderFactory.Setup(x => x.Create(It.IsAny<IServiceRequest>())).ReturnsAsync(urlProvider.Object);

        var service = new ReferralService(
            MockUnitOfWorkFactory.Create(context),
            emailService.Object,
            new EmailValidationService(new InconclusiveEmailVerifier()),
            urlProviderFactory.Object);

        return (service, request.Object, emailService);
    }

    private static void VerifyNoEmailSent(Mock<IEmailService> emailService)
        => emailService.Verify(
            x => x.SendEmail(
                It.IsAny<IServiceRequest>(),
                It.IsAny<Chapter?>(),
                It.IsAny<IEnumerable<EmailAddressee>>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<EmailRecipientType>(),
                It.IsAny<IEmailParameters>()),
            Times.Never);
}
