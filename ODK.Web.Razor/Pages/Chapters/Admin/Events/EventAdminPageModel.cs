using Microsoft.AspNetCore.Mvc.Filters;
using ODK.Core.Events;
using ODK.Services.Caching;
using ODK.Services.Events;

namespace ODK.Web.Razor.Pages.Chapters.Admin.Events;

public abstract class EventAdminPageModel : AdminPageModel
{
    protected EventAdminPageModel(IRequestCache requestCache, IEventAdminService eventAdminService)
        : base(requestCache)
    {
        EventAdminService = eventAdminService;
    }

    public Event Event { get; private set; } = null!;

    protected IEventAdminService EventAdminService { get; }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        await base.OnPageHandlerExecutionAsync(context, next);

        Guid.TryParse(Request.RouteValues["id"]?.ToString(), out Guid id);

        var request = await GetAdminServiceRequest();
        Event = await EventAdminService.GetEvent(request, id);
        if (Event == null)
        {
            Response.Redirect($"{ChapterName}/Admin/Events");
            return;
        }
    }
}
