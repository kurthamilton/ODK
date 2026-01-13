using Microsoft.AspNetCore.Mvc.Filters;
using ODK.Core.Chapters;
using ODK.Services;

namespace ODK.Web.Razor.Pages.Chapters;

public abstract class ChapterPageModel : OdkPageModel
{
    public Chapter Chapter { get; private set; } = null!;

    public MemberChapterServiceRequest CreateMemberChapterServiceRequest()
        => MemberChapterServiceRequest.Create(Chapter.Id, MemberServiceRequest);

    public override async Task OnPageHandlerExecutionAsync(
        PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        Chapter = await GetChapter();

        await base.OnPageHandlerExecutionAsync(context, next);
    }
}