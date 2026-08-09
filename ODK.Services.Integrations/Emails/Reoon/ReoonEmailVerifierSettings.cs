namespace ODK.Services.Integrations.Emails.Reoon;

public class ReoonEmailVerifierSettings
{
    public required string ApiKey { get; init; }

    /// <summary>
    /// "quick" or "power". Power adds an SMTP connection and mailbox check, so it catches a well-formed
    /// address at a real domain with no such mailbox. Both consume a credit, so quick buys speed rather
    /// than quota.
    /// </summary>
    public required string Mode { get; init; }

    public required string VerifyUrl { get; init; }
}
