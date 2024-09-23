using ODK.Core.Members;

namespace ODK.Web.Razor.Models.Components;

public class MessagesViewModel
{
    public required Member CurrentMember { get; init; }

    public required IReadOnlyCollection<MessageViewModel> Messages { get; init; }

    public required TimeZoneInfo? TimeZone { get; init; }
}
