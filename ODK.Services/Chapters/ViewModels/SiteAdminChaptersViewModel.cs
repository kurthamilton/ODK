using ODK.Core.Platforms;

namespace ODK.Services.Chapters.ViewModels;

public class SiteAdminChaptersViewModel
{
    public required IReadOnlyCollection<SiteAdminChaptersRowViewModel> Approved { get; init; }

    public required IReadOnlyCollection<SiteAdminChaptersRowViewModel> Pending { get; init; }

    public required PlatformType Platform { get; init; }
}