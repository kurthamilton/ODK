using ODK.Core.Members;

namespace ODK.Services.Events.ViewModels;

public class EventInvitesAdminPageViewModel : EventAdminPageViewModelBase
{
    public required Member CurrentMember { get; init; }

    public required EventInvitesDto Invites { get; init; }
}
