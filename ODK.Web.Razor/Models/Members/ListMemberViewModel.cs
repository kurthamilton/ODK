using ODK.Core.Chapters;
using ODK.Core.Members;

namespace ODK.Web.Razor.Models.Members;

public class ListMemberViewModel
{
    public ListMemberViewModel()
    {
    }

    public ListMemberViewModel(Chapter chapter, Member member)
    {
        Chapter = chapter;
        Member = member;
    }

    public required Chapter Chapter { get; init; }

    public bool HideName => Size is "sm" or "xs";
    
    public int ImageHeight { get; set; }

    public int MaxWidth { get; set; }

    public required Member Member { get; init; }

    public string? Size { get; set; }
}
