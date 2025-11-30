using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Core.Platforms;
using ODK.Services.Caching;
using ODK.Web.Common.Services;

namespace ODK.Web.Razor.Pages.Chapters;

public class ChapterPageContext
{
    private readonly HttpContext _httpContext;
    private readonly IRequestCache _requestCache;

    public ChapterPageContext(IRequestCache requestCache, HttpContext httpContext)
    {
        _httpContext = httpContext;
        _requestCache = requestCache;
    }

    public static Guid? GetChapterId(HttpContext httpContext)
    {
        return Guid.TryParse(httpContext.Request.RouteValues["id"]?.ToString(), out Guid id)
            ? id
            : null;
    }

    public static string? GetChapterName(HttpContext httpContext)
    {
        var chapterName = httpContext.Request.RouteValues["chapterName"] as string;
        return (!string.IsNullOrWhiteSpace(chapterName))
            ? chapterName
            : null;
    }

    public static string? GetChapterSlug(HttpContext httpContext)
    {
        var slug = httpContext.Request.RouteValues["slug"] as string;
        return (!string.IsNullOrWhiteSpace(slug))
            ? slug
            : null;
    }

    public async Task<Chapter?> GetChapterAsync(PlatformType platform)
    {
        var chapterName = _httpContext.Request.RouteValues["chapterName"] as string;
        if (string.IsNullOrWhiteSpace(chapterName))
        {
            return null;
        }

        try
        {
            var httpRequestContext = new HttpRequestContext(_httpContext.Request);
            return await _requestCache.GetChapterAsync(platform, chapterName);
        }
        catch
        {
            return null;
        }
    }

    public async Task<Member?> GetMemberAsync(Guid? memberId)
    {       
       if (!memberId.HasValue)
       {
            return null;
       }

       return await _requestCache.GetMemberAsync(memberId.Value);
    }
}
