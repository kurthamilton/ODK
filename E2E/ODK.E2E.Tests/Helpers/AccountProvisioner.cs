using Microsoft.Playwright;
using ODK.E2E.Data;
using ODK.E2E.Tests.Pages;

namespace ODK.E2E.Tests.Helpers;

/// <summary>
/// The reusable "create a usable account" journey: sign up, read the activation token from the DB, and
/// activate (which sets the password). Shared by <c>AccountFlowTests</c> (the full end-to-end journey)
/// and <c>SharedAccounts</c> (provisioning reusable accounts for other tests) so there is one path.
/// </summary>
internal static class AccountProvisioner
{
    public static async Task RegisterAndActivateAsync(
        IPage page,
        string email,
        string password,
        string firstName = "E2E",
        string lastName = "Test")
    {
        await new SignUpPage(page).SignUpAsync(firstName, lastName, email);

        var token = await ActivationTokenDataHelper.GetActivationToken(email);

        await new ActivatePage(page).ActivateAsync(token, password);
    }
}