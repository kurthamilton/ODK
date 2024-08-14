using ODK.Core.Chapters;

namespace ODK.Web.Razor.Models.Chapters;

public class ChapterQuestionsViewModel
{
    public required Chapter Chapter { get; set; }

    public bool IsAdmin { get; set; }

    public required IReadOnlyCollection<ChapterQuestion> Questions { get; init; }
}
