using Microsoft.AspNetCore.Mvc;
using ODK.Core.Logging;
using ODK.Services.Caching;
using ODK.Services.Logging;

namespace ODK.Web.Razor.Pages.Chapters.SuperAdmin
{
    public class ErrorModel : SuperAdminPageModel
    {
        private readonly ILoggingService _loggingService;

        public ErrorModel(IRequestCache requestCache, ILoggingService loggingService) 
            : base(requestCache)
        {
            _loggingService = loggingService;
        }

        public LogMessage Error { get; private set; } = null!;

        public async Task<IActionResult> OnGet(int id)
        {
            Error = await _loggingService.GetErrorMessage(CurrentMemberId, id);
            if (Error == null)
            {
                return NotFound();
            }

            return Page();
        }
    }
}
