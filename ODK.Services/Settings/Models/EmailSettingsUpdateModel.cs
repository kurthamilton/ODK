namespace ODK.Services.Settings.Models;

public class EmailSettingsUpdateModel
{
    public required string ContactEmailAddress { get; init; }
    public required string Title { get; init; }
    public required string FromEmailAddress { get; init; }
    public required string FromEmailName { get; init; }
    public required string PlatformTitle { get; init; }
}