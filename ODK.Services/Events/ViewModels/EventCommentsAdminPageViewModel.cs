using ODK.Data.Core.Events;

namespace ODK.Services.Events.ViewModels;

public class EventCommentsAdminPageViewModel : EventAdminPageViewModelBase
{
    public required IReadOnlyCollection<EventCommentDto> Comments { get; init; }
}