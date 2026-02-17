using ODK.Core.Chapters;

namespace ODK.Data.Core.Members;

public class ChapterAdminMemberWithAvatarDto
{
    public required int? AvatarVersion { get; init; }

    public required ChapterAdminMember ChapterAdminMember { get; init; }
}
