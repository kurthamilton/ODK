using NUnit.Framework;
using ODK.E2E.Data.Models;
using ODK.E2E.Tests.Config;
using ODK.E2E.Tests.Helpers;
using ODK.E2E.Tests.Pages;

namespace ODK.E2E.Tests;

/// <summary>
/// Account-management scenarios on the <c>Default</c> platform, where the account pages are global
/// (<c>/account/...</c>) so the member just needs a registered account. All scenario bodies live in
/// <see cref="AccountSettingsTestsBase"/>.
/// </summary>
[TestFixture]
[Category("Default")]
public class AccountSettingsTests : AccountSettingsTestsBase
{
    protected override string PlatformBaseUrl => E2ESettings.DefaultBaseUrl;

    protected override async Task<(TestAccount Member, AccountRoutes Routes)> ProvisionMember()
    {
        var member = await Provisioning.NewAccount("account-member");
        return (member, AccountRoutes.Default());
    }
}