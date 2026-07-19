using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Core.Platforms;

namespace ODK.Services.Members.Tasks;

/// <summary>
/// The member state a set of <see cref="IMemberTaskProvider"/>s inspect, loaded once by
/// <see cref="MemberTaskService"/> so providers don't issue their own queries.
/// </summary>
public class MemberTaskContext
{
    public required IReadOnlyCollection<Chapter> Chapters { get; init; }

    public required IReadOnlyCollection<ChapterProperty> ChapterProperties { get; init; }

    public required bool HasAvatar { get; init; }

    public required Member Member { get; init; }

    public required IReadOnlyCollection<MemberProperty> MemberProperties { get; init; }

    public required PlatformType Platform { get; init; }
}
