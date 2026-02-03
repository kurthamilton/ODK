using ODK.Core.Chapters;

namespace ODK.Services.Chapters.ViewModels;

public class ChapterTextsAdminPageViewModel
{
    public required Chapter Chapter { get; init; }

    public required ChapterTexts? Texts { get; init; }
}
