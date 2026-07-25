using NUnit.Framework;
using ODK.E2E.Data.Models;
using ODK.E2E.Tests.Config;
using ODK.E2E.Tests.Helpers;
using ODK.E2E.Tests.Pages;

namespace ODK.E2E.Tests;

/// <summary>
/// Chapter member-profile-property scenarios on the <c>Default</c> platform. The owner self-serves a
/// published group; members join it through the group join form. All scenario bodies live in
/// <see cref="MemberProfileTestsBase"/>.
/// </summary>
[TestFixture]
[Category("Default")]
public class MemberProfileTests : MemberProfileTestsBase
{
    protected override string PlatformBaseUrl => E2ESettings.DefaultBaseUrl;

    private protected override Task<TestAccount> JoinChapterWithProperties(
        TestGroup group, IReadOnlyDictionary<Guid, string> answers)
        => Provisioning.JoinGroupMemberWithProperties(group, answers);

    private protected override async Task<(TestAccount Owner, TestGroup Group)> ProvisionOwnerChapter(string name)
    {
        var owner = await Provisioning.NewAccount(SharedAccounts.GroupOwner);
        var group = await Provisioning.CreatePublishedGroup(owner, name);
        return (owner, group);
    }

    private protected override PlatformRoutes RoutesFor(TestGroup group) => PlatformRoutes.Default(group);

    private protected override Task<bool> TryJoinChapterWithoutRequired(
        TestGroup group, IReadOnlyDictionary<Guid, string> answers)
        => Provisioning.TryJoinGroupWithoutRequired(group, answers);
}
