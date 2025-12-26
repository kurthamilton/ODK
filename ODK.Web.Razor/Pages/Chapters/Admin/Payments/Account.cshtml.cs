using Microsoft.AspNetCore.Mvc;
using ODK.Services.Caching;
using ODK.Services.Chapters;
using ODK.Web.Common.Routes;

namespace ODK.Web.Razor.Pages.Chapters.Admin.Payments;

public class AccountModel : AdminPageModel
{
    private const string RefreshAction = "refresh";

    private readonly IChapterAdminService _chapterAdminService;

    public AccountModel(
        IRequestCache requestCache,
        IChapterAdminService chapterAdminService)
        : base(requestCache)
    {
        _chapterAdminService = chapterAdminService;
    }

    public async Task<IActionResult> OnGet(string? action = null)
    {
        if (action == RefreshAction)
        {
            await LoadChapter();

            var request = AdminServiceRequest;

            var chapter = await _chapterAdminService.GetChapter(request);
            var returnPath = OdkRoutes.Payments.PaymentAccount(Platform, chapter);
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
        await LoadChapter();

        var request = AdminServiceRequest;

        var returnPath = OdkRoutes.Payments.PaymentAccount(Platform, Chapter);
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
