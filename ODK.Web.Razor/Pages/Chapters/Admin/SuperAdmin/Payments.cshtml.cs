using Microsoft.AspNetCore.Mvc;
using ODK.Services.Caching;
using ODK.Services.Chapters;
using ODK.Services.Chapters.Models;
using ODK.Web.Common.Feedback;
using ODK.Web.Razor.Models.Chapters.SuperAdmin;

namespace ODK.Web.Razor.Pages.Chapters.Admin.SuperAdmin;

public class PaymentSettingsModel : ChapterSuperAdminPageModel
{
    private readonly IChapterAdminService _chapterAdminService;

    public PaymentSettingsModel(IRequestCache requestCache, IChapterAdminService chapterAdminService) 
        : base(requestCache)
    {
        _chapterAdminService = chapterAdminService;
    }

    public async Task<IActionResult> OnPostAsync(PaymentSettingsFormViewModel viewModel)
    {
        var serviceRequest = await GetAdminServiceRequest();
        var existing = await _chapterAdminService.GetChapterPaymentSettings(serviceRequest);
        var result = await _chapterAdminService.UpdateChapterPaymentSettings(serviceRequest, 
            new UpdateChapterPaymentSettings
            {
                ApiPublicKey = viewModel.PublicKey,
                ApiSecretKey = viewModel.SecretKey,
                CurrencyId = viewModel.CurrencyId,
                EmailAddress = existing?.EmailAddress,
                Provider = viewModel.Provider,
                UseSitePaymentProvider = viewModel.UseSitePaymentProvider
            });

        if (!result.Success)
        {
            AddFeedback(new FeedbackViewModel(result));
            return Page();
        }

        AddFeedback(new FeedbackViewModel("Payment settings updated", FeedbackType.Success));
        return RedirectToPage();
    }
}
