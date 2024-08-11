using ODK.Core.Platforms;

namespace ODK.Core.Chapters;

public class Chapter : IDatabaseEntity, ITimeZoneEntity
{
    public const string DefaultTimeZoneId = "GMT Standard Time";

    public DateTime? ApprovedUtc { get; set; }

    public string? BannerImageUrl { get; set; }

    public Guid? CountryId { get; set; }

    public DateTime CreatedUtc { get; set; }

    public string? Description { get; set; }

    public int? DisplayOrder { get; set; }

    public Guid Id { get; set; }

    public string Name { get; set; } = "";

    public Guid? OwnerId { get; set; }

    public PlatformType Platform { get; set; }

    public DateTime? PublishedUtc { get; set; }

    public string? RedirectUrl { get; set; }

    public string Slug { get; set; } = "";

    public TimeZoneInfo? TimeZone { get; set; }

    public bool CanBePublished() => ApprovedUtc != null && PublishedUtc == null;

    public bool IsOpenForRegistration() => ApprovedUtc != null && PublishedUtc != null;

    public DateTime ToChapterTime(DateTime utc) => TimeZone != null
        ? TimeZoneInfo.ConvertTimeFromUtc(utc, TimeZone)
        : utc;

    public DateTime? ToChapterTime(DateTime? utc) => utc != null ? ToChapterTime(utc.Value) : null;
}
