using Microsoft.AspNetCore.Mvc;
using ODK.Services.Caching;
using ODK.Services.Settings;
using ODK.Web.Common.Feedback;
using ODK.Web.Razor.Models.SuperAdmin;

namespace ODK.Web.Razor.Pages.SuperAdmin;

public class EmailProviderCreateModel : SuperAdminPageModel
{
    private readonly ISettingsService _settingsService;

    public EmailProviderCreateModel(IRequestCache requestCache, ISettingsService settingsService)
        : base(requestCache)
    {
        _settingsService = settingsService;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(EmailProviderFormViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await _settingsService.AddEmailProvider(CurrentMemberId, new UpdateEmailProvider
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
            return Page();
        }

        AddFeedback(new FeedbackViewModel("Email provider created", FeedbackType.Success));
        return Redirect($"/SuperAdmin/EmailProviders");
    }
}
