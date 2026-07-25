using NUnit.Framework;
using ODK.E2E.Data.Models;
using ODK.E2E.Tests.Config;
using ODK.E2E.Tests.Helpers;
using ODK.E2E.Tests.Pages;

namespace ODK.E2E.Tests;

/// <summary>
/// Event-admin and RSVP scenarios on the <c>Default</c> platform. The owner self-serves a published
/// group; a member joins it through the UI. All scenario bodies live in <see cref="EventTestsBase"/>.
/// </summary>
[TestFixture]
[Category("Default")]
public class EventTests : EventTestsBase
{
    protected override string PlatformBaseUrl => E2ESettings.DefaultBaseUrl;

    private protected override Task<TestAccount> ProvisionMember(TestGroup group)
        => Provisioning.JoinGroupAsMember(group);

    private protected override async Task<(TestAccount Owner, TestGroup Group)> ProvisionOwnerChapter(string name)
    {
        var owner = await Provisioning.NewAccount(SharedAccounts.GroupOwner);
        var group = await Provisioning.CreatePublishedGroup(owner, name);
        return (owner, group);
    }

    private protected override PlatformRoutes RoutesFor(TestGroup group) => PlatformRoutes.Default(group);
}
