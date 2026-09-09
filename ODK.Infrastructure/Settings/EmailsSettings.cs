namespace ODK.Infrastructure.Settings;

public class EmailsSettings
{
    /// <summary>The client this environment sends through.</summary>
    public required EmailClientType Client { get; init; }

    public string? DebugEmailAddress { get; set; }

    public required Dictionary<PlatformKey, EmailsPlatformSettings> Platforms { get; init; }

    /// <summary>
    /// Where <see cref="EmailClientType.Smtp"/> delivers. Stated by the environment that runs a mail sink;
    /// every other one leaves it unset.
    /// </summary>
    public required EmailsSmtpSettings Smtp { get; init; }

    public required EmailsThemeSettings Theme { get; init; }
}