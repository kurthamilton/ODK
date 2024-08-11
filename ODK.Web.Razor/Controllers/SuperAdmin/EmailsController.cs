using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ODK.Core.Emails;
using ODK.Services.Authentication;
using ODK.Services.Emails;
using ODK.Services.Settings;
using ODK.Web.Common.Feedback;
using ODK.Web.Razor.Models.SuperAdmin;

namespace ODK.Web.Razor.Controllers.SuperAdmin;

[Authorize(Roles = OdkRoles.SuperAdmin)]
public class EmailsController : OdkControllerBase
{
    private readonly IEmailAdminService _emailAdminService;
    private readonly ISettingsService _settingsService;

    public EmailsController(ISettingsService settingsService, IEmailAdminService emailAdminService)
    {
        _emailAdminService = emailAdminService;
        _settingsService = settingsService;
    }

    [HttpPost("/SuperAdmin/Emails/{type}/Send/Test")]
    public async Task<IActionResult> SendTestEmail(string chapterName, EmailType type)
    {
        var result = await _emailAdminService.SendTestEmail(MemberId, type);
        AddFeedback(result, "Test email sent");
        return RedirectToReferrer();
    }

    [HttpPost("SuperAdmin/Emails/Providers/{id:guid}/Delete")]
    public async Task<IActionResult> DeleteEmailProvider(Guid id)
    {
        await _settingsService.DeleteEmailProvider(MemberId, id);
        AddFeedback("Email provider deleted", FeedbackType.Success);
        return RedirectToReferrer();
    }

    [HttpPost("SuperAdmin/Emails/Settings")]
    public async Task<IActionResult> UpdateSettings(SiteEmailSettingsViewModel viewModel)
    {
        await _settingsService.UpdateEmailSettings(MemberId, 
            viewModel.FromEmailAddress, 
            viewModel.FromEmailName, 
            viewModel.Title,
            viewModel.ContactEmailAddress);
        AddFeedback("Email settings updated", FeedbackType.Success);
        return RedirectToReferrer();
    }
}
