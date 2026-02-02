using Microsoft.AspNetCore.Mvc;
using ODK.Core.Emails;
using ODK.Services.Emails;
using ODK.Services.Emails.Models;
using ODK.Web.Common.Feedback;
using ODK.Web.Common.Routes;
using ODK.Web.Razor.Models.Admin.Chapters;

namespace ODK.Web.Razor.Pages.SiteAdmin;

public class EmailModel : SiteAdminPageModel
{
    private readonly IEmailAdminService _emailAdminService;

    public EmailModel(IEmailAdminService emailAdminService)
    {
        _emailAdminService = emailAdminService;
    }

    public Email Email { get; private set; } = null!;

    public async Task<IActionResult> OnGetAsync(EmailType type)
    {
        Email = await _emailAdminService.GetEmail(MemberServiceRequest, type);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(EmailType type,
        [FromForm] ChapterEmailFormViewModel viewModel,
        [FromForm] bool overridable)
    {
        var result = await _emailAdminService.UpdateEmail(MemberServiceRequest, type, new EmailUpdateModel
        {
            HtmlContent = viewModel.Content,
            Overridable = overridable,
            Subject = viewModel.Subject
        });

        if (!result.Success)
        {
            AddFeedback(result);
            return Page();
        }

        AddFeedback("Email updated", FeedbackType.Success);
        return Redirect(OdkRoutes.SiteAdmin.Emails);
    }
}