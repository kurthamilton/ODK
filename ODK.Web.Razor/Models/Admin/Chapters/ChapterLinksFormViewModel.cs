using ODK.Core.Chapters;
using ODK.Core.Features;

namespace ODK.Web.Razor.Models.Admin.Chapters;

public class ChapterLinksFormViewModel : ChapterLinksFormSubmitViewModel
{
    public required Chapter Chapter { get; init; }

    public required IReadOnlyCollection<SiteFeatureType> OwnerSubscriptionFeatures { get; init; }
}