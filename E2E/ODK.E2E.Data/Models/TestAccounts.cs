namespace ODK.E2E.Data.Models;

/// <summary>
/// Test accounts use a dedicated email domain so the post-run cleanup can identify and remove exactly
/// the data the E2E tests created, and never touch real members.
/// </summary>
public static class TestAccounts
{
    public const string EmailDomain = "e2e.odk.test";

    /// <summary>
    /// A fixed, policy-compliant password (the policy only enforces a minimum length) used for
    /// provisioned/shared accounts. The e2e environment disables the breach check
    /// (<c>Hibp:Enabled=false</c>), so a known constant is safe - and being known is the whole point:
    /// a later test can log in as an account an earlier step created.
    /// </summary>
    public const string Password = "E2eTestPassword!";

    public static string NewEmailAddress() => NewEmailAddress(null);

    public static string NewEmailAddress(string? role)
    {
        // Tag the local part with the role (when given) so DB rows and failures are legible; the guid
        // keeps it unique across runs, and the domain suffix still drives cleanup.
        var prefix = string.IsNullOrEmpty(role) ? "e2e" : $"e2e-{Slug(role)}";
        return $"{prefix}-{Guid.NewGuid():N}@{EmailDomain}";
    }

    private static string Slug(string value)
    {
        var chars = value.ToLowerInvariant()
            .Select(c => char.IsLetterOrDigit(c) ? c : '-')
            .ToArray();
        return new string(chars).Trim('-');
    }
}