using ODK.Core.Members;

namespace ODK.Web.Razor.Models.Members;

public class ListMemberViewModel
{
    public required string ChapterName { get; init; }

    public bool HideName => Size is "sm" or "xs";

    public int MaxWidth { get; init; }

    public required Member Member { get; init; }

    public string? Size { get; set; }
}
