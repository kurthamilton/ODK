using Microsoft.AspNetCore.Mvc;
using ODK.Services.Chapters;
using ODK.Services.Chapters.Models;
using ODK.Web.Razor.Models.Chapters.SiteAdmin;
using ODK.Web.Razor.Models.Feedback;

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
        var request = MemberChapterAdminServiceRequest;
        var result = await _chapterAdminService.UpdateChapterPaymentSettings(request,
            new ChapterPaymentSettingsUpdateModel
            {
                CurrencyId = viewModel.CurrencyId
            });

        if (!result.Success)
        {
            AddFeedback(result);
            return Page();
        }

        AddFeedback("Payment settings updated", FeedbackType.Success);
        return RedirectToPage();
    }
}