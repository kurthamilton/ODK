using NUnit.Framework;
using ODK.E2E.Data.Models;
using ODK.E2E.Tests.Config;
using ODK.E2E.Tests.Helpers;
using ODK.E2E.Tests.Pages;

namespace ODK.E2E.Tests;

/// <summary>
/// Chapter member-profile-property scenarios on the <c>DrunkenKnitwits</c> platform. The chapter is
/// created through the Default UI then re-platformed; members join by signing up against the chapter
/// (answering the profile questions as part of sign-up). All scenario bodies live in
/// <see cref="MemberProfileTestsBase"/>.
/// </summary>
[TestFixture]
[Category("DrunkenKnitwits")]
public class DrunkenKnitwitsMemberProfileTests : MemberProfileTestsBase
{
    protected override string PlatformBaseUrl => E2ESettings.DrunkenKnitwitsBaseUrl;

    private protected override Task<TestAccount> JoinChapterWithProperties(
        TestGroup group, IReadOnlyDictionary<Guid, string> answers)
        => Provisioning.JoinDrunkenKnitwitsMemberWithProperties(group, answers);

    private protected override async Task<(TestAccount Owner, TestGroup Group)> ProvisionOwnerChapter(string name)
    {
        var owner = await Provisioning.NewAccount(SharedAccounts.GroupOwner);
        var group = await Provisioning.SeedDrunkenKnitwitsChapter(owner, name);
        return (owner, group);
    }

    private protected override PlatformRoutes RoutesFor(TestGroup group) => PlatformRoutes.DrunkenKnitwits(group);

    private protected override Task<bool> TryJoinChapterWithoutRequired(
        TestGroup group, IReadOnlyDictionary<Guid, string> answers)
        => Provisioning.TryJoinDrunkenKnitwitsWithoutRequired(group, answers);
}
