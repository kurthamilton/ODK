using NUnit.Framework;
using ODK.E2E.Tests.Config;

namespace ODK.E2E.Tests.Helpers;

/// <summary>
/// The window an expired site subscription keeps its access for. Its length is the app's own configuration
/// (<c>Subscriptions:DefaultCooldownMonths</c>), which these tests cannot read, so it is stated again as
/// <see cref="E2ESettings.SiteSubscriptionCooldownMonths"/> and the two have to agree.
/// </summary>
internal static class SiteSubscriptionCooldownWindow
{
    /// <summary>
    /// An expiry just inside the window, or just beyond it. Inside is yesterday, which a cooldown of any
    /// length covers; beyond is a day before the window opened. Both are a day clear of the boundary, so the
    /// app resolving the window a moment later than this cannot land on the wrong side of it.
    /// </summary>
    public static DateTime LapsedAt(bool withinCooldown)
        => withinCooldown
            ? DateTime.UtcNow.AddDays(-1)
            : DateTime.UtcNow.AddMonths(-E2ESettings.SiteSubscriptionCooldownMonths).AddDays(-1);

    /// <summary>
    /// Skips a test that needs somewhere inside the window to arrange. Where the app runs with no cooldown
    /// there is nothing to assert rather than something failing: no cooldown is a valid way to run it, and
    /// what a lapsed subscription then means is what the without-the-feature cases already cover.
    /// </summary>
    public static void Require()
    {
        if (E2ESettings.SiteSubscriptionCooldownMonths <= 0)
        {
            Assert.Ignore(
                "The app under test is configured with no site subscription cooldown " +
                "(Subscriptions:DefaultCooldownMonths), so there is no window to be inside.");
        }
    }
}
