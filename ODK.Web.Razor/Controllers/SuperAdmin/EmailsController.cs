using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ODK.Core.Emails;
using ODK.Services.Authentication;
using ODK.Services.Emails;
using ODK.Services.Settings;
using ODK.Web.Common.Feedback;
using ODK.Web.Razor.Models.SuperAdmin;
using ODK.Web.Razor.Services;

namespace ODK.Web.Razor.Controllers.SuperAdmin;

[Authorize(Roles = OdkRoles.SuperAdmin)]
public class EmailsController : OdkControllerBase
{
    private readonly IEmailAdminService _emailAdminService;
    private readonly ISettingsService _settingsService;

    public EmailsController(
        ISettingsService settingsService,
        IEmailAdminService emailAdminService,
        IRequestStore requestStore)
        : base(requestStore)
    {
        _emailAdminService = emailAdminService;
        _settingsService = settingsService;
    }

    [HttpPost("/superadmin/emails/{type}/send/test")]
    public async Task<IActionResult> SendTestEmail(string chapterName, EmailType type)
    {
        var result = await _emailAdminService.SendTestMemberEmail(MemberServiceRequest, type);
        AddFeedback(result, "Test email sent");
        return RedirectToReferrer();
    }

    [HttpPost("superadmin/emails/providers/{id:guid}/delete")]
    public async Task<IActionResult> DeleteEmailProvider(Guid id)
    {
        await _settingsService.DeleteEmailProvider(MemberId, id);
        AddFeedback("Email provider deleted", FeedbackType.Success);
        return RedirectToReferrer();
    }

    [HttpPost("superadmin/emails/settings")]
    public async Task<IActionResult> UpdateSettings(SiteEmailSettingsViewModel viewModel)
    {
        await _settingsService.UpdateEmailSettings(
            MemberServiceRequest,
            viewModel.FromEmailAddress,
            viewModel.FromEmailName,
            viewModel.Title,
            viewModel.ContactEmailAddress);
        AddFeedback("Email settings updated", FeedbackType.Success);
        return RedirectToReferrer();
    }
}
