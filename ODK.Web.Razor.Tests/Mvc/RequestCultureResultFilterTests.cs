using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using NUnit.Framework;
using ODK.Web.Razor.Mvc;

namespace ODK.Web.Razor.Tests.Mvc;

[Parallelizable]
public static class RequestCultureResultFilterTests
{
    [Test]
    public static async Task OnResultExecutionAsync_AcceptLanguageSpecificCulture_AppliesItForRendering()
    {
        // Arrange - a request asking for en-US.
        var cultureDuringRender = await RunFilterWithAcceptLanguage("en-US");

        // Assert - the culture is applied while the result executes (the view renders).
        cultureDuringRender.Name.Should().Be("en-US");
    }

    [Test]
    public static async Task OnResultExecutionAsync_NoUsableAcceptLanguage_LeavesCurrentCultureUnchanged()
    {
        // Arrange - a neutral-only header yields no specific culture, so the ambient (default) culture stands.
        var ambient = CultureInfo.CurrentCulture;

        // Act
        var cultureDuringRender = await RunFilterWithAcceptLanguage("en");

        // Assert
        cultureDuringRender.Name.Should().Be(ambient.Name);
    }

    private static async Task<CultureInfo> RunFilterWithAcceptLanguage(string acceptLanguage)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers.AcceptLanguage = acceptLanguage;

        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        var resultContext = new ResultExecutingContext(
            actionContext, new List<IFilterMetadata>(), new EmptyResult(), controller: new object());

        var captured = CultureInfo.CurrentCulture;
        await new RequestCultureResultFilter().OnResultExecutionAsync(resultContext, () =>
        {
            captured = CultureInfo.CurrentCulture;
            return Task.FromResult<ResultExecutedContext>(null!);
        });

        return captured;
    }
}
