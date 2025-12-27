using Microsoft.AspNetCore.Mvc;
using ODK.Services.Chapters;
using ODK.Services.Chapters.Models;
using ODK.Web.Razor.Models.Admin.Chapters;

namespace ODK.Web.Razor.Pages.My.Groups.Group.Payments;

public class IndexModel : OdkGroupAdminPageModel
{
    private readonly IChapterAdminService _chapterAdminService;

    public IndexModel(IChapterAdminService chapterAdminService)
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
        var result = await _chapterAdminService.UpdateChapterPaymentSettings(AdminServiceRequest, 
            new UpdateChapterPaymentSettings
            {
                ApiPublicKey = existing?.ApiPublicKey,
                ApiSecretKey = existing?.ApiSecretKey,
                CurrencyId = viewModel.CurrencyId,
                EmailAddress = null,
                Provider = null,
                UseSitePaymentProvider = existing?.UseSitePaymentProvider ?? false
            });

        AddFeedback(result, "Payment settings updated");

        return RedirectToPage();
    }
}
