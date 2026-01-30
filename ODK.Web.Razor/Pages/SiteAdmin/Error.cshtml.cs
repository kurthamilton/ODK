using Microsoft.AspNetCore.Mvc;
using ODK.Services.Logging;

namespace ODK.Web.Razor.Pages.SiteAdmin;

public class ErrorModel : SiteAdminPageModel
{
    private readonly ILoggingService _loggingService;

    public ErrorModel(ILoggingService loggingService)
    {
        _loggingService = loggingService;
    }

    public ErrorDto Error { get; private set; } = null!;

    public async Task<IActionResult> OnGet(Guid id)
    {
        Error = await _loggingService.GetErrorDto(MemberServiceRequest, id);
        return Page();
    }
}