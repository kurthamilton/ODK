using Microsoft.AspNetCore.Mvc.Filters;
using ODK.Core.Events;
using ODK.Services.Caching;
using ODK.Services.Events;

namespace ODK.Web.Razor.Pages.Chapters.Admin.Events
{
    public abstract class AdminEventPageModel : AdminPageModel
    {
        protected AdminEventPageModel(IRequestCache requestCache, IEventAdminService eventAdminService) 
            : base(requestCache)
        {
            EventAdminService = eventAdminService;
        }

        public Event Event { get; private set; } = null!;

        protected IEventAdminService EventAdminService { get; }

        public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
        {
            Guid.TryParse(Request.RouteValues["id"]?.ToString(), out Guid id);
            Event = await EventAdminService.GetEvent(CurrentMemberId, id);
            if (Event == null)
            {
                Response.Redirect($"{Request.RouteValues["chapterName"]}/Admin/Events");
                return;
            }

            await base.OnPageHandlerExecutionAsync(context, next);
        }
    }
}
