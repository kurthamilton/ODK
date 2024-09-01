using Microsoft.AspNetCore.Mvc;
using ODK.Services.Chapters;
using ODK.Web.Razor.Models.Admin.Chapters;

namespace ODK.Web.Razor.Pages.My.Groups.Group;

public class PaymentsModel : OdkGroupAdminPageModel
{
    private readonly IChapterAdminService _chapterAdminService;

    public PaymentsModel(IChapterAdminService chapterAdminService)
    {
        _chapterAdminService = chapterAdminService;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(
        [FromForm] ChapterPaymentSettingsFormViewModel viewModel)
    {
        // temp while transitioning from Api keys to emails
        var existing = await _chapterAdminService.GetChapterPaymentSettings(AdminServiceRequest);
        var result = await _chapterAdminService.UpdateChapterPaymentSettings(AdminServiceRequest, new UpdateChapterPaymentSettings
        {
            ApiPublicKey = existing?.ApiPublicKey,
            ApiSecretKey = existing?.ApiSecretKey,
            CurrencyId = viewModel.CurrencyId,
            Provider = viewModel.Provider
        });

        AddFeedback(result, "Payment settings updated");

        return RedirectToPage();
    }
}
