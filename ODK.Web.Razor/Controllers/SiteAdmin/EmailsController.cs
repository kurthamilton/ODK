using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ODK.Core.Emails;
using ODK.Services.Authentication;
using ODK.Services.Emails;
using ODK.Services.Settings;
using ODK.Services.Settings.Models;
using ODK.Web.Common.Feedback;
using ODK.Web.Razor.Models.SiteAdmin;
using ODK.Web.Razor.Services;

namespace ODK.Web.Razor.Controllers.SiteAdmin;

[Authorize(Roles = OdkRoles.SiteAdmin)]
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

    [HttpPost("/siteadmin/emails/{type}/send/test")]
    public async Task<IActionResult> SendTestEmail(string chapterName, EmailType type)
    {
        var result = await _emailAdminService.SendTestMemberEmail(MemberServiceRequest, type);
        AddFeedback(result, "Test email sent");
        return RedirectToReferrer();
    }

    [HttpPost("siteadmin/emails/settings")]
    public async Task<IActionResult> UpdateSettings(SiteEmailSettingsViewModel viewModel)
    {
        var model = new UpdateEmailSettings
        {
            ContactEmailAddress = viewModel.ContactEmailAddress,
            FromEmailAddress = viewModel.FromEmailAddress,
            FromEmailName = viewModel.FromEmailName,
            PlatformTitle = viewModel.PlatformTitle,
            Title = viewModel.Title
        };

        await _settingsService.UpdateEmailSettings(MemberServiceRequest, model);
        AddFeedback("Email settings updated", FeedbackType.Success);
        return RedirectToReferrer();
    }
}