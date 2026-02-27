using ODK.Core.Chapters;
using ODK.Core.Members;

namespace ODK.Data.Core.Members;

public class MemberChapterDto
{
    public required Chapter Chapter { get; init; }

    public required MemberChapter MemberChapter { get; init; }
}