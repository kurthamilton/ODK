namespace ODK.Services.Questions;

/// <summary>
/// The placeholders a site question's wording may use. Written in braces and replaced when the About page
/// renders, so an answer names the platform it is being read on rather than repeating a name that config
/// already holds.
/// </summary>
public static class SiteQuestionParameters
{
    /// <summary>
    /// The name of the platform the question belongs to.
    /// </summary>
    public const string PlatformName = "platform.name";

    /// <summary>
    /// The values to interpolate a question's wording with, for a question on the platform named by
    /// <paramref name="platformName"/>.
    /// </summary>
    /// <remarks>
    /// Matched case-insensitively: the wording is typed into a textarea by an admin, so <c>{Platform.Name}</c>
    /// has to resolve as readily as <c>{platform.name}</c> rather than reaching the page with its braces
    /// still in it.
    /// </remarks>
    public static IReadOnlyDictionary<string, string> ToDictionary(string platformName)
        => new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            [PlatformName] = platformName
        };
}
