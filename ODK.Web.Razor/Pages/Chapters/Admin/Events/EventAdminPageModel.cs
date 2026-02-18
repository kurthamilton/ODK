using Microsoft.AspNetCore.Mvc.Filters;
using ODK.Services.Events;
using ODK.Services.Security;
using ODK.Web.Razor.Models.Feedback;
using CoreEvent = ODK.Core.Events.Event;

namespace ODK.Web.Razor.Pages.Chapters.Admin.Events;

public abstract class EventAdminPageModel : AdminPageModel
{
    protected EventAdminPageModel(IEventAdminService eventAdminService)
    {
        EventAdminService = eventAdminService;
    }

    public CoreEvent Event { get; private set; } = null!;

    protected IEventAdminService EventAdminService { get; }

    public override ChapterAdminSecurable Securable => ChapterAdminSecurable.Events;

    public override async Task OnPageHandlerExecutionAsync(
        PageHandlerExecutingContext context,
        PageHandlerExecutionDelegate next)
    {
        if (!Guid.TryParse(Request.RouteValues["id"]?.ToString(), out Guid id))
        {
            Redirect(context);
            return;
        }

        var request = MemberChapterAdminServiceRequest;

        try
        {
            Event = await EventAdminService.GetEvent(request, id);
            await next();
        }
        catch
        {
            AddFeedback("Event not found", FeedbackType.Error);

            Redirect(context);
        }
    }

    private void Redirect(PageHandlerExecutingContext context)
    {
        var redirectPath = AdminRoutes.Events(Chapter).Path;
        context.Result = Redirect(redirectPath);
    }
}