using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Core.Platforms;
using ODK.Core.Venues;

namespace ODK.Services.Events.ViewModels;

public class EventCreateAdminPageViewModel
{
    public required IReadOnlyCollection<ChapterAdminMember> AdminMembers { get; init; }

    public required Chapter Chapter { get; init; }

    public required DateTime Date { get; init; }

    public required ChapterEventSettings? EventSettings { get; init; }

    public required MemberSiteSubscription? OwnerSubscription { get; init; }

    public required ChapterPaymentSettings? PaymentSettings { get; init; }

    public required PlatformType Platform { get; init; }

    public required IReadOnlyCollection<Venue> Venues { get; init; }
}
