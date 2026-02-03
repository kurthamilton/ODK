using Microsoft.AspNetCore.Mvc.Filters;
using ODK.Core.Venues;
using ODK.Services.Security;
using ODK.Services.Venues;
using ODK.Web.Common.Feedback;

namespace ODK.Web.Razor.Pages.Chapters.Admin.Venues;

public abstract class VenueAdminPageModel : AdminPageModel
{
    protected VenueAdminPageModel(IVenueAdminService venueAdminService)
    {
        VenueAdminService = venueAdminService;
    }

    public override ChapterAdminSecurable Securable => ChapterAdminSecurable.Venues;

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

        var request = MemberChapterAdminServiceRequest;

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
        var redirectPath = AdminRoutes.Venues(Chapter).Path;
        context.Result = Redirect(redirectPath);
    }
}