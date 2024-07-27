using ODK.Core.Chapters;
using ODK.Core.Features;

namespace ODK.Web.Razor.Models.SuperAdmin;

public class FeatureContentViewModel
{
    public FeatureContentViewModel(Chapter chapter, Feature feature)
    {
        Chapter = chapter;
        Feature = feature;
    }

    public Chapter Chapter { get; }

    public Feature Feature { get; }
}
