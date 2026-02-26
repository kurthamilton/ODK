using ODK.Core.Subscriptions;
using ODK.Data.Core.Chapters;

namespace ODK.Services.Members.ViewModels;

public class MemberConversationsAdminPageViewModel : MemberAdminPageViewModelBase
{
    public required IReadOnlyCollection<ChapterConversationDto> Conversations { get; init; }

    public required SiteSubscription? OwnerSubscription { get; init; }
}
