namespace ODK.Core.Chapters;

public class ChapterPrivacySettings : IChapterEntity
{
    internal static readonly IReadOnlyDictionary<ChapterFeatureType, ChapterFeatureVisibilityType> Defaults =
        new Dictionary<ChapterFeatureType, ChapterFeatureVisibilityType>
        {
            { ChapterFeatureType.Conversations, ChapterFeatureVisibilityType.Public },
            { ChapterFeatureType.EventResponses, ChapterFeatureVisibilityType.AllMembers },
            { ChapterFeatureType.Events, ChapterFeatureVisibilityType.ActiveMembers },
            { ChapterFeatureType.Members, ChapterFeatureVisibilityType.ActiveMembers },
            { ChapterFeatureType.Venues, ChapterFeatureVisibilityType.ActiveMembers }
        }.AsReadOnly();

    public Guid ChapterId { get; set; }

    public ChapterFeatureVisibilityType? Conversations { get; set; }

    public ChapterFeatureVisibilityType? EventResponseVisibility { get; set; }

    public ChapterFeatureVisibilityType? EventVisibility { get; set; }

    public bool? InstagramFeed { get; set; }

    public ChapterFeatureVisibilityType? MemberVisibility { get; set; }

    public ChapterFeatureVisibilityType? VenueVisibility { get; set; }    
}
