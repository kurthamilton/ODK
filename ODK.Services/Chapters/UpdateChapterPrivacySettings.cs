using ODK.Core.Chapters;

namespace ODK.Services.Chapters;

public class UpdateChapterPrivacySettings
{
    public ChapterFeatureVisibilityType? EventResponseVisibility { get; init; }

    public ChapterFeatureVisibilityType? EventVisibility { get; init; }

    public ChapterFeatureVisibilityType? MemberVisibility { get; init; }

    public ChapterFeatureVisibilityType? VenueVisibility { get; init; }
}
