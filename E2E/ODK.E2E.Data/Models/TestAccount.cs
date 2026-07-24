namespace ODK.E2E.Data.Models;

/// <summary>
/// A provisioned test account. The password is retained from creation (it can't be recovered from the
/// hashed DB value or guessed from the random email), so later tests can log in as this account.
/// </summary>
public sealed record TestAccount(string Role, string Email, string Password);