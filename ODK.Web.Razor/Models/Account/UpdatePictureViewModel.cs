using ODK.Core.Members;

namespace ODK.Web.Razor.Models.Account;

public class UpdatePictureViewModel
{
    public required MemberAvatar? Avatar { get; init; }

    public required string? ChapterName { get; init; }    

    public required MemberImage? Image { get; init; }

    public required Member Member { get; init; }
}
