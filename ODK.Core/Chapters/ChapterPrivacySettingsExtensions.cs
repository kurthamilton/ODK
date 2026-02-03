namespace ODK.Core.Chapters;

public static class ChapterPrivacySettingsExtensions
{
    public static ChapterFeatureVisibilityType Visibility(
        this ChapterPrivacySettings? settings, ChapterFeatureType feature)
    {
        var visibility = feature switch
        {
            ChapterFeatureType.Conversations => settings?.Conversations,
            ChapterFeatureType.EventResponses => settings?.EventResponseVisibility,
            ChapterFeatureType.Events => settings?.EventVisibility,
            ChapterFeatureType.Members => settings?.MemberVisibility,
            ChapterFeatureType.Venues => settings?.VenueVisibility,
            _ => null
        };

        if (visibility != null)
        {
            return visibility.Value;
        }

        return ChapterPrivacySettings.Defaults.TryGetValue(feature, out var @default)
            ? @default
            : default;
    }
}
