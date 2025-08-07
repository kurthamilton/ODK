using Microsoft.AspNetCore.Mvc;
using ODK.Services;
using ODK.Services.Caching;
using ODK.Services.Chapters;
using ODK.Services.Settings;
using ODK.Web.Common.Feedback;
using ODK.Web.Razor.Models.SuperAdmin;

namespace ODK.Web.Razor.Pages.Chapters.Admin.SuperAdmin;

public class EmailProviderModel : ChapterSuperAdminPageModel
{
    private readonly IChapterAdminService _chapterAdminService;

    public EmailProviderModel(IRequestCache requestCache, IChapterAdminService chapterAdminService)
        : base(requestCache)
    {
        _chapterAdminService = chapterAdminService;
    }

    public Guid ChapterEmailProviderId { get; private set; }

    public IActionResult OnGet(Guid id)
    {
        ChapterEmailProviderId = id;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(Guid id, EmailProviderFormViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            return OnGet(id);
        }

        var serviceRequest = await GetAdminServiceRequest();
        var result = await _chapterAdminService.UpdateChapterEmailProvider(serviceRequest, id, new UpdateEmailProvider
        {
            BatchSize = viewModel.BatchSize,
            DailyLimit = viewModel.DailyLimit ?? 0,
            SmtpLogin = viewModel.SmtpLogin,
            SmtpPassword = viewModel.SmtpPassword,
            SmtpPort = viewModel.SmtpPort ?? 0,
            SmtpServer = viewModel.SmtpServer
        });

        if (!result.Success)
        {
            AddFeedback(new FeedbackViewModel(result));
            return OnGet(id);
        }

        AddFeedback(new FeedbackViewModel("Email provider updated", FeedbackType.Success));
        return Redirect($"/{ChapterName}/admin/superadmin/emails");
    }
}
