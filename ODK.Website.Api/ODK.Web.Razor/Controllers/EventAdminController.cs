using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ODK.Services;
using ODK.Services.Events;
using ODK.Web.Common.Feedback;

namespace ODK.Web.Razor.Controllers
{
    [Authorize(Roles = "Admin")]
    public class EventAdminController : OdkControllerBase
    {
        private readonly IEventAdminService _eventAdminService;

        public EventAdminController(IEventAdminService eventAdminService)
        {
            _eventAdminService = eventAdminService;
        }

        [HttpPost("{chapterName}/Admin/Events/{id:guid}/Delete")]
        public async Task<IActionResult> DeleteEvent(string chapterName, Guid id)
        {
            await _eventAdminService.DeleteEvent(MemberId, id);
            AddFeedback(new FeedbackViewModel("Event deleted", FeedbackType.Success));
            return Redirect($"/{chapterName}/Admin/Events");
        }

        [HttpPost("{chapterName}/Admin/Events/{id:guid}/Invites/Send")]
        public IActionResult SendInvites(Guid id)
        {
            return RedirectToReferrer();
        }

        [HttpPost("{chapterName}/Admin/Events/{id:guid}/Invites/SendTest")]
        public async Task<IActionResult> SendTestInvites(Guid id)
        {
            ServiceResult result = await _eventAdminService.SendEventInvites(MemberId, id, true);
            if (result.Success)
            {
                AddFeedback(new FeedbackViewModel("Invites sent", FeedbackType.Success));
            }
            else
            {
                AddFeedback(new FeedbackViewModel(result));
            }

            return RedirectToReferrer();
        }
    }
}
