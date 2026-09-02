using ODK.Core.Messages;

namespace ODK.Services.Contact.ViewModels;

public class MessageAdminPageViewModel
{
    public required SiteContactMessage Message { get; init; }

    public required IReadOnlyCollection<SiteContactMessageReply> Replies { get; init; }

    /// <summary>The viewing site admin's zone: a site-wide page has no chapter to fall back to.</summary>
    public required TimeZoneInfo TimeZone { get; init; }
}
