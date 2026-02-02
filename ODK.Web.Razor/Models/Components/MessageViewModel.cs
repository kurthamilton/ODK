namespace ODK.Web.Razor.Models.Components;

public class MessageViewModel
{
    public required DateTime CreatedUtc { get; init; }

    public required string MemberFullName { get; init; }

    public required Guid MemberId { get; init; }

    public required string Text { get; init; }
}
