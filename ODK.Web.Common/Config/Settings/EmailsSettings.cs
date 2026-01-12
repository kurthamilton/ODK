namespace ODK.Web.Common.Config.Settings;

public class EmailsSettings
{
    public string? DebugEmailAddress { get; set; }

    public required EmailsThemeSettings Theme { get; init; }
}