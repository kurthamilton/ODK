using ODK.Core.Chapters;
using ODK.Core.Messages;

namespace ODK.Services.Chapters.ViewModels;

public class ChapterMessagesAdminPageViewModel
{
    public required Chapter Chapter { get; init; }

    public required MessageStatus Status { get; init; }

    public required IReadOnlyCollection<ChapterContactMessage> Messages { get; init; }

    public required IReadOnlyDictionary<MessageStatus, int> StatusCounts { get; init; }
}