using ODK.Core.Events;
using ODK.Data.Core.Members;

namespace ODK.Services.Events.ViewModels;

public class EventAttendeesAdminPageViewModel : EventAdminPageViewModelBase
{
    public required IReadOnlyCollection<MemberWithAvatarDto> Members { get; init; }

    public required IReadOnlyCollection<EventResponse> Responses { get; init; }

    public required IReadOnlyCollection<EventWaitlistMember> Waitlist { get; init; }
}