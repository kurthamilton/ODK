using ODK.Core.Events;
using ODK.Core.Members;
using ODK.Data.Core.Members;

namespace ODK.Services.Events.ViewModels;

public class EventAttendeesAdminPageViewModel : EventAdminPageViewModelBase
{
    public required IReadOnlyCollection<MemberAvatarVersionDto> MemberAvatars { get; init; }

    public required IReadOnlyCollection<Member> Members { get; init; }

    public required IReadOnlyCollection<EventResponse> Responses { get; init; }

    public required IReadOnlyCollection<EventWaitlistMember> Waitlist { get; init; }
}