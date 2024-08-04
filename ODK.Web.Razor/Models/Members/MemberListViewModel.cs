namespace ODK.Web.Razor.Models.Members;

public class MemberListViewModel
{
    public required string ChapterName { get; init; }

    public int Cols { get; set; }

    public int ImageHeight => Size == "xs" ? 50 : 150;

    public int MaxWidth => Size == "xs" ? 50 : 0;

    public required IReadOnlyCollection<ListMemberViewModel> Members { get; init; }

    public string? Size { get; set; }
}
