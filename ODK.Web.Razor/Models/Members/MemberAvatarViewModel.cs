using ODK.Core.Members;

namespace ODK.Web.Razor.Models.Members;

public class MemberAvatarViewModel
{
    public bool IsTop { get; init; }

    public int MaxWidth { get; init; }

    public required Member Member { get; init; }

    public required int? Version { get; init; }

    public int? Width { get; init; }
}