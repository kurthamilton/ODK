using ODK.Core.Chapters;

namespace ODK.Services.Chapters.Models;

public class UpdateChapterPrivacySettings
{
    public ChapterFeatureVisibilityType? Conversations { get; init; }

    public ChapterFeatureVisibilityType? EventResponseVisibility { get; init; }

    public ChapterFeatureVisibilityType? EventVisibility { get; init; }

    public ChapterFeatureVisibilityType? MemberVisibility { get; init; }

    public ChapterFeatureVisibilityType? VenueVisibility { get; init; }
}
