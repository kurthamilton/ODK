using ODK.Core.Members;
using ODK.Core.Platforms;

namespace ODK.Services.Users.ViewModels;

public class MemberEmailPreferencesPageViewModel
{
    public required Member CurrentMember { get; init; }

    public required PlatformType Platform { get; init; }

    public required IReadOnlyCollection<MemberEmailPreference> Preferences { get; init; }
}
