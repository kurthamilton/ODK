using Microsoft.AspNetCore.Mvc;
using ODK.Services.Subscriptions;

namespace ODK.Web.Razor.Pages.Account;

public class SubscriptionModel : OdkSiteAccountPageModel
{
    private readonly ISiteSubscriptionService _siteSubscriptionService;

    public SubscriptionModel(ISiteSubscriptionService siteSubscriptionService)
    {
        _siteSubscriptionService = siteSubscriptionService;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(Guid id)
    {
        var result = await _siteSubscriptionService.CancelMemberSiteSubscription(MemberServiceRequest);
        AddFeedback(result, "Subscription cancelled");
        return RedirectToPage();
    }
}