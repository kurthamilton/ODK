using Microsoft.AspNetCore.Mvc;
using ODK.Core.Emails;
using ODK.Services.Emails;
using ODK.Services.Emails.Models;
using ODK.Web.Razor.Models.Admin.Chapters;
using ODK.Web.Razor.Models.Feedback;

namespace ODK.Web.Razor.Pages.SiteAdmin;

public class EmailModel : SiteAdminPageModel
{
    private readonly IEmailAdminService _emailAdminService;

    public EmailModel(IEmailAdminService emailAdminService)
    {
        _emailAdminService = emailAdminService;
    }

    public EmailType Type { get; private set; }

    public void OnGet(EmailType type)
    {
        Type = type;
    }

    public async Task<IActionResult> OnPostAsync(EmailType type,
        [FromForm] ChapterEmailFormSubmitViewModel viewModel,
        [FromForm] bool overridable)
    {
        // Set for the failure path below, which renders the page and so loads the email by this type.
        Type = type;

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
        return Redirect(OdkRoutes.SiteAdmin.Emails.Path);
    }
}
