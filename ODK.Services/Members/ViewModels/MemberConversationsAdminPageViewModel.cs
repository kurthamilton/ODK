using ODK.Core.Chapters;
using ODK.Core.Members;

namespace ODK.Services.Members.ViewModels;

public class MemberConversationsAdminPageViewModel : MemberAdminPageViewModelBase
{
    public required IReadOnlyCollection<ChapterConversationDto> Conversations { get; init; }

    public required MemberSiteSubscription? OwnerSubscription { get; init; }
}
