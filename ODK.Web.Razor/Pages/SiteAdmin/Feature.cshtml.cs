using Microsoft.AspNetCore.Mvc;
using ODK.Core.Features;
using ODK.Services.Features;
using ODK.Services.Features.Models;
using ODK.Web.Common.Feedback;
using ODK.Web.Razor.Models.SiteAdmin;

namespace ODK.Web.Razor.Pages.SiteAdmin;

public class FeatureModel : SiteAdminPageModel
{
    private readonly IFeatureService _featureService;

    public FeatureModel(IFeatureService featureService)
    {
        _featureService = featureService;
    }

    public Feature Feature { get; private set; } = null!;

    public async Task OnGetAsync(Guid id)
    {
        Feature = await _featureService.GetFeature(MemberServiceRequest, id);
    }

    public async Task<IActionResult> OnPostAsync(Guid id, FeatureFormViewModel viewModel)
    {
        await _featureService.UpdateFeature(MemberServiceRequest, id, new FeatureUpdateModel
        {
            Description = viewModel.Description,
            Name = viewModel.Name
        });

        AddFeedback("Feature updated", FeedbackType.Success);

        return RedirectToPage();
    }
}