namespace ODK.E2E.Tests.Helpers;

/// <summary>
/// Paths to files copied to the test output directory and used to drive file uploads (e.g. the
/// required group picture on the create-group wizard).
/// </summary>
internal static class TestAssets
{
    public static string GroupImagePath => Path.Combine(AppContext.BaseDirectory, "Assets", "group-image.png");
}