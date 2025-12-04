using ODK.Core.Chapters;

namespace ODK.Services.Chapters.ViewModels;

public class GroupQuestionsPageViewModel : GroupPageViewModel
{
    public required IReadOnlyCollection<ChapterQuestion> Questions { get; init; }
}
