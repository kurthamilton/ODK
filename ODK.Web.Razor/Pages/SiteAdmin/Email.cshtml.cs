using Microsoft.AspNetCore.Mvc;
using ODK.Core.Emails;
using ODK.Services.Emails;
using ODK.Services.Emails.Models;
using ODK.Web.Razor.Models.Feedback;
using ODK.Web.Razor.Models.SiteAdmin;

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
        [FromForm] SiteEmailFormSubmitViewModel viewModel,
        [FromForm] bool isGroupEmail)
    {
        // Set for the failure path below, which renders the page and so loads the email by this type.
        Type = type;

        var result = await _emailAdminService.UpdateEmail(MemberServiceRequest, type, new EmailUpdateModel
        {
            HtmlContent = viewModel.Content,
            IsGroupEmail = isGroupEmail,
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
