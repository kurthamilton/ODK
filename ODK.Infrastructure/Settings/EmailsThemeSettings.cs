namespace ODK.Infrastructure.Settings;

public class EmailsThemeSettings
{
    public required ThemeSettings Header { get; init; }

    public required ThemeSettings Body { get; init; }
}