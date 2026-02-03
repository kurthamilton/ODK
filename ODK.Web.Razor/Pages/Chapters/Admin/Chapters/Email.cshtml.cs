using Microsoft.AspNetCore.Mvc;
using ODK.Core.Emails;
using ODK.Services.Emails;
using ODK.Services.Emails.Models;
using ODK.Services.Security;
using ODK.Web.Common.Feedback;
using ODK.Web.Razor.Models.Admin.Chapters;

namespace ODK.Web.Razor.Pages.Chapters.Admin.Chapters;

public class EmailModel : AdminPageModel
{
    private readonly IEmailAdminService _emailAdminService;

    public EmailModel(IEmailAdminService emailAdminService)
    {
        _emailAdminService = emailAdminService;
    }

    public ChapterEmail Email { get; private set; } = null!;

    public override ChapterAdminSecurable Securable => ChapterAdminSecurable.Emails;

    public async Task<IActionResult> OnGetAsync(EmailType type)
    {
        var request = MemberChapterAdminServiceRequest;
        Email = await _emailAdminService.GetChapterEmail(request, type);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(EmailType type, ChapterEmailFormViewModel viewModel)
    {
        var request = MemberChapterAdminServiceRequest;
        var result = await _emailAdminService.UpdateChapterEmail(request, type, new EmailUpdateModel
        {
            HtmlContent = viewModel.Content,
            Overridable = false,
            Subject = viewModel.Subject
        });

        if (!result.Success)
        {
            AddFeedback(result);
            return Page();
        }

        var chapter = Chapter;
        AddFeedback("Email updated", FeedbackType.Success);
        return Redirect(AdminRoutes.Emails(chapter).Path);
    }
}