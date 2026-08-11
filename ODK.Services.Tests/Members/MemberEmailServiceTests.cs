using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using ODK.Core.Chapters;
using ODK.Core.Emails;
using ODK.Core.Members;
using ODK.Core.Platforms;
using ODK.Core.Web;
using ODK.Data.Core;
using ODK.Services.Emails;
using ODK.Services.Members;
using ODK.Services.Web;

namespace ODK.Services.Tests.Members;

[Parallelizable]
public static class MemberEmailServiceTests
{
    [Test]
    public static async Task SendMemberImportActivationEmail_SendsActivationTemplateWithTheActivationUrl()
    {
        // Arrange
        var chapter = CreateChapter();
        var member = CreateMember();

        var emailService = CreateEmailService();
        var urlProvider = new Mock<IUrlProvider>();
        urlProvider
            .Setup(x => x.ActivateAccountUrl(chapter, "token-123"))
            .Returns("https://test.local/activate/token-123");

        var service = CreateService(emailService, urlProvider);

        var request = new MemberChapterServiceRequest
        {
            Chapter = chapter,
            CurrentMember = member,
            CurrentMemberOrDefault = member,
            HttpRequestContext = CreateHttpRequestContext(),
            Platform = PlatformType.Default
        };

        // Act
        await service.SendMemberImportActivationEmail(request, "token-123");

        // Assert
        emailService.Verify(
            x => x.SendEmail(
                request,
                chapter,
                It.Is<EmailAddressee>(x => x.Address == member.EmailAddress),
                EmailType.MemberImportActivation,
                It.Is<IEmailParameters>(x => x.ToDictionary()["account.urls.activate"] == "https://test.local/activate/token-123")),
            Times.Once);
    }

    [Test]
    public static async Task SendMemberImportInviteEmail_SendsInviteTemplateWithTheSubscriptionUrl()
    {
        // Arrange
        var chapter = CreateChapter();
        var member = CreateMember();

        var emailService = CreateEmailService();
        var urlProvider = new Mock<IUrlProvider>();
        urlProvider
            .Setup(x => x.ChapterSubscription(chapter))
            .Returns("https://test.local/group/subscription");

        var service = CreateService(emailService, urlProvider);

        var request = new ChapterServiceRequest
        {
            Chapter = chapter,
            CurrentMemberOrDefault = null,
            HttpRequestContext = CreateHttpRequestContext(),
            Platform = PlatformType.Default
        };

        // Act
        await service.SendMemberImportInviteEmail(request, member);

        // Assert
        emailService.Verify(
            x => x.SendEmail(
                request,
                chapter,
                It.Is<EmailAddressee>(x => x.Address == member.EmailAddress),
                EmailType.MemberImportInvite,
                It.Is<IEmailParameters>(x => x.ToDictionary()["group.urls.join"] == "https://test.local/group/subscription")),
            Times.Once);
    }

    private static Chapter CreateChapter() => new()
    {
        Id = Guid.NewGuid(),
        Name = "Test group",
        Slug = "test-group"
    };

    private static Mock<IEmailService> CreateEmailService()
    {
        var mock = new Mock<IEmailService>();

        mock
            .Setup(x => x.SendEmail(
                It.IsAny<IServiceRequest>(),
                It.IsAny<Chapter?>(),
                It.IsAny<EmailAddressee>(),
                It.IsAny<EmailType>(),
                It.IsAny<IEmailParameters>()))
            .ReturnsAsync(ServiceResult.Successful());

        return mock;
    }

    private static IHttpRequestContext CreateHttpRequestContext()
    {
        var mock = new Mock<IHttpRequestContext>();
        mock.Setup(x => x.BaseUrl).Returns("https://test.local");
        return mock.Object;
    }

    private static Member CreateMember() => new()
    {
        EmailAddress = "member@example.com",
        FirstName = "Test",
        Id = Guid.NewGuid(),
        LastName = "Member"
    };

    private static MemberEmailService CreateService(
        Mock<IEmailService> emailService,
        Mock<IUrlProvider> urlProvider)
    {
        var urlProviderFactory = new Mock<IUrlProviderFactory>();
        urlProviderFactory
            .Setup(x => x.Create(It.IsAny<IServiceRequest>()))
            .ReturnsAsync(urlProvider.Object);

        return new MemberEmailService(
            emailService.Object,
            urlProviderFactory.Object,
            Mock.Of<IUnitOfWork>(),
            Mock.Of<IMemberLocaleService>());
    }
}
