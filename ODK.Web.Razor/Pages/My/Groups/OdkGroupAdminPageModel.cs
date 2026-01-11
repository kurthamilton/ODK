using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using ODK.Core;
using ODK.Services;
using ODK.Web.Common.Extensions;
using ODK.Web.Razor.Pages.Chapters;

namespace ODK.Web.Razor.Pages.My.Groups;

/// <summary>
/// Base class for all /my/groups/* pages
/// </summary>
[Authorize]
public abstract class OdkGroupAdminPageModel : OdkPageModel
{
    private readonly Lazy<MemberChapterServiceRequest> _adminServiceRequest;

    protected OdkGroupAdminPageModel()
    {
        _adminServiceRequest = new(() => MemberChapterServiceRequest.Create(ChapterId, MemberServiceRequest));
    }

    public Guid ChapterId { get; private set; }

    public MemberChapterServiceRequest AdminServiceRequest => _adminServiceRequest.Value;

    public override async Task OnPageHandlerExecutionAsync(
        PageHandlerExecutingContext context,
        PageHandlerExecutionDelegate next)
    {
        var chapterId = HttpContext.ChapterId();
        OdkAssertions.Exists(chapterId, "ChapterId missing");

        ChapterId = chapterId.Value;

        await base.OnPageHandlerExecutionAsync(context, next);
    }
}