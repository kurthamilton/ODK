using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using ODK.Core.Chapters;
using ODK.Core.Platforms;
using ODK.Services;
using ODK.Services.Chapters;
using ODK.Web.Common.Account;
using ODK.Web.Common.Routes;
using ODK.Web.Common.Services;
using ODK.Web.Razor.Pages.Account;

namespace ODK.Web.Razor.Tests.Pages.Account;

[Parallelizable]
public static class LogoutModelTests
{
    [Test]
    public static async Task OnGet_DrunkenKnitwitsWithSoleChapter_RedirectsToThatGroup()
    {
        // Arrange - Drunken Knitwits is a group-level platform, so a member of a single group belongs
        // on that group's page once signed out.
        var chapter = CreateChapter(PlatformType.DrunkenKnitwits, "Bristol Drunken Knitwits");
        var model = CreateLogoutModel(PlatformType.DrunkenKnitwits, chapter);

        // Act
        var result = await model.OnGet();

        // Assert
        result.Should().BeOfType<RedirectResult>()
            .Which.Url.Should().Be("/bristol");
    }

    [Test]
    public static async Task OnGet_DrunkenKnitwitsWithoutSoleChapter_RedirectsToTheSiteHome()
    {
        // Arrange - no sole chapter means the member belongs to none, or to several
        var model = CreateLogoutModel(PlatformType.DrunkenKnitwits, soleChapter: null);

        // Act
        var result = await model.OnGet();

        // Assert
        result.Should().BeOfType<RedirectResult>()
            .Which.Url.Should().Be("/");
    }

    [Test]
    public static async Task OnGet_DefaultPlatform_RedirectsToTheSiteHome()
    {
        // Arrange - Group Squirrel is a site-level platform, so its members always land on the site
        // home and the chapter lookup is never made
        var chapterService = new Mock<IChapterService>();
        var model = CreateLogoutModel(PlatformType.Default, chapterService: chapterService);

        // Act
        var result = await model.OnGet();

        // Assert
        result.Should().BeOfType<RedirectResult>()
            .Which.Url.Should().Be("/");
        chapterService.Verify(x => x.GetSoleChapter(It.IsAny<IMemberServiceRequest>()), Times.Never);
    }

    [Test]
    public static async Task OnGet_SignsTheMemberOut()
    {
        // Arrange
        var loginHandler = new Mock<ILoginHandler>();
        var model = CreateLogoutModel(
            PlatformType.DrunkenKnitwits,
            CreateChapter(PlatformType.DrunkenKnitwits, "Bristol Drunken Knitwits"),
            loginHandler: loginHandler);

        // Act
        await model.OnGet();

        // Assert
        loginHandler.Verify(x => x.Logout(), Times.Once);
    }

    [Test]
    public static async Task OnGet_ResolvesTheLandingPageBeforeSigningOut()
    {
        // Arrange - the landing page depends on the current member, so it cannot be resolved once the
        // member has been signed out
        var signedOut = false;
        var resolvedWhileSignedIn = false;

        var loginHandler = new Mock<ILoginHandler>();
        loginHandler
            .Setup(x => x.Logout())
            .Callback(() => signedOut = true)
            .Returns(Task.CompletedTask);

        var chapterService = new Mock<IChapterService>();
        chapterService
            .Setup(x => x.GetSoleChapter(It.IsAny<IMemberServiceRequest>()))
            .Callback(() => resolvedWhileSignedIn = !signedOut)
            .ReturnsAsync(CreateChapter(PlatformType.DrunkenKnitwits, "Bristol Drunken Knitwits"));

        var model = CreateLogoutModel(
            PlatformType.DrunkenKnitwits,
            chapterService: chapterService,
            loginHandler: loginHandler);

        // Act
        await model.OnGet();

        // Assert
        resolvedWhileSignedIn.Should().BeTrue();
    }

    private static Chapter CreateChapter(PlatformType platform, string name) => new()
    {
        Id = Guid.NewGuid(),
        Name = name,
        Platform = platform,
        Slug = "bristol"
    };

    private static LogoutModel CreateLogoutModel(
        PlatformType platform,
        Chapter? soleChapter = null,
        Mock<IChapterService>? chapterService = null,
        Mock<ILoginHandler>? loginHandler = null)
    {
        // A supplied mock keeps its own setups: setting GetSoleChapter up again here would replace them.
        if (chapterService == null)
        {
            chapterService = new Mock<IChapterService>();
            chapterService
                .Setup(x => x.GetSoleChapter(It.IsAny<IMemberServiceRequest>()))
                .ReturnsAsync(soleChapter);
        }

        loginHandler ??= new Mock<ILoginHandler>();
        loginHandler
            .Setup(x => x.Logout())
            .Returns(Task.CompletedTask);

        var requestStore = new Mock<IRequestStore>();
        requestStore
            .Setup(x => x.Platform)
            .Returns(platform);
        requestStore
            .Setup(x => x.MemberServiceRequest)
            .Returns(Mock.Of<IMemberServiceRequest>());

        return new LogoutModel(chapterService.Object, loginHandler.Object)
        {
            OdkRoutes = new OdkRoutes(requestStore.Object),
            RequestStore = requestStore.Object
        };
    }
}
