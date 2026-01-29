using Microsoft.AspNetCore.Mvc;
using ODK.Services.Chapters;

namespace ODK.Web.Razor.Pages.Chapters.Admin.Payments;

public class AccountModel : AdminPageModel
{
    private const string RefreshAction = "refresh";

    private readonly IChapterAdminService _chapterAdminService;

    public AccountModel(IChapterAdminService chapterAdminService)
    {
        _chapterAdminService = chapterAdminService;
    }

    public async Task<IActionResult> OnGet(string? action = null)
    {
        if (action == RefreshAction)
        {
            var request = MemberChapterServiceRequest;

            var returnPath = OdkRoutes.Payments.PaymentAccount(Chapter);
            var refreshPath = $"{returnPath}?action={RefreshAction}";

            var url = await _chapterAdminService.GenerateChapterPaymentAccountSetupUrl(
                request,
                refreshPath: refreshPath,
                returnPath: returnPath);

            if (url.Success && !string.IsNullOrEmpty(url.Value))
            {
                return Redirect(url.Value);
            }
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var request = MemberChapterServiceRequest;

        var returnPath = OdkRoutes.Payments.PaymentAccount(Chapter);
        var refreshPath = $"{returnPath}?action={RefreshAction}";

        var result = await _chapterAdminService.CreateChapterPaymentAccount(
            request,
            refreshPath: refreshPath,
            returnPath: returnPath);

        if (result.Success && result.Value?.OnboardingUrl != null)
        {
            return Redirect(result.Value.OnboardingUrl);
        }

        AddFeedback(result);
        return Page();
    }
}