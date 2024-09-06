namespace ODK.Core.Chapters;

public class ChapterPrivacySettings : IChapterEntity
{
    public Guid ChapterId { get; set; }

    public ChapterFeatureVisibilityType? Conversations { get; set; }

    public ChapterFeatureVisibilityType? EventResponseVisibility { get; set; }

    public ChapterFeatureVisibilityType? EventVisibility { get; set; }

    public ChapterFeatureVisibilityType? MemberVisibility { get; set; }

    public ChapterFeatureVisibilityType? VenueVisibility { get; set; }
}
