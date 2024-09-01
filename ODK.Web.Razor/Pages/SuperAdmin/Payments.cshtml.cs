using Microsoft.AspNetCore.Mvc;
using ODK.Services.Caching;
using ODK.Services.Chapters;
using ODK.Services.Settings;
using ODK.Web.Common.Feedback;
using ODK.Web.Razor.Models.SuperAdmin;

namespace ODK.Web.Razor.Pages.SuperAdmin;

public class PaymentsModel : SuperAdminPageModel
{
    private readonly ISettingsService _settingsService;

    public PaymentsModel(IRequestCache requestCache, ISettingsService settingsService) 
        : base(requestCache)
    {
        _settingsService = settingsService;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(PaymentSettingsFormViewModel viewModel)
    {
        var result = await _settingsService.UpdatePaymentSettings(CurrentMemberId,
            viewModel.PublicKey ?? "",
            viewModel.SecretKey ?? "");

        if (!result.Success)
        {
            AddFeedback(new FeedbackViewModel(result));
            return Page();
        }

        AddFeedback(new FeedbackViewModel("Payment settings updated", FeedbackType.Success));
        return RedirectToPage();
    }
}
