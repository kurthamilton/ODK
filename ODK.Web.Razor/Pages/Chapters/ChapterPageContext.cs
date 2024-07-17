using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Services.Caching;
using ODK.Web.Common.Extensions;

namespace ODK.Web.Razor.Pages.Chapters
{
    public class ChapterPageContext
    {
        private readonly HttpContext? _httpContext;
        private readonly IRequestCache _requestCache;

        public ChapterPageContext(IRequestCache requestCache, HttpContext httpContext)
        {
            _httpContext = httpContext;
            _requestCache = requestCache;
        }
        
        public async Task<Chapter?> GetChapterAsync()
        {
            string? chapterName = _httpContext?.Request.RouteValues["chapterName"] as string;
            if (string.IsNullOrWhiteSpace(chapterName))
            {
                return null;
            }

            return await _requestCache.GetChapterAsync(chapterName);
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
