using Microsoft.AspNetCore.Mvc;
using ODK.Services.Referrals;
using ODK.Services.Referrals.Models;
using ODK.Services.Referrals.ViewModels;
using ODK.Web.Razor.Models.Feedback;
using ODK.Web.Razor.Models.SiteAdmin;

namespace ODK.Web.Razor.Pages.SiteAdmin;

public class ReferralCampaignModel : SiteAdminPageModel
{
    private readonly IReferralAdminService _referralAdminService;

    public ReferralCampaignModel(IReferralAdminService referralAdminService)
    {
        _referralAdminService = referralAdminService;
    }

    public ReferralCampaignAdminPageViewModel ViewModel { get; private set; } = null!;

    public async Task OnGetAsync(Guid id)
    {
        ViewModel = await _referralAdminService.GetCampaignViewModel(MemberServiceRequest, id);
    }

    public async Task<IActionResult> OnPostAsync(Guid id, ReferralCampaignFormViewModel viewModel)
    {
        var result = await _referralAdminService.UpdateCampaign(
            MemberServiceRequest,
            id,
            new ReferralCampaignUpdateModel
            {
                DescriptionHtml = viewModel.DescriptionHtml,
                EmailSubject = viewModel.EmailSubject,
                EmailTextHtml = viewModel.EmailTextHtml,
                ExpiresLocalDate = viewModel.ExpiresLocalDate,
                Name = viewModel.Name
            });

        AddFeedback(result, "Referral campaign updated");

        return RedirectToPage();
    }
}
