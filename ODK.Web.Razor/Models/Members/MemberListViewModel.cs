using ODK.Core.Chapters;
using ODK.Core.Platforms;

namespace ODK.Web.Razor.Models.Members;

public class MemberListViewModel
{
    public required Chapter Chapter { get; init; }

    public int Cols { get; set; }

    public int ImageHeight => Size == "xs" ? 50 : 150;

    public int MaxWidth => Size == "xs" ? 50 : 0;

    public required IReadOnlyCollection<ListMemberViewModel> Members { get; init; }

    public required PlatformType Platform { get; init; }

    public string? Size { get; set; }
}
