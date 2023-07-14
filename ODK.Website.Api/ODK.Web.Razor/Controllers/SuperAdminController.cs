using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ODK.Services.Emails;
using ODK.Web.Common.Feedback;

namespace ODK.Web.Razor.Controllers
{
    [Authorize(Roles = "SuperAdmin")]
    public class SuperAdminController : OdkControllerBase
    {
        private readonly IEmailAdminService _emailAdminService;

        public SuperAdminController(IEmailAdminService emailAdminService)
        {
            _emailAdminService = emailAdminService;
        }

        [HttpPost("{chapterAdmin}/Admin/SuperAdmin/EmailProviders/{id:guid}/Delete")]
        public async Task<IActionResult> DeleteEmailProvider(Guid id)
        {
            await _emailAdminService.DeleteChapterEmailProvider(MemberId, id);

            AddFeedback(new FeedbackViewModel("Email provider deleted", FeedbackType.Success));

            return RedirectToReferrer();
        }
    }
}
