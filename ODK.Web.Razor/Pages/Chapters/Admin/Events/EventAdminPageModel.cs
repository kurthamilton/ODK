using Microsoft.AspNetCore.Mvc.Filters;
using ODK.Core.Events;
using ODK.Services.Events;
using ODK.Services.Security;
using ODK.Web.Common.Feedback;

namespace ODK.Web.Razor.Pages.Chapters.Admin.Events;

public abstract class EventAdminPageModel : AdminPageModel
{
    protected EventAdminPageModel(IEventAdminService eventAdminService)
    {
        EventAdminService = eventAdminService;
    }

    public Event Event { get; private set; } = null!;

    protected IEventAdminService EventAdminService { get; }

    public override ChapterAdminSecurable Securable => ChapterAdminSecurable.Events;

    public override async Task OnPageHandlerExecutionAsync(
        PageHandlerExecutingContext context,
        PageHandlerExecutionDelegate next)
    {
        if (!Guid.TryParse(Request.RouteValues["id"]?.ToString(), out Guid id))
        {
            await Redirect(context);
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

            await Redirect(context);
        }
    }

    private async Task Redirect(PageHandlerExecutingContext context)
    {
        var redirectPath = OdkRoutes.GroupAdmin.Events(Chapter);
        context.Result = Redirect(redirectPath);
    }
}