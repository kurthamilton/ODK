using System.Text.Json.Serialization;

namespace ODK.Services.Integrations.Emails.Models;

/// <summary>
/// The parts of Reoon's single-verification response the app acts on. The API returns considerably more
/// (per-check booleans, MX records, a score); only the overall verdict is read, so a change to the rest
/// can't break us.
/// </summary>
public class ReoonVerifyResponse
{
    /// <summary>
    /// Quick mode: valid, invalid, disposable, spamtrap.
    /// Power mode: safe, invalid, disabled, disposable, inbox_full, catch_all, role_account, spamtrap,
    /// unknown.
    /// Null when the body didn't carry one, which is treated as no answer rather than a rejection.
    /// </summary>
    [JsonPropertyName("status")]
    public string? Status { get; init; }
}
