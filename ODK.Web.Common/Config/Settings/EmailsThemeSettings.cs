namespace ODK.Web.Common.Config.Settings;

public class EmailsThemeSettings
{
    public required ThemeSettings Header { get; init; }

    public required ThemeSettings Body { get; init; }
}