using ODK.Core.Messages;

namespace ODK.Services.Contact.ViewModels;

public class MessagesAdminPageViewModel
{
    public required IReadOnlyCollection<SiteContactMessage> Messages { get; init; }

    public required MessageStatus Status { get; init; }

    public required IReadOnlyDictionary<MessageStatus, int> StatusCounts { get; init; }

    /// <summary>The viewing site admin's zone: a site-wide page has no chapter to fall back to.</summary>
    public required TimeZoneInfo TimeZone { get; init; }
}