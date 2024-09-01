using ODK.Services.Events.ViewModels;

namespace ODK.Services.Members.ViewModels;

public class MemberEventsAdminPageViewModel : MemberAdminPageViewModelBase
{
    public required IReadOnlyCollection<EventResponseViewModel> Responses { get; init; }
}
