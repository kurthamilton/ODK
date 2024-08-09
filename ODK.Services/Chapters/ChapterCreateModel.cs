namespace ODK.Services.Chapters;

public class ChapterCreateModel
{
    public required Guid? CountryId { get; set; }

    public required string Description { get; set; }

    public required string Name { get; set; }

    public required string? TimeZoneId { get; set; }
}
