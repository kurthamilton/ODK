using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Moq;
using NUnit.Framework;
using ODK.Web.Razor.Models.Feedback;
using ODK.Web.Razor.Mvc;

namespace ODK.Web.Razor.Tests.Mvc;

[Parallelizable]
public static class AntiforgeryValidationFailedResultFilterTests
{
    [Test]
    public static void OnResultExecuting_FormSubmission_RedirectsToTheReferrer()
    {
        // Arrange - a browser posting a form whose token no longer matches the current identity.
        var context = CreateContext(new AntiforgeryValidationFailedResult(), secFetchMode: "navigate");
        context.HttpContext.Request.Headers.Referer = "https://example.com/manchester/account/login";

        // Act
        CreateFilter(out _).OnResultExecuting(context);

        // Assert
        context.Result.Should().BeOfType<RedirectResult>()
            .Which.Url.Should().Be("https://example.com/manchester/account/login");
    }

    [Test]
    public static void OnResultExecuting_FormSubmission_AddsFeedbackSoTheRedirectExplainsItself()
    {
        // Arrange
        var context = CreateContext(new AntiforgeryValidationFailedResult(), secFetchMode: "navigate");
        var filter = CreateFilter(out var tempData);

        // Act
        filter.OnResultExecuting(context);

        // Assert
        tempData["Feedback[0].Message"].Should().Be(
            "That page had expired, so the form was not submitted. Please try again.");
        tempData["Feedback[0].Type"].Should().Be(FeedbackType.Warning);
    }

    [Test]
    public static void OnResultExecuting_FormSubmission_SavesTheFeedbackItself()
    {
        // Arrange - SaveTempDataFilter is a resource and result filter, and neither stage runs when an
        // authorization filter short-circuits, so unsaved feedback never reaches the redirected page.
        var context = CreateContext(new AntiforgeryValidationFailedResult(), secFetchMode: "navigate");
        var filter = CreateFilter(out _, out var tempDataProvider);

        // Act
        filter.OnResultExecuting(context);

        // Assert
        tempDataProvider.Verify(
            x => x.SaveTempData(It.IsAny<HttpContext>(), It.IsAny<IDictionary<string, object?>>()),
            Times.Once());
    }

    [Test]
    public static void OnResultExecuting_FormSubmissionWithOffSiteReferer_RedirectsHome()
    {
        // Arrange - an off-site Referer is an open-redirect vector, so it is never followed.
        var context = CreateContext(new AntiforgeryValidationFailedResult(), secFetchMode: "navigate");
        context.HttpContext.Request.Headers.Referer = "https://evil.example/landing";

        // Act
        CreateFilter(out _).OnResultExecuting(context);

        // Assert
        context.Result.Should().BeOfType<RedirectResult>().Which.Url.Should().Be("/");
    }

    [Test]
    public static void OnResultExecuting_AjaxPost_LeavesTheBadRequest()
    {
        // Arrange - a fetch would follow the redirect and be handed a page where it expected a response.
        var result = new AntiforgeryValidationFailedResult();
        var context = CreateContext(result, secFetchMode: "same-origin");

        // Act
        CreateFilter(out _).OnResultExecuting(context);

        // Assert
        context.Result.Should().BeSameAs(result);
    }

    [Test]
    public static void OnResultExecuting_NoSecFetchMode_LeavesTheBadRequest()
    {
        // Arrange - a client that sends no Sec-Fetch-Mode is not a browser submitting a form.
        var result = new AntiforgeryValidationFailedResult();
        var context = CreateContext(result, secFetchMode: null);

        // Act
        CreateFilter(out _).OnResultExecuting(context);

        // Assert
        context.Result.Should().BeSameAs(result);
    }

    [Test]
    public static void OnResultExecuting_ResultIsNotAnAntiforgeryFailure_LeavesItAlone()
    {
        // Arrange
        var result = new EmptyResult();
        var context = CreateContext(result, secFetchMode: "navigate");

        // Act
        CreateFilter(out var tempData).OnResultExecuting(context);

        // Assert
        context.Result.Should().BeSameAs(result);
        tempData.ContainsKey("FeedbackCount").Should().BeFalse();
    }

    private static ResultExecutingContext CreateContext(IActionResult result, string? secFetchMode)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Method = HttpMethods.Post;
        httpContext.Request.Host = new HostString("example.com");
        httpContext.Request.Path = "/manchester/account/login";

        if (secFetchMode != null)
        {
            httpContext.Request.Headers["Sec-Fetch-Mode"] = secFetchMode;
        }

        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        return new ResultExecutingContext(actionContext, [], result, controller: new object());
    }

    private static AntiforgeryValidationFailedResultFilter CreateFilter(out ITempDataDictionary tempData)
        => CreateFilter(out tempData, out _);

    private static AntiforgeryValidationFailedResultFilter CreateFilter(
        out ITempDataDictionary tempData, out Mock<ITempDataProvider> tempDataProvider)
    {
        tempDataProvider = new Mock<ITempDataProvider>();
        tempData = new TempDataDictionary(new DefaultHttpContext(), tempDataProvider.Object);

        var factory = new Mock<ITempDataDictionaryFactory>();
        factory.Setup(x => x.GetTempData(It.IsAny<HttpContext>())).Returns(tempData);

        return new AntiforgeryValidationFailedResultFilter(factory.Object);
    }
}
