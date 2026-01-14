using Microsoft.AspNetCore.Mvc;
using ODK.Core.Emails;
using ODK.Services.Emails;
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

    public async Task<IActionResult> OnGetAsync(EmailType type)
    {
        var request = await CreateMemberChapterServiceRequest();
        Email = await _emailAdminService.GetChapterEmail(request, type);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(EmailType type, ChapterEmailFormViewModel viewModel)
    {
        var request = await CreateMemberChapterServiceRequest();
        var result = await _emailAdminService.UpdateChapterEmail(request, type, new UpdateEmail
        {
            HtmlContent = viewModel.Content,
            Overridable = false,
            Subject = viewModel.Subject
        });

        if (!result.Success)
        {
            AddFeedback(new FeedbackViewModel(result));
            return Page();
        }

        var chapter = await GetChapter();
        AddFeedback(new FeedbackViewModel("Email updated", FeedbackType.Success));
        return Redirect($"/{chapter.ShortName}/Admin/Chapter/Emails");
    }
}