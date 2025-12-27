using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using ODK.Core;
using ODK.Core.Chapters;
using ODK.Services;
using ODK.Web.Razor.Pages.Chapters;

namespace ODK.Web.Razor.Pages.My.Groups;

[Authorize]
public abstract class OdkGroupAdminPageModel : OdkPageModel
{
    private readonly Lazy<MemberChapterServiceRequest> _adminServiceRequest;

    protected OdkGroupAdminPageModel()
    {
        _adminServiceRequest = new(() => new MemberChapterServiceRequest(ChapterId, MemberServiceRequest));
    }

    public Guid ChapterId { get; private set; }

    public MemberChapterServiceRequest AdminServiceRequest => _adminServiceRequest.Value;

    public override async Task OnPageHandlerExecutionAsync(
        PageHandlerExecutingContext context,
        PageHandlerExecutionDelegate next)
    {
        var chapterId = ChapterPageContext.GetChapterId(HttpContext);
        OdkAssertions.Exists(chapterId, "ChapterId missing");

        ChapterId = chapterId.Value;
        
        await base.OnPageHandlerExecutionAsync(context, next);
    }

    public Task<Chapter> GetChapter() => RequestCache.GetChapterAsync(Platform, ChapterId);
}
