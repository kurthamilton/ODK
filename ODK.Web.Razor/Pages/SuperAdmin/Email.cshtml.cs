using Microsoft.AspNetCore.Mvc;
using ODK.Core.Emails;
using ODK.Services.Caching;
using ODK.Services.Emails;
using ODK.Web.Common.Feedback;
using ODK.Web.Razor.Models.Admin.Chapters;

namespace ODK.Web.Razor.Pages.SuperAdmin;

public class EmailModel : SuperAdminPageModel
{
    private readonly IEmailAdminService _emailAdminService;

    public EmailModel(IRequestCache requestCache, IEmailAdminService emailAdminService)
        : base(requestCache)
    {
        _emailAdminService = emailAdminService;
    }

    public Email Email { get; private set; } = null!;

    public async Task<IActionResult> OnGetAsync(EmailType type)
    {
        Email = await _emailAdminService.GetEmail(CurrentMemberId, type);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(EmailType type, ChapterEmailFormViewModel viewModel)
    {
        var result = await _emailAdminService.UpdateEmail(CurrentMemberId, type, new UpdateEmail
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
        return Redirect($"/SuperAdmin/Emails");
    }
}
