using ODK.Core.Chapters;
using ODK.Core.Features;

namespace ODK.Services.Chapters.ViewModels;

public class ChapterLinksAdminPageViewModel
{
    public required Chapter Chapter { get; init; }

    public required ChapterLinks? Links { get; init; }

    public required IReadOnlyCollection<SiteFeatureType> OwnerSubscriptionFeatures { get; init; }

    public required bool ShowInstagramFeed { get; init; }
}