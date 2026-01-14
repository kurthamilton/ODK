using Microsoft.AspNetCore.Mvc.Filters;
using ODK.Core.Events;
using ODK.Services.Events;
using ODK.Web.Common.Feedback;
using ODK.Web.Common.Routes;

namespace ODK.Web.Razor.Pages.Chapters.Admin.Events;

public abstract class EventAdminPageModel : AdminPageModel
{
    protected EventAdminPageModel(IEventAdminService eventAdminService)
    {
        EventAdminService = eventAdminService;
    }

    public Event Event { get; private set; } = null!;

    protected IEventAdminService EventAdminService { get; }

    public override async Task OnPageHandlerExecutionAsync(
        PageHandlerExecutingContext context,
        PageHandlerExecutionDelegate next)
    {
        if (!Guid.TryParse(Request.RouteValues["id"]?.ToString(), out Guid id))
        {
            await Redirect(context);
            return;
        }

        var request = await CreateMemberChapterServiceRequest();

        try
        {
            Event = await EventAdminService.GetEvent(request, id);
            await next();
        }
        catch
        {
            AddFeedback("Event not found", FeedbackType.Error);

            await Redirect(context);
        }
    }

    private async Task Redirect(PageHandlerExecutingContext context)
    {
        var chapter = await RequestStore.GetChapter();
        var redirectPath = OdkRoutes.MemberGroups.Events(Platform, chapter);
        context.Result = Redirect(redirectPath);
    }
}