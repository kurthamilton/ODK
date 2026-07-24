using Microsoft.Playwright;
using ODK.E2E.Core;
using ODK.E2E.Tests.Pages;

namespace ODK.E2E.Tests.Helpers;

/// <summary>
/// Hits app endpoints directly (not via the UI) for exercising actions the UI doesn't expose - e.g.
/// publishing a group that isn't approved, where no Publish button is rendered. Uses the logged-in
/// browser context's session cookies and an antiforgery token scraped from a rendered form (the app
/// validates antiforgery on all POSTs), sent via the configured <c>RequestVerificationToken</c> header.
/// </summary>
internal static class ApiRequests
{
    public static async Task<int> PostAsync(IPage loggedInPage, string path, string formPath)
    {
        await loggedInPage.Navigate(formPath);
        var token = await loggedInPage.GetAttributeAsync("input[name='__RequestVerificationToken']", "value")
            ?? throw new InvalidOperationException($"No antiforgery token found on '{formPath}'.");

        var response = await loggedInPage.Context.APIRequest.PostAsync(
            $"{E2ESettings.BaseUrl}{path}",
            new APIRequestContextOptions
            {
                Headers = new Dictionary<string, string> { ["RequestVerificationToken"] = token },
                MaxRedirects = 0
            });

        return response.Status;
    }
}