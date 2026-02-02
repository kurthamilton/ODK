using ODK.Core.Members;

namespace ODK.Services.Members.Models;

public class MemberPropertyUpdateModel
{
    public Guid ChapterPropertyId { get; set; }

    public string Value { get; set; } = string.Empty;

    public MemberProperty ToMemberProperty(Guid memberId) => new MemberProperty
    {
        ChapterPropertyId = ChapterPropertyId,
        MemberId = memberId,
        Value = Value
    };
}
