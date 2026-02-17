namespace ODK.Data.Core.Members;

public class MemberImageVersionDto
{
    public required Guid MemberId { get; init; }

    public required int Version { get; init; }
}