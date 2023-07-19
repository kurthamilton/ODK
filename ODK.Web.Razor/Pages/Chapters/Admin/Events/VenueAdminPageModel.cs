using Microsoft.AspNetCore.Mvc.Filters;
using ODK.Core.Venues;
using ODK.Services.Caching;
using ODK.Services.Venues;

namespace ODK.Web.Razor.Pages.Chapters.Admin.Events
{
    public abstract class VenueAdminPageModel : AdminPageModel
    {
        protected VenueAdminPageModel(IRequestCache requestCache, IVenueAdminService venueAdminService) 
            : base(requestCache)
        {
            VenueAdminService = venueAdminService;
        }

        public Venue Venue { get; private set; } = null!;

        protected IVenueAdminService VenueAdminService { get; }

        public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
        {
            Guid.TryParse(Request.RouteValues["id"]?.ToString(), out Guid id);
            Venue = await VenueAdminService.GetVenue(CurrentMemberId, id);
            if (Venue == null)
            {
                Response.Redirect($"{Request.RouteValues["chapterName"]}/Admin/Events/Venues");
                return;
            }

            await base.OnPageHandlerExecutionAsync(context, next);
        }
    }
}
