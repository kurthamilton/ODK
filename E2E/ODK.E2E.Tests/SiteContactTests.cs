using NUnit.Framework;
using ODK.E2E.Data.Models;
using ODK.E2E.Tests.Config;
using ODK.E2E.Tests.Helpers;

namespace ODK.E2E.Tests;

/// <summary>
/// Contacting the site on the <c>Default</c> platform, where a member's threads with the site sit in
/// their account area. All scenario bodies live in <see cref="SiteContactTestsBase"/>.
/// </summary>
[TestFixture]
[Category("Default")]
public class SiteContactTests : SiteContactTestsBase
{
    protected override string PlatformBaseUrl => E2ESettings.DefaultBaseUrl;

    private protected override string ConversationsPath => "/account/site-conversations";

    private protected override string PlatformName => "Group Squirrel";

    // A plain account, with no group behind it: contacting the site belongs to no chapter, and Group
    // Squirrel signs a member up without one.
    private protected override Task<TestAccount> NewMember() => Provisioning.NewAccount("site-contact-member");
}
