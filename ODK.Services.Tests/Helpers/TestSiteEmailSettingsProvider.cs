using System.Collections.Generic;
using ODK.Core.Emails;
using ODK.Core.Platforms;
using ODK.Services.Emails;

namespace ODK.Services.Tests.Helpers;

/// <summary>
/// The real provider over test settings, rather than a double: it does nothing but read the settings a
/// platform is configured with, so a fake would only restate the lookup.
/// </summary>
internal static class TestSiteEmailSettingsProvider
{
    internal const string AdminTitle = "Site admins";

    internal const string DrunkenKnitwitsFromEmailAddress = "noreply@drunkenknitwits.example.com";

    internal const string FromEmailAddress = "noreply@example.com";

    internal const string MemberTitle = "Site members";

    /// <param name="adminTitle">
    /// The admin title, for a test that turns on how the title itself resolves.
    /// </param>
    /// <param name="memberTitle">
    /// The member title, for a test that turns on how the title itself resolves.
    /// </param>
    internal static ISiteEmailSettingsProvider Create(
        string? adminTitle = null,
        string? memberTitle = null)
    {
        // A distinct address per platform, because that is what the real configuration holds and what lets a
        // test tell which platform an email was addressed as.
        SiteEmailSettings Settings(string fromEmailAddress) => new()
        {
            AdminTitle = adminTitle ?? AdminTitle,
            FromEmailAddress = fromEmailAddress,
            MemberTitle = memberTitle ?? MemberTitle
        };

        return new SiteEmailSettingsProvider(new SiteEmailSettingsProviderSettings
        {
            Platforms = new Dictionary<PlatformType, SiteEmailSettings>
            {
                { PlatformType.GroupSquirrel, Settings(FromEmailAddress) },
                { PlatformType.DrunkenKnitwits, Settings(DrunkenKnitwitsFromEmailAddress) }
            }
        });
    }
}
