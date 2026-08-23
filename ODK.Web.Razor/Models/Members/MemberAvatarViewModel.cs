using ODK.Core.Members;

namespace ODK.Web.Razor.Models.Members;

public class MemberAvatarViewModel
{
    public bool IsTop { get; init; }

    public int MaxWidth { get; init; }

    public required Member Member { get; init; }

    /// <summary>
    /// Whether a member with no avatar renders the placeholder rather than nothing. For a layout that
    /// expects an image in every slot, such as a row of cards.
    /// </summary>
    public bool ShowPlaceholder { get; init; }

    public required int? Version { get; init; }

    public int? Width { get; init; }
}