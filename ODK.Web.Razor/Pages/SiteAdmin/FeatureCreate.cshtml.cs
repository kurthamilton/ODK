using Microsoft.AspNetCore.Mvc;
using ODK.Services.Features;
using ODK.Services.Features.Models;
using ODK.Web.Common.Feedback;
using ODK.Web.Common.Routes;
using ODK.Web.Razor.Models.SiteAdmin;

namespace ODK.Web.Razor.Pages.SiteAdmin;

public class FeatureCreateModel : SiteAdminPageModel
{
    private readonly IFeatureService _featureService;

    public FeatureCreateModel(IFeatureService featureService)
    {
        _featureService = featureService;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(FeatureFormViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await _featureService.AddFeature(MemberServiceRequest, new FeatureUpdateModel
        {
            Description = viewModel.Description,
            Name = viewModel.Name
        });

        if (!result.Success)
        {
            AddFeedback(result);
            return Page();
        }

        AddFeedback("Feature created", FeedbackType.Success);
        return Redirect(OdkRoutes.SiteAdmin.Features);
    }
}