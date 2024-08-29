using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Core.Platforms;
using ODK.Services.Events.ViewModels;

namespace ODK.Services.Members.ViewModels;

public class MemberEventsAdminPageViewModel
{
    public required Chapter Chapter { get; init; }    

    public required Member Member { get; init; }

    public required PlatformType Platform { get; init; }

    public required IReadOnlyCollection<EventResponseViewModel> Responses { get; init; }
}
