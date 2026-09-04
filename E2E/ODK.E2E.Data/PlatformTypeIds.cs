namespace ODK.E2E.Data;

/// <summary>
/// The app's <c>PlatformType</c> numbers, which are what its <c>PlatformTypeId</c> columns store. Repeated
/// here rather than referenced because these tests deliberately do not depend on the app's projects; the
/// numbers are safe to repeat because they are a persisted contract, which is why that enum assigns them
/// explicitly rather than letting them fall where they may.
/// </summary>
public static class PlatformTypeIds
{
    public const int Default = 1;

    public const int DrunkenKnitwits = 2;

    /// <summary>
    /// The label a config key names one platform's values by, as the app's own configuration labels it.
    /// The numbers above are the app's enum; these are what its <c>appsettings.json</c> is keyed by.
    /// </summary>
    public static string Key(int platformTypeId) => platformTypeId switch
    {
        Default => "GS",
        DrunkenKnitwits => "DK",
        _ => throw new ArgumentOutOfRangeException(
            nameof(platformTypeId), platformTypeId, "Unknown platform")
    };
}
