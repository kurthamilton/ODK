namespace ODK.Core.Members;

public class MemberProperty : IDatabaseEntity
{
    public Guid ChapterPropertyId { get; set; }

    public Guid Id { get; set; }

    public Guid MemberId { get; set; }

    public string? Value { get; set; }
}
