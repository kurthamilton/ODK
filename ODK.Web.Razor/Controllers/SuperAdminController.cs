using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ODK.Services.Emails;
using ODK.Services.Logging;
using ODK.Web.Common.Feedback;

namespace ODK.Web.Razor.Controllers
{
    [Authorize(Roles = "SuperAdmin")]
    public class SuperAdminController : OdkControllerBase
    {
        private readonly IEmailAdminService _emailAdminService;
        private readonly ILoggingService _loggingService;

        public SuperAdminController(IEmailAdminService emailAdminService, 
            ILoggingService loggingService)
        {
            _emailAdminService = emailAdminService;
            _loggingService = loggingService;
        }

        [HttpPost("{chapterName}/Admin/SuperAdmin/EmailProviders/{id:guid}/Delete")]
        public async Task<IActionResult> DeleteEmailProvider(Guid id)
        {
            await _emailAdminService.DeleteChapterEmailProvider(MemberId, id);

            AddFeedback(new FeedbackViewModel("Email provider deleted", FeedbackType.Success));

            return RedirectToReferrer();
        }

        [HttpPost("{chapterName}/Admin/SuperAdmin/Errors/{id:int}/Delete")]
        public async Task<IActionResult> DeleteError(string chapterName, int id)
        {
            await _loggingService.DeleteError(MemberId, id);

            return Redirect($"/{chapterName}/Admin/SuperAdmin/Errors");
        }

        [HttpPost("{chapterName}/Admin/SuperAdmin/Errors/{id:int}/DeleteAll")]
        public async Task<IActionResult> DeleteAllErrors(string chapterName, int id)
        {
            await _loggingService.DeleteAllErrors(MemberId, id);

            return Redirect($"/{chapterName}/Admin/SuperAdmin/Errors");
        }
    }
}
