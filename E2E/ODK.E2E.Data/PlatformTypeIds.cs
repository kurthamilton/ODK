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
    /// The platform's name as the app's enum spells it, for a config key that names one platform's values.
    /// </summary>
    public static string Name(int platformTypeId) => platformTypeId switch
    {
        Default => nameof(Default),
        DrunkenKnitwits => nameof(DrunkenKnitwits),
        _ => throw new ArgumentOutOfRangeException(
            nameof(platformTypeId), platformTypeId, "Unknown platform")
    };
}
