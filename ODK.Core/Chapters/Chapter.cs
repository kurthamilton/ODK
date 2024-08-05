using ODK.Core.Extensions;

namespace ODK.Core.Chapters;

public class Chapter : IDatabaseEntity, ITimeZoneEntity
{
    public string BannerImageUrl { get; } = "";

    public Guid CountryId { get; set; }

    public int DisplayOrder { get; set; }

    public Guid Id { get; set; }

    public string Name { get; set; } = "";

    public Guid? OwnerId { get; set; }

    public string? RedirectUrl { get; set; }

    public TimeZoneInfo? TimeZone { get; set; }

    public DateTime ToChapterTime(DateTime utc) => TimeZone != null
        ? TimeZoneInfo.ConvertTimeFromUtc(utc, TimeZone)
        : utc;

    public DateTime? ToChapterTime(DateTime? utc) => utc != null ? ToChapterTime(utc.Value) : null;
}
