namespace ODK.Data.Core.Members;

public class MemberAvatarVersionDto
{
    public required Guid MemberId { get; init; }

    public required int Version { get; init; }
}