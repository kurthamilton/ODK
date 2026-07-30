using ODK.Core.Platforms;

namespace ODK.Services.Subscriptions.ViewModels;

public class SiteAdminMembersViewModel
{
    public required PlatformType Platform { get; init; }

    public required IReadOnlyCollection<SiteAdminMemberRowViewModel> Rows { get; init; }
}
