using ODK.Core.Members;

namespace ODK.Web.Razor.Models.Members;

public class MemberAvatarViewModel
{
    public MemberAvatar? Avatar { get; init; }

    public required string? ChapterName { get; init; }    

    public bool IsTop { get; init; }

    public int MaxWidth { get; init; }    

    public required Member Member { get; init; }    
}
