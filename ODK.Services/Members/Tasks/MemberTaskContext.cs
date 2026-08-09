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

    /// <summary>
    /// Ids of the member's owned chapters that already have an image, so a provider can tell "no image"
    /// from "not owned" without querying per chapter.
    /// </summary>
    public required IReadOnlyCollection<Guid> ChaptersWithImage { get; init; }

    public required bool HasAvatar { get; init; }

    public required Member Member { get; init; }

    public required IReadOnlyCollection<MemberProperty> MemberProperties { get; init; }

    /// <summary>The chapters the member owns, which is what the group-owner tasks are computed from.</summary>
    public required IReadOnlyCollection<Chapter> OwnedChapters { get; init; }

    public required PlatformType Platform { get; init; }
}
