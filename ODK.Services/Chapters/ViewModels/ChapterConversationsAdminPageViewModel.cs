using ODK.Core.Chapters;
using ODK.Core.Messages;
using ODK.Core.Platforms;
using ODK.Data.Core.Chapters;

namespace ODK.Services.Chapters.ViewModels;

public class ChapterConversationsAdminPageViewModel
{
    public required Chapter Chapter { get; init; }

    public required IReadOnlyCollection<ChapterConversationDto> Conversations { get; init; }

    public required ChapterPrivacySettings? PrivacySettings { get; init; }

    public required MessageStatus Status { get; init; }
}
