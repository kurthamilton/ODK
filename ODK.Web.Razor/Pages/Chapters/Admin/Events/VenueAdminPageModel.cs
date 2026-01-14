using Microsoft.AspNetCore.Mvc.Filters;
using ODK.Core.Venues;
using ODK.Services.Events;
using ODK.Services.Venues;
using ODK.Web.Common.Feedback;
using ODK.Web.Common.Routes;

namespace ODK.Web.Razor.Pages.Chapters.Admin.Events;

public abstract class VenueAdminPageModel : AdminPageModel
{
    protected VenueAdminPageModel(IVenueAdminService venueAdminService)
    {
        VenueAdminService = venueAdminService;
    }

    public Venue Venue { get; private set; } = null!;

    protected IVenueAdminService VenueAdminService { get; }

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
            Venue = await VenueAdminService.GetVenue(request, id);
            await next();
        }
        catch
        {
            AddFeedback("Venue not found", FeedbackType.Error);

            await Redirect(context);
        }
    }

    private async Task Redirect(PageHandlerExecutingContext context)
    {
        var chapter = await RequestStore.GetChapter();
        var redirectPath = OdkRoutes.MemberGroups.Venues(Platform, chapter);
        context.Result = Redirect(redirectPath);
    }
}