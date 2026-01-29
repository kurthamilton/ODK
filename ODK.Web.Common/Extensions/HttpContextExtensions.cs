using System;
using ODK.Core.Chapters;
using ODK.Core.Platforms;
using ODK.Core.Web;

namespace ODK.Web.Common.Extensions;

public static class HttpContextExtensions
{
    public static Guid? ChapterId(this IHttpRequestContext context)
        => context.RouteValues.TryGetValue("chapterId", out var chapterIdRouteValue) &&
           Guid.TryParse(chapterIdRouteValue, out Guid id)
            ? id
            : null;

    public static string? ChapterName(this IHttpRequestContext context)
        => context.RouteValues.TryGetValue("chapterName", out var chapterName) &&
           !string.IsNullOrWhiteSpace(chapterName)
            ? Chapter.GetFullName(PlatformType.DrunkenKnitwits, chapterName)
            : null;

    public static string? ChapterSlug(this IHttpRequestContext context)
        => context.RouteValues.TryGetValue("slug", out var slug) &&
           !string.IsNullOrWhiteSpace(slug)
            ? slug
            : null;
}