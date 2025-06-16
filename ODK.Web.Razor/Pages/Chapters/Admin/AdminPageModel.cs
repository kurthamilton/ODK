using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using ODK.Core;
using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Services;
using ODK.Services.Caching;

namespace ODK.Web.Razor.Pages.Chapters.Admin;

[Authorize(Roles = "Admin")]
public abstract class AdminPageModel : ChapterPageModel2
{
    private readonly IRequestCache _requestCache;

    protected AdminPageModel(IRequestCache requestCache) 
    {
        _requestCache = requestCache;
    }

    public AdminServiceRequest AdminServiceRequest => new AdminServiceRequest(Chapter.Id, CurrentMemberId);

    public Chapter Chapter { get; set; } = null!;

    public Member CurrentMember { get; private set; } = null!;    

    protected async Task<AdminServiceRequest> GetAdminServiceRequest()
    {
        await LoadChapter();

        return new AdminServiceRequest(Chapter.Id, CurrentMemberId);
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, 
        PageHandlerExecutionDelegate next)
    {        
        await base.OnPageHandlerExecutionAsync(context, next);

        await LoadChapter();
        var member = await _requestCache.GetMemberAsync(CurrentMemberId);
        OdkAssertions.Exists(member);
        CurrentMember = member;        
    }

    protected async Task<Chapter> LoadChapter()
    {
        if (Chapter != null)
        {
            return Chapter;
        }

        Chapter = await _requestCache.GetChapterAsync(ChapterName);
        return Chapter;
    }
}
