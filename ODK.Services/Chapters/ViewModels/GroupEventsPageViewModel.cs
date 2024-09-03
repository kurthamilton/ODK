namespace ODK.Services.Chapters.ViewModels;

public class GroupEventsPageViewModel : GroupPageViewModelBase
{
    public required IReadOnlyCollection<GroupPageListEventViewModel> Events { get; init; }
}
