namespace ODK.Infrastructure.Settings;

public class GroupsSettings
{
    public required GroupsDashboardSettings Dashboard { get; init; }

    public required string DefaultCountryCode { get; init; }

    public required int MigrationWindowDays { get; init; }

    public required string[] ReservedSlugs { get; init; }
}