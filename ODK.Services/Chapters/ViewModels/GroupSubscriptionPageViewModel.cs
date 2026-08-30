using ODK.Services.Members.ViewModels;

namespace ODK.Services.Chapters.ViewModels;

public class GroupSubscriptionPageViewModel : GroupPageViewModel
{
    /// <summary>
    /// Held rather than inherited: both types carry a chapter and a current member, and this one requires
    /// the member to be non-null where the group page allows a signed-out visitor.
    /// </summary>
    public required SubscriptionsPageViewModel Subscriptions { get; init; }
}
