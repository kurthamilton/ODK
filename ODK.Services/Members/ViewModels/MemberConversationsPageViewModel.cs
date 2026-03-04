using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Core.Platforms;
using ODK.Data.Core.Chapters;

namespace ODK.Services.Members.ViewModels;

public class MemberConversationsPageViewModel
{
    public required int ActiveConversationCount { get; init; }

    public required bool Archived { get; init; }

    public required int ArchivedConversationCount { get; init; }

    public required Chapter? Chapter { get; init; }

    public required IReadOnlyCollection<Chapter> Chapters { get; init; }

    public required IReadOnlyCollection<ChapterConversationDto> Conversations { get; init; }

    public required Member CurrentMember { get; init; }

    public required PlatformType Platform { get; init; }

    public int TotalConversationCount => ActiveConversationCount + ArchivedConversationCount;
}