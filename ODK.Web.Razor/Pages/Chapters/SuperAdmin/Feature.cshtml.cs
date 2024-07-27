using Microsoft.AspNetCore.Mvc;
using ODK.Core.Features;
using ODK.Services.Caching;
using ODK.Services.Features;
using ODK.Web.Common.Feedback;
using ODK.Web.Razor.Models.SuperAdmin;

namespace ODK.Web.Razor.Pages.Chapters.SuperAdmin;

public class FeatureModel : SuperAdminPageModel
{
    private readonly IFeatureService _featureService;

    public FeatureModel(IRequestCache requestCache, IFeatureService featureService) 
        : base(requestCache)
    {
        _featureService = featureService;
    }

    public Feature Feature { get; private set; } = null!;

    public async Task OnGetAsync(Guid id)
    {
        Feature = await _featureService.GetFeature(CurrentMemberId, id);
    }

    public async Task<IActionResult> OnPostAsync(Guid id, FeatureFormViewModel viewModel)
    {
        await _featureService.UpdateFeature(CurrentMemberId, id, new UpdateFeature
        {
            Description = viewModel.Description,
            Name = viewModel.Name
        });

        AddFeedback(new FeedbackViewModel("Feature updated", FeedbackType.Success));

        return RedirectToPage();
    }
}
