using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Core.Platforms;

namespace ODK.Services.Members.ViewModels;

public class MemberConversationsPageViewModel
{
    public required IReadOnlyCollection<Chapter> Chapters { get; init; }

    public required IReadOnlyCollection<ChapterConversationDto> Conversations { get; init; }

    public required Member CurrentMember { get; init; }

    public required PlatformType Platform { get; init; }
}
