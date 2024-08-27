using Microsoft.AspNetCore.Mvc.Filters;
using ODK.Core;

namespace ODK.Web.Razor.Pages.My.Groups.Events;

public class EventModel : OdkGroupAdminPageModel
{
    public Guid EventId { get; private set; }

    public void OnGet()
    {
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        await base.OnPageHandlerExecutionAsync(context, next);

        var eventId = Guid.TryParse(context.HttpContext.Request.RouteValues["eventId"]?.ToString(), out var id)
            ? id
            : default(Guid?);

        OdkAssertions.Exists(eventId);

        EventId = eventId.Value;
    }
}
