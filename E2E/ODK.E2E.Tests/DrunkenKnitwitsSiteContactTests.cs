using NUnit.Framework;
using ODK.E2E.Data.Models;
using ODK.E2E.Tests.Config;
using ODK.E2E.Tests.Helpers;

namespace ODK.E2E.Tests;

/// <summary>
/// Contacting the site on the <c>DrunkenKnitwits</c> platform, where a member's threads with the site get
/// a page of their own (<c>/conversations</c>) rather than a tab in an account area that is chapter-scoped.
/// All scenario bodies live in <see cref="SiteContactTestsBase"/>.
/// </summary>
[TestFixture]
[Category("DrunkenKnitwits")]
public class DrunkenKnitwitsSiteContactTests : SiteContactTestsBase
{
    protected override string PlatformBaseUrl => E2ESettings.DrunkenKnitwitsBaseUrl;

    private protected override string ConversationsPath => "/conversations";

    private protected override string PlatformName => "Drunken Knitwits";

    /// <summary>
    /// On DrunkenKnitwits an account belongs to a chapter - joining one IS the sign-up - so the member
    /// comes from a join. The chapter is pure context (nothing here mutates it), so it is the shared one;
    /// the member is fresh, because each test opens a thread of their own.
    /// </summary>
    private protected override async Task<TestAccount> NewMember()
    {
        var group = await SharedChapters.DrunkenKnitwits();
        return await Provisioning.JoinDrunkenKnitwitsChapterAsMember(group);
    }
}
