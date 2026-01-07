using Microsoft.AspNetCore.Mvc.Filters;
using ODK.Core.Venues;
using ODK.Services.Caching;
using ODK.Services.Venues;

namespace ODK.Web.Razor.Pages.Chapters.Admin.Events;

public abstract class VenueAdminPageModel : AdminPageModel
{
    protected VenueAdminPageModel(IRequestCache requestCache, IVenueAdminService venueAdminService)
        : base(requestCache)
    {
        VenueAdminService = venueAdminService;
    }

    public Venue Venue { get; private set; } = null!;

    public Guid VenueId => Guid.TryParse(Request.RouteValues["id"]?.ToString(), out Guid id) ? id : Guid.Empty;

    protected IVenueAdminService VenueAdminService { get; }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        await base.OnPageHandlerExecutionAsync(context, next);

        var request = await GetAdminServiceRequest();
        Venue = await VenueAdminService.GetVenue(request, VenueId);
        if (Venue == null)
        {
            Response.Redirect($"{Request.RouteValues["chapterName"]}/Admin/Events/Venues");
            return;
        }
    }
}
