using ODK.Core.Features;

namespace ODK.Web.Razor.Models.SiteAdmin;

public class FeatureContentViewModel
{
    public FeatureContentViewModel(Feature feature)
    {
        Feature = feature;
    }

    public Feature Feature { get; }
}