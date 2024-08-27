using ODK.Core.Events;
using ODK.Core.Members;

namespace ODK.Services.Events.ViewModels;

public class EventAttendeesAdminPageViewModel : EventAdminPageViewModelBase
{
    public required IReadOnlyCollection<Member> Members { get; init; }

    public required IReadOnlyCollection<EventResponse> Responses { get; init; }
}
