using NUnit.Framework;
using ODK.E2E.Data.Models;
using ODK.E2E.Tests.Config;
using ODK.E2E.Tests.Helpers;
using ODK.E2E.Tests.Pages;

namespace ODK.E2E.Tests;

/// <summary>
/// Event-admin and RSVP scenarios on the <c>DrunkenKnitwits</c> platform. DrunkenKnitwits has no
/// self-service group creation, so the chapter is created through the Default UI then re-platformed;
/// a member joins by signing up against the chapter. All scenario bodies live in
/// <see cref="EventTestsBase"/>.
/// </summary>
[TestFixture]
[Category("DrunkenKnitwits")]
public class DrunkenKnitwitsEventTests : EventTestsBase
{
    protected override string PlatformBaseUrl => E2ESettings.DrunkenKnitwitsBaseUrl;

    private protected override Task<TestAccount> ProvisionMember(TestGroup group)
        => Provisioning.JoinDrunkenKnitwitsChapterAsMember(group);

    private protected override async Task<(TestAccount Owner, TestGroup Group)> ProvisionOwnerChapter(string name)
    {
        var owner = await Provisioning.NewAccount(SharedAccounts.GroupOwner);
        var group = await Provisioning.SeedDrunkenKnitwitsChapter(owner, name);
        return (owner, group);
    }

    private protected override PlatformRoutes RoutesFor(TestGroup group) => PlatformRoutes.DrunkenKnitwits(group);
}
