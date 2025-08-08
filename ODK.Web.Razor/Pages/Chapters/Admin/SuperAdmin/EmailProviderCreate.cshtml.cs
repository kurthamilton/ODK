using Microsoft.AspNetCore.Mvc;
using ODK.Services.Caching;
using ODK.Services.Chapters;
using ODK.Services.Settings;
using ODK.Web.Common.Feedback;
using ODK.Web.Razor.Models.SuperAdmin;

namespace ODK.Web.Razor.Pages.Chapters.Admin.SuperAdmin;

public class EmailProviderCreateModel : ChapterSuperAdminPageModel
{
    private readonly IChapterAdminService _chapterAdminService;

    public EmailProviderCreateModel(IRequestCache requestCache, IChapterAdminService chapterAdminService)
        : base(requestCache)
    {
        _chapterAdminService = chapterAdminService;
    }

    public async Task<IActionResult> OnPostAsync(EmailProviderFormViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var serviceRequest = await GetAdminServiceRequest();
        var result = await _chapterAdminService.AddChapterEmailProvider(serviceRequest, new UpdateEmailProvider
        {
            BatchSize = viewModel.BatchSize,
            DailyLimit = viewModel.DailyLimit ?? 0,
            Name = viewModel.Name,
            SmtpLogin = viewModel.SmtpLogin,
            SmtpPassword = viewModel.SmtpPassword,
            SmtpPort = viewModel.SmtpPort ?? 0,
            SmtpServer = viewModel.SmtpServer
        });

        if (!result.Success)
        {
            AddFeedback(new FeedbackViewModel(result));
            return Page();
        }

        AddFeedback(new FeedbackViewModel("Email provider created", FeedbackType.Success));
        return Redirect($"/{Chapter.Name}/admin/superadmin/emails");
    }
}
