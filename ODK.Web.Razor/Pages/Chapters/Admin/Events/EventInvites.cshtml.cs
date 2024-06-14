using Microsoft.AspNetCore.Mvc;
using ODK.Services;
using ODK.Services.Caching;
using ODK.Services.Events;
using ODK.Web.Common.Feedback;

namespace ODK.Web.Razor.Pages.Chapters.Admin.Events;

public class EventInvitesModel : EventAdminPageModel
{
    public EventInvitesModel(IRequestCache requestCache, IEventAdminService eventAdminService) 
        : base(requestCache, eventAdminService)
    {
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(Guid id)
    {
        ServiceResult result = await EventAdminService.SendEventInvites(CurrentMemberId, id);
        if (!result.Success)
        {
            AddFeedback(new FeedbackViewModel(result));
        }

        return RedirectToPage();
    }
}
