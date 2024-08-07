using ODK.Core.Features;

namespace ODK.Web.Razor.Models.SuperAdmin;

public class FeatureContentViewModel
{
    public FeatureContentViewModel(Feature feature)
    {
        Feature = feature;
    }

    public Feature Feature { get; }
}
