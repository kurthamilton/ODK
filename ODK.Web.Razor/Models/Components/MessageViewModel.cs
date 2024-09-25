using ODK.Core.Members;

namespace ODK.Web.Razor.Models.Components;

public class MessageViewModel
{
    public required DateTime CreatedUtc { get; init; }

    public required Member? Member { get; init; }

    public required string Text { get; init; }
}
