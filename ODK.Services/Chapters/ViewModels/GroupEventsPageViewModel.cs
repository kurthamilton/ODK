namespace ODK.Services.Chapters.ViewModels;

public class GroupEventsPageViewModel : GroupPageViewModel
{
    public required IReadOnlyCollection<GroupPageListEventViewModel> Events { get; init; }

    public required int PastEventCount { get; init; }
}
