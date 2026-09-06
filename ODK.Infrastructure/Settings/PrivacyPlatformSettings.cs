namespace ODK.Infrastructure.Settings;

public class PrivacyPlatformSettings
{
    /// <summary>
    /// Where a member sends a data protection request. Per platform, because a member knows the site they
    /// joined rather than the trader behind it.
    /// </summary>
    public required string EmailAddress { get; init; }
}
