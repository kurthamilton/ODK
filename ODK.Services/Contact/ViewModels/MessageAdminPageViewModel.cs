using ODK.Core.Members;
using ODK.Core.Messages;

namespace ODK.Services.Contact.ViewModels;

public class MessageAdminPageViewModel
{
    public required Member CurrentMember { get; init; }

    public required SiteContactMessage Message { get; init; }

    public required IReadOnlyCollection<SiteContactMessageReply> Replies { get; init; }
}
