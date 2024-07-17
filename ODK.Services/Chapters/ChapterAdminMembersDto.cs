using ODK.Core.Chapters;
using ODK.Core.Members;

namespace ODK.Services.Chapters;

public class ChapterAdminMembersDto
{    
    public required IReadOnlyCollection<ChapterAdminMember> AdminMembers { get; set; }

    public required IReadOnlyCollection<Member> Members { get; set; }
}
