using Microsoft.AspNetCore.Mvc;
using ODK.Core;
using ODK.Core.Chapters;
using ODK.Core.Emails;
using ODK.Services.Caching;
using ODK.Services.Settings;
using ODK.Web.Common.Feedback;
using ODK.Web.Razor.Models.SuperAdmin;

namespace ODK.Web.Razor.Pages.SuperAdmin;

public class EmailProviderModel : SuperAdminPageModel
{
    private readonly ISettingsService _settingsService;

    public EmailProviderModel(IRequestCache requestCache, ISettingsService settingsService)
        : base(requestCache)
    {
        _settingsService = settingsService;
    }

    public EmailProvider Provider { get; private set; } = null!;

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        Provider = await _settingsService.GetEmailProvider(CurrentMemberId, id);
        OdkAssertions.Exists(Provider);

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(Guid id, EmailProviderFormViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            return await OnGetAsync(id);
        }

        var result = await _settingsService.UpdateEmailProvider(CurrentMemberId, id, new UpdateEmailProvider
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
            return await OnGetAsync(id);
        }

        AddFeedback(new FeedbackViewModel("Email provider updated", FeedbackType.Success));
        return Redirect("/SuperAdmin/EmailProviders");
    }
}
