using System.Text.RegularExpressions;
using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Services.Caching;
using ODK.Web.Common.Extensions;

namespace ODK.Web.Razor.Pages.Chapters
{
    public class ChapterPageContext
    {
        private static readonly Regex ChapterNameRegex = new(@"^[A-Za-z\s]+$", RegexOptions.Compiled);

        private readonly HttpContext? _httpContext;
        private readonly IRequestCache _requestCache;

        public ChapterPageContext(IRequestCache requestCache, HttpContext httpContext)
        {
            _httpContext = httpContext;
            _requestCache = requestCache;
        }
        
        public static string? GetChapterName(HttpContext httpContext)
        {
            var chapterName = httpContext.Request.RouteValues["chapterName"] as string;
            return (!string.IsNullOrWhiteSpace(chapterName) && ChapterNameRegex.IsMatch(chapterName))
                ? chapterName
                : null;
        }

        public async Task<Chapter?> GetChapterAsync()
        {
            var chapterName = _httpContext?.Request.RouteValues["chapterName"] as string;
            if (string.IsNullOrWhiteSpace(chapterName) || !ChapterNameRegex.IsMatch(chapterName))
            {
                return null;
            }

            try
            {
                return await _requestCache.GetChapterAsync(chapterName);
            }
            catch
            {
                return null;
            }
        }

        public async Task<Member?> GetMemberAsync()
        {
           var memberId = _httpContext?.User.MemberIdOrDefault();
           if (!memberId.HasValue)
           {
                return null;
           }

           return await _requestCache.GetMemberAsync(memberId.Value);
        }
    }
}
