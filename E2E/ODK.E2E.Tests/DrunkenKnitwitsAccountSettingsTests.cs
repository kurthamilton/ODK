using NUnit.Framework;
using ODK.E2E.Data.Models;
using ODK.E2E.Tests.Config;
using ODK.E2E.Tests.Helpers;
using ODK.E2E.Tests.Pages;

namespace ODK.E2E.Tests;

/// <summary>
/// Account-management scenarios on the <c>DrunkenKnitwits</c> platform, where the account pages are
/// chapter-scoped (<c>/{chapterName}/account/...</c>) - so the member belongs to a re-platformed chapter.
/// All scenario bodies live in <see cref="AccountSettingsTestsBase"/>.
/// </summary>
[TestFixture]
[Category("DrunkenKnitwits")]
public class DrunkenKnitwitsAccountSettingsTests : AccountSettingsTestsBase
{
    protected override string PlatformBaseUrl => E2ESettings.DrunkenKnitwitsBaseUrl;

    protected override async Task<(TestAccount Member, AccountRoutes Routes)> ProvisionMember()
    {
        // The chapter is pure context here (each test mutates only its own fresh member), so it's shared
        // and provisioned once; the member is local because account settings mutate it.
        var group = await SharedChapters.DrunkenKnitwits();
        var member = await Provisioning.JoinDrunkenKnitwitsChapterAsMember(group);
        return (member, AccountRoutes.DrunkenKnitwits(group.Name.ToLowerInvariant()));
    }
}