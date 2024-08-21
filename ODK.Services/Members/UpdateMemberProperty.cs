using ODK.Core.Members;

namespace ODK.Services.Members;

public class UpdateMemberProperty
{
    public Guid ChapterPropertyId { get; set; }

    public string Value { get; set; } = "";

    public MemberProperty ToMemberProperty(Guid memberId) => new MemberProperty
    {
        ChapterPropertyId = ChapterPropertyId,
        MemberId = memberId,
        Value = Value
    };
}
