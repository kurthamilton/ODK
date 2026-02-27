using ODK.Core.Chapters;

namespace ODK.Data.Core.Chapters;

public class ChapterAdminMemberDto
{
    public required Chapter Chapter { get; init; }

    public required ChapterAdminMember ChapterAdminMember { get; init; }
}