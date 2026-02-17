using ODK.Core.Members;

namespace ODK.Data.Core.Members;

public class MemberWithImageDto
{
    public required int? ImageVersion { get; init; }

    public required Member Member { get; init; }
}
