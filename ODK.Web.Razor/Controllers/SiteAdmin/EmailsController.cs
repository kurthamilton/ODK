using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ODK.Core.Emails;
using ODK.Services.Authentication;
using ODK.Services.Emails;
using ODK.Services.Settings;
using ODK.Services.Settings.Models;
using ODK.Web.Common.Routes;
using ODK.Web.Common.Services;
using ODK.Web.Razor.Models.Admin.Chapters;
using ODK.Web.Razor.Models.Admin.Emails;
using ODK.Web.Razor.Models.Feedback;
using ODK.Web.Razor.Models.SiteAdmin;

namespace ODK.Web.Razor.Controllers.SiteAdmin;

[Authorize(Roles = OdkRoles.SiteAdmin)]
public class EmailsController : OdkControllerBase
{
    private readonly IEmailAdminService _emailAdminService;
    private readonly ISettingsService _settingsService;

    public EmailsController(
        ISettingsService settingsService,
        IEmailAdminService emailAdminService,
        IRequestStore requestStore,
        IOdkRoutes odkRoutes)
        : base(requestStore, odkRoutes)
    {
        _emailAdminService = emailAdminService;
        _settingsService = settingsService;
    }

    /* Binds the same view model the save binds, so the preview is of what saving would send rather than of
       what is stored. */
    [HttpPost("/siteadmin/emails/{type}/preview")]
    public async Task<IActionResult> PreviewEmail(
        EmailType type, [FromForm] SiteEmailFormSubmitViewModel viewModel)
    {
        var preview = await _emailAdminService.PreviewEmail(
            MemberServiceRequest, type, viewModel.Subject, viewModel.Content);

        return Ok(EmailPreviewViewModel.FromRendered(preview));
    }

    [HttpPost("/siteadmin/emails/{type}/send/test")]
    public async Task<IActionResult> SendTestEmail(string chapterName, EmailType type)
    {
        var result = await _emailAdminService.SendTestMemberEmail(MemberServiceRequest, type);
        AddFeedback(result, "Test email sent");
        return RedirectToReferrer();
    }

    /* The site-admin counterpart of the group endpoint, and the same contract - see
       EmailHtmlValidationResultViewModel. */
    [HttpPost("/siteadmin/emails/{type}/validate")]
    public IActionResult ValidateEmail(EmailType type, [FromForm] string? content)
    {
        var result = _emailAdminService.ValidateEmailHtml(MemberServiceRequest, type, content);

        return Ok(new EmailHtmlValidationResultViewModel
        {
            Message = result.Message,
            Valid = result.Success
        });
    }

    [HttpPost("siteadmin/emails/settings")]
    public async Task<IActionResult> UpdateSettings(SiteEmailSettingsViewModel viewModel)
    {
        var model = new EmailSettingsUpdateModel
        {
            AdminTitle = viewModel.AdminTitle,
            FromEmailAddress = viewModel.FromEmailAddress,
            FromEmailName = viewModel.FromEmailName,
            MemberTitle = viewModel.MemberTitle,
            PlatformTitle = viewModel.PlatformTitle
        };

        await _settingsService.UpdateEmailSettings(MemberServiceRequest, model);
        AddFeedback("Email settings updated", FeedbackType.Success);
        return RedirectToReferrer();
    }
}