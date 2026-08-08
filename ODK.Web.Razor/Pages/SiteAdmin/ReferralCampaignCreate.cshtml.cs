using Microsoft.AspNetCore.Mvc;
using ODK.Services.Referrals;
using ODK.Services.Referrals.Models;
using ODK.Web.Razor.Models.Feedback;
using ODK.Web.Razor.Models.SiteAdmin;

namespace ODK.Web.Razor.Pages.SiteAdmin;

public class ReferralCampaignCreateModel : SiteAdminPageModel
{
    private readonly IReferralAdminService _referralAdminService;

    public ReferralCampaignCreateModel(IReferralAdminService referralAdminService)
    {
        _referralAdminService = referralAdminService;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(ReferralCampaignFormViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await _referralAdminService.CreateCampaign(
            MemberServiceRequest,
            new ReferralCampaignUpdateModel
            {
                Description = viewModel.Description,
                EmailSubject = viewModel.EmailSubject,
                EmailText = viewModel.EmailText,
                ExpiresLocalDate = viewModel.ExpiresLocalDate,
                Name = viewModel.Name
            });

        if (!result.Success)
        {
            AddFeedback(result);
            return Page();
        }

        AddFeedback("Referral campaign created", FeedbackType.Success);

        // Straight to the new campaign, since the create form is the same one used to edit it.
        return Redirect(OdkRoutes.SiteAdmin.ReferralCampaign(result.Value).Path);
    }
}
