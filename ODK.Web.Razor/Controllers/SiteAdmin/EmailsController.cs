using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ODK.Core.Emails;
using ODK.Services.Authentication;
using ODK.Services.Emails;
using ODK.Web.Common.Routes;
using ODK.Web.Common.Services;
using ODK.Web.Razor.Models.Admin;
using ODK.Web.Razor.Models.Admin.Emails;
using ODK.Web.Razor.Models.SiteAdmin;

namespace ODK.Web.Razor.Controllers.SiteAdmin;

[Authorize(Roles = OdkRoles.SiteAdmin)]
public class EmailsController : OdkControllerBase
{
    private readonly IEmailAdminService _emailAdminService;

    public EmailsController(
        IEmailAdminService emailAdminService,
        IRequestStore requestStore,
        IOdkRoutes odkRoutes)
        : base(requestStore, odkRoutes)
    {
        _emailAdminService = emailAdminService;
    }

    /* Binds the same view model the save binds, so the preview is of what saving would send rather than of
       what is stored. */
    [HttpPost("/siteadmin/emails/{type}/preview")]
    public async Task<IActionResult> PreviewEmail(
        EmailType type, [FromForm] SiteEmailFormSubmitViewModel viewModel)
    {
        var preview = await _emailAdminService.PreviewSiteEmail(
            MemberServiceRequest, type, viewModel.Subject, viewModel.ContentHtml);

        return Ok(EmailPreviewViewModel.FromRendered(preview));
    }

    /* Answers with its feedback rather than redirecting: the form is posted by script, which shows the
       result without the editor being reloaded and the author losing what they were part-way through
       writing. */
    [HttpPost("/siteadmin/emails/{type}/send/test")]
    public async Task<IActionResult> SendTestEmail(EmailType type)
    {
        var result = await _emailAdminService.SendTestMemberEmail(MemberServiceRequest, type);
        return FeedbackResponse(result, "Test email sent");
    }

    /* The site-admin counterpart of the group endpoint, and the same contract - see
       HtmlValidationResultViewModel. */
    [HttpPost("/siteadmin/emails/{type}/validate")]
    public IActionResult ValidateEmail(EmailType type, [FromForm] string? content)
    {
        var result = _emailAdminService.ValidateEmailHtml(MemberServiceRequest, type, content);

        return Ok(new HtmlValidationResultViewModel
        {
            Message = result.Message,
            Valid = result.Success
        });
    }
}