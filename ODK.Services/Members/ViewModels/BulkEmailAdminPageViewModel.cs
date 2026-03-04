using ODK.Core.Chapters;
using ODK.Core.Features;

namespace ODK.Services.Members.ViewModels;

public class BulkEmailAdminPageViewModel
{
    public required Chapter Chapter { get; init; }

    public required IReadOnlyCollection<SiteFeatureType> OwnerSubscriptionFeatures { get; init; }
}