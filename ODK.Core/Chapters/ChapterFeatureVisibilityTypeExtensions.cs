using ODK.Core.Extensions;

namespace ODK.Core.Chapters;

public static class ChapterFeatureVisibilityTypeExtensions
{
    private static IReadOnlyCollection<ChapterFeatureVisibilityType> Hierarchy =
    [
        ChapterFeatureVisibilityType.ActiveMembers,
        ChapterFeatureVisibilityType.AllMembers,
        ChapterFeatureVisibilityType.Public
    ];

    public static bool CanView(this ChapterFeatureVisibilityType feature, ChapterFeatureVisibilityType other)
    {
        var featureIndex = Hierarchy.IndexOf(feature);
        var otherIndex = Hierarchy.IndexOf(other);
        return featureIndex >= 0 && otherIndex >= 0 && featureIndex <= otherIndex;
    }

    public static bool IsMember(this ChapterFeatureVisibilityType feature)
    {
        switch (feature)
        {
            case ChapterFeatureVisibilityType.ActiveMembers:
            case ChapterFeatureVisibilityType.AllMembers:
                return true;
            default:
                return false;
        }
    }
}
