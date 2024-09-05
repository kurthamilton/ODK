using ODK.Core.Chapters;

namespace ODK.Services.Members.ViewModels;

public class MemberConversationsAdminPageViewModel : MemberAdminPageViewModelBase
{
    public required IReadOnlyCollection<ChapterConversationDto> Conversations { get; init; }
}
