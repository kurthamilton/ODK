using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Services;

namespace ODK.Web.Razor.Pages.Chapters.Admin;

[Authorize(Roles = "Admin")]
public abstract class AdminPageModel : ChapterPageModel2
{
    private readonly Lazy<MemberChapterServiceRequest> _adminServiceRequest;

    protected AdminPageModel()
    {
        _adminServiceRequest = new(
            () => MemberChapterServiceRequest.Create(Chapter.Id, MemberServiceRequest));
    }

    public MemberChapterServiceRequest AdminServiceRequest => _adminServiceRequest.Value;

    public Chapter Chapter { get; set; } = null!;

    public Member CurrentMember { get; private set; } = null!;

    protected async Task<MemberChapterServiceRequest> GetAdminServiceRequest()
    {
        await LoadChapter();

        return AdminServiceRequest;
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context,
        PageHandlerExecutionDelegate next)
    {
        await base.OnPageHandlerExecutionAsync(context, next);

        await LoadChapter();
        CurrentMember = await GetCurrentMember();
    }

    protected async Task<Chapter> LoadChapter()
    {
        Chapter = await GetChapter();
        return Chapter;
    }
}