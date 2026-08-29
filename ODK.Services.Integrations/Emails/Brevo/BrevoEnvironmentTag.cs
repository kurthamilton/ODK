using ODK.Core.Platforms;

namespace ODK.Services.Integrations.Emails.Brevo;

/// <summary>
/// The environment tag's format, in one place, so a send and a received webhook cannot disagree about it.
/// </summary>
/// <remarks>
/// The prefix is configuration rather than a literal here because the repository is public. It has to be
/// separable from the environment: a receiver that knew only its own whole tag could not tell a tag belonging
/// to another deployment from no tag at all, and would have to discard both.
/// </remarks>
internal static class BrevoEnvironmentTag
{
    internal static string? Format(string prefix, EnvironmentType environment) =>
        !string.IsNullOrEmpty(prefix) && environment != EnvironmentType.None
            ? prefix + environment
            : null;

    /// <summary>
    /// The environment the tags name, or <see cref="EnvironmentType.None"/> where they name none - which
    /// includes an unconfigured prefix, since there is then nothing to recognise a tag by.
    /// </summary>
    internal static EnvironmentType Parse(string prefix, IEnumerable<string> tags)
    {
        if (string.IsNullOrEmpty(prefix))
        {
            return EnvironmentType.None;
        }

        foreach (var tag in tags)
        {
            if (!tag.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var name = tag[prefix.Length..];
            if (Enum.TryParse<EnvironmentType>(name, ignoreCase: true, out var environment) &&
                environment != EnvironmentType.None)
            {
                return environment;
            }
        }

        return EnvironmentType.None;
    }
}
