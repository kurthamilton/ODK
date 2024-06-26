using Microsoft.AspNetCore.Mvc;
using ODK.Services;
using ODK.Services.Caching;
using ODK.Services.Chapters;
using ODK.Web.Common.Feedback;
using ODK.Web.Razor.Models.SuperAdmin;

namespace ODK.Web.Razor.Pages.Chapters.SuperAdmin;

public class PaymentSettingsModel : SuperAdminPageModel
{
    private readonly IChapterAdminService _chapterAdminService;

    public PaymentSettingsModel(IRequestCache requestCache, IChapterAdminService chapterAdminService) 
        : base(requestCache)
    {
        _chapterAdminService = chapterAdminService;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(PaymentSettingsFormViewModel viewModel)
    {
        ServiceResult result = await _chapterAdminService.UpdateChapterPaymentSettings(CurrentMemberId, Chapter.Id, new UpdateChapterPaymentSettings
        {
            ApiPublicKey = viewModel.PublicKey,
            ApiSecretKey = viewModel.SecretKey
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
