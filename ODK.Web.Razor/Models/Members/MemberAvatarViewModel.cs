using ODK.Core.Members;
using ODK.Core.Platforms;

namespace ODK.Web.Razor.Models.Members;

public class MemberAvatarViewModel
{
    public MemberAvatar? Avatar { get; init; }

    public bool IsTop { get; init; }

    public int MaxWidth { get; init; }    

    public required Member Member { get; init; }    
}
