using System;
using Microsoft.AspNetCore.Http;
using ODK.Core.Chapters;
using ODK.Core.Platforms;

namespace ODK.Web.Common.Extensions;

public static class HttpContextExtensions
{
    public static Guid? ChapterId(this HttpContext httpContext)
        => Guid.TryParse(httpContext.Request.RouteValues["chapterId"]?.ToString(), out Guid id)
            ? id
            : null;

    public static string? ChapterName(this HttpContext httpContext)
    {
        var chapterName = httpContext.Request.RouteValues["chapterName"] as string;
        return (!string.IsNullOrWhiteSpace(chapterName))
            ? Chapter.GetFullName(PlatformType.DrunkenKnitwits, chapterName)
            : null;
    }

    public static string? ChapterSlug(this HttpContext httpContext)
    {
        var slug = httpContext.Request.RouteValues["slug"] as string;
        return (!string.IsNullOrWhiteSpace(slug))
            ? slug
            : null;
    }
}