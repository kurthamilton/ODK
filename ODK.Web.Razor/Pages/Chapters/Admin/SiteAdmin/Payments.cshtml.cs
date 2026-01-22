using Microsoft.AspNetCore.Mvc;
using ODK.Services.Chapters;
using ODK.Services.Chapters.Models;
using ODK.Web.Common.Feedback;
using ODK.Web.Razor.Models.Chapters.SiteAdmin;

namespace ODK.Web.Razor.Pages.Chapters.Admin.SiteAdmin;

public class PaymentSettingsModel : ChapterSiteAdminPageModel
{
    private readonly IChapterAdminService _chapterAdminService;

    public PaymentSettingsModel(IChapterAdminService chapterAdminService)
    {
        _chapterAdminService = chapterAdminService;
    }

    public async Task<IActionResult> OnPostAsync(PaymentSettingsFormViewModel viewModel)
    {
        var serviceRequest = await CreateMemberChapterServiceRequest();
        var result = await _chapterAdminService.UpdateChapterPaymentSettings(serviceRequest,
            new UpdateChapterPaymentSettings
            {
                CurrencyId = viewModel.CurrencyId
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