using ODK.Core.Members;
using ODK.Core.Messages;

namespace ODK.Services.Contact.ViewModels;

public class MessagesAdminPageViewModel
{
    public required Member CurrentMember { get; init; }

    public required IReadOnlyCollection<SiteContactMessage> Messages { get; init; }
}
