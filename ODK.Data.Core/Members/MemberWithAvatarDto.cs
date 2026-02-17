using ODK.Core.Members;

namespace ODK.Data.Core.Members;

public class MemberWithAvatarDto
{
    public required int? AvatarVersion { get; init; }

    public required Member Member { get; init; }
}