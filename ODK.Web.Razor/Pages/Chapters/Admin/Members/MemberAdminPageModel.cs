using Microsoft.AspNetCore.Mvc.Filters;
using ODK.Core.Exceptions;
using ODK.Core.Members;
using ODK.Services.Caching;
using ODK.Services.Members;

namespace ODK.Web.Razor.Pages.Chapters.Admin.Members;

public abstract class MemberAdminPageModel : AdminPageModel
{
    protected MemberAdminPageModel(IRequestCache requestCache, IMemberAdminService memberAdminService) 
        : base(requestCache)
    {
        MemberAdminService = memberAdminService;
    }

    public Member Member { get; private set; } = null!;

    protected IMemberAdminService MemberAdminService { get; }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        await base.OnPageHandlerExecutionAsync(context, next);

        if (!Guid.TryParse(Request.RouteValues["id"]?.ToString(), out Guid id))
        {
            throw new OdkNotFoundException();
        }

        var request = await GetAdminServiceRequest();
        Member = await MemberAdminService.GetMember(request, id);        
    }
}
