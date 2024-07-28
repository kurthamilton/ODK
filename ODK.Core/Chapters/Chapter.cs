namespace ODK.Core.Chapters;

public class Chapter : IDatabaseEntity
{
    public string BannerImageUrl { get; } = "";

    public Guid CountryId { get; set; }

    public DateTime CurrentTime => ToChapterTime(DateTime.UtcNow);

    public int DisplayOrder { get; set; }

    public Guid Id { get; set; }

    public string Name { get; set; } = "";

    public string? RedirectUrl { get; set; }

    public TimeZoneInfo? TimeZone { get; set; }

    public DateTime ToChapterTime(DateTime utc) => TimeZone != null
        ? TimeZoneInfo.ConvertTimeFromUtc(utc, TimeZone)
        : utc;
}
