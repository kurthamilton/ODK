using ODK.Core.Chapters;

namespace ODK.Services.Chapters.ViewModels;

public class ChapterMessageAdminPageViewModel
{
    public required Chapter Chapter { get; init; }

    public required ChapterContactMessage Message { get; init; }

    public required IReadOnlyCollection<ChapterContactMessageReply> Replies { get; init; }
}
