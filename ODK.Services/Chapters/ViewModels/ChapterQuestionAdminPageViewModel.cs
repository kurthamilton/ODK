using ODK.Core.Chapters;

namespace ODK.Services.Chapters.ViewModels;

public class ChapterQuestionAdminPageViewModel
{
    public required Chapter Chapter { get; init; }

    public required ChapterQuestion Question { get; init; }
}
