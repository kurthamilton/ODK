using Microsoft.AspNetCore.Mvc;
using ODK.Core.Emails;
using ODK.Services;
using ODK.Services.Caching;
using ODK.Services.Emails;
using ODK.Web.Common.Feedback;
using ODK.Web.Razor.Models.Admin.Chapters;

namespace ODK.Web.Razor.Pages.Chapters.Admin.Chapters;

public class EmailModel : AdminPageModel
{
    private readonly IEmailAdminService _emailAdminService;

    public EmailModel(IRequestCache requestCache, IEmailAdminService emailAdminService)
        : base(requestCache)
    {
        _emailAdminService = emailAdminService;
    }

    public ChapterEmail Email { get; private set; } = null!;

    public async Task<IActionResult> OnGetAsync(EmailType type)
    {
        var request = GetAdminServiceRequest();
        Email = await _emailAdminService.GetChapterEmail(request, type);        
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(EmailType type, ChapterEmailFormViewModel viewModel)
    {
        var request = GetAdminServiceRequest();
        var result = await _emailAdminService.UpdateChapterEmail(request, type, new UpdateEmail
        {
            HtmlContent = viewModel.Content,
            Subject = viewModel.Subject
        });

        if (!result.Success)
        {
            AddFeedback(new FeedbackViewModel(result));
            return Page();
        }

        AddFeedback(new FeedbackViewModel("Email updated", FeedbackType.Success));
        return Redirect($"/{Chapter.Name}/Admin/Chapter/Emails");
    }
}
