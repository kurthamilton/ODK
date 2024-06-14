using Microsoft.AspNetCore.Mvc;
using ODK.Core.Chapters;
using ODK.Services;
using ODK.Services.Caching;
using ODK.Services.Emails;
using ODK.Web.Common.Feedback;
using ODK.Web.Razor.Models.SuperAdmin;

namespace ODK.Web.Razor.Pages.Chapters.SuperAdmin;

public class EmailProviderModel : SuperAdminPageModel
{
    private readonly IEmailAdminService _emailAdminService;

    public EmailProviderModel(IRequestCache requestCache, IEmailAdminService emailAdminService) 
        : base(requestCache)
    {
        _emailAdminService = emailAdminService;
    }

    public ChapterEmailProvider Provider { get; private set; } = null!;

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        Provider = await _emailAdminService.GetChapterEmailProvider(CurrentMemberId, id);
        if (Provider == null)
        {
            return NotFound();
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(Guid id, EmailProviderFormViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            return await OnGetAsync(id);
        }

        ServiceResult result = await _emailAdminService.UpdateChapterEmailProvider(CurrentMemberId, id, new UpdateChapterEmailProvider
        {
            BatchSize = viewModel.BatchSize,
            DailyLimit = viewModel.DailyLimit ?? 0,
            FromEmailAddress = viewModel.FromEmailAddress,
            FromName = viewModel.FromName,
            SmtpLogin = viewModel.SmtpLogin,
            SmtpPassword = viewModel.SmtpPassword,
            SmtpPort = viewModel.SmtpPort ?? 0,
            SmtpServer = viewModel.SmtpServer
        });

        if (!result.Success)
        {
            AddFeedback(new FeedbackViewModel(result));
            return await OnGetAsync(id);
        }

        AddFeedback(new FeedbackViewModel("Email provider updated", FeedbackType.Success));
        return Redirect($"/{Chapter.Name}/Admin/SuperAdmin/EmailProviders");
    }
}
