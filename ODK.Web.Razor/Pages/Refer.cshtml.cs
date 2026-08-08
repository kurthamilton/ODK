using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ODK.Core.Exceptions;
using ODK.Core.Platforms;
using ODK.Core.Referrals;
using ODK.Services.Referrals;
using ODK.Web.Razor.Models.Account;

namespace ODK.Web.Razor.Pages;

/// <summary>
/// The member-facing refer-a-friend page. Not available on the DrunkenKnitwits platform, and 404s when
/// no campaign is running - the account menu link is hidden in both cases, so reaching this is either a
/// stale link or a typed URL.
/// </summary>
[Authorize]
public class ReferModel : OdkPageModel
{
    private readonly IReferralService _referralService;

    public ReferModel(IReferralService referralService)
    {
        _referralService = referralService;
    }

    public ReferralCampaign Campaign { get; private set; } = null!;

    public void OnGet()
    {
        Campaign = GetCampaign();
    }

    public async Task<IActionResult> OnPostAsync(ReferFormViewModel viewModel)
    {
        Campaign = GetCampaign();

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await _referralService.CreateReferral(MemberServiceRequest, viewModel.EmailAddress);

        AddFeedback(result);

        // Redirect on success so a refresh can't send a second referral; stay put on failure so the
        // message renders against the form the member just submitted.
        return result.Success ? RedirectToPage() : Page();
    }

    private ReferralCampaign GetCampaign()
    {
        if (Platform == PlatformType.DrunkenKnitwits)
        {
            throw new OdkNotFoundException();
        }

        return RequestStore.ActiveReferralCampaign ?? throw new OdkNotFoundException();
    }
}
