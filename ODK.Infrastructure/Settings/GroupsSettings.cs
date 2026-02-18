namespace ODK.Infrastructure.Settings;

public class GroupsSettings
{
    public required string DefaultCountryCode { get; init; }

    public required string[] ReservedSlugs { get; init; }
}