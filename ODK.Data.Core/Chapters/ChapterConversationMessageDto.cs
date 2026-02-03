using ODK.Core.Chapters;
using ODK.Core.Utils;

namespace ODK.Data.Core.Chapters;

public class ChapterConversationMessageDto
{    
    public required string MemberFirstName { get; init; }    

    public string MemberFullName => NameUtils.FullName(MemberFirstName, MemberLastName);

    public required string MemberLastName { get; init; }

    public required ChapterConversationMessage Message { get; init; }
}
