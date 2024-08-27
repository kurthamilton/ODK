namespace ODK.Services.Chapters.ViewModels;

public class GroupEventsPageViewModel : GroupPageViewModelBase
{
    public required IReadOnlyCollection<GroupPageListEventViewModel> PastEvents { get; init; }

    public required IReadOnlyCollection<GroupPageListEventViewModel> UpcomingEvents { get; init; }    
}
