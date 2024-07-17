using ODK.Core.Chapters;
using ODK.Core.Members;

namespace ODK.Services.Chapters;

public class ChapterAdminMemberDto
{    
    public required ChapterAdminMember AdminMember { get; set; }

    public required Member Member { get; set; }
}
