using ODK.Core.Chapters;
using ODK.Core.Events;
using ODK.Core.Venues;

namespace ODK.Services.Events.ViewModels;

public class EventEditAdminPageViewModel : EventAdminPageViewModelBase
{
    public required IReadOnlyCollection<ChapterAdminMember> ChapterAdminMembers { get; init; }

    public required IReadOnlyCollection<EventHost> Hosts { get; init; }

    public required ChapterPaymentSettings PaymentSettings { get; init; }

    public required IReadOnlyCollection<Venue> Venues { get; init; }
}
