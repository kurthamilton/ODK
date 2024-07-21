using Microsoft.AspNetCore.Mvc;
using ODK.Services.Caching;
using ODK.Services.Logging;

namespace ODK.Web.Razor.Pages.Chapters.SuperAdmin;

public class ErrorModel : SuperAdminPageModel
{
    private readonly ILoggingService _loggingService;

    public ErrorModel(IRequestCache requestCache, ILoggingService loggingService)
        : base(requestCache)
    {
        _loggingService = loggingService;
    }

    public ErrorDto Error { get; private set; } = null!;

    public async Task<IActionResult> OnGet(Guid id)
    {
        Error = await _loggingService.GetErrorDto(CurrentMemberId, id);
        return Page();
    }
}
