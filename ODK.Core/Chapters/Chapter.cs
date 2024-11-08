using ODK.Core.Platforms;

namespace ODK.Core.Chapters;

public class Chapter : IDatabaseEntity, ITimeZoneEntity, ICloneable<Chapter>
{
    public const string DefaultTimeZoneId = "GMT Standard Time";

    public const string DrunkenKnitwitsSuffix = " Drunken Knitwits";

    public DateTime? ApprovedUtc { get; set; }

    public string? BannerImageUrl { get; set; }

    public Guid? CountryId { get; set; }

    public DateTime CreatedUtc { get; set; }    

    public int? DisplayOrder { get; set; }

    public Guid Id { get; set; }

    public string Name { get; set; } = "";

    public Guid? OwnerId { get; set; }

    public PlatformType Platform { get; set; }

    public DateTime? PublishedUtc { get; set; }

    public string? RedirectUrl { get; set; }

    public string Slug { get; set; } = "";

    public TimeZoneInfo? TimeZone { get; set; }

    public bool Approved() => ApprovedUtc != null;

    public bool CanBeDeleted(int memberCount) => memberCount == 1;

    public bool CanBePublished() => Approved() && !Published();

    public Chapter Clone() => new Chapter
    {
        ApprovedUtc = ApprovedUtc,
        BannerImageUrl = BannerImageUrl,
        CountryId = CountryId,
        CreatedUtc = CreatedUtc,
        DisplayOrder = DisplayOrder,
        Id = Id,
        Name = Name,
        OwnerId = OwnerId,
        Platform = Platform,
        PublishedUtc = PublishedUtc,
        RedirectUrl = RedirectUrl,
        Slug = Slug,
        TimeZone = TimeZone
    };

    public string GetDisplayName(PlatformType currentPlatform)
    {
        if (Platform != PlatformType.DrunkenKnitwits)
        {
            return Name;
        }

        if (currentPlatform == PlatformType.DrunkenKnitwits)
        {
            return Name.EndsWith(DrunkenKnitwitsSuffix, StringComparison.InvariantCultureIgnoreCase)
                ? Name.Substring(0, Name.Length - DrunkenKnitwitsSuffix.Length) 
                : Name;
        }
        
        return !Name.EndsWith(DrunkenKnitwitsSuffix, StringComparison.InvariantCultureIgnoreCase)
            ? Name + DrunkenKnitwitsSuffix
            : Name;
    }

    public string GetFullName(PlatformType currentPlatform)
    {
        if (Platform != PlatformType.DrunkenKnitwits)
        {
            return Name;
        }

        return !Name.EndsWith(DrunkenKnitwitsSuffix, StringComparison.InvariantCultureIgnoreCase)
            ? Name + DrunkenKnitwitsSuffix
            : Name;
    }

    public bool IsOpenForRegistration() => Approved() && Published();

    public bool Published() => PublishedUtc != null;

    public DateTime ToChapterTime(DateTime utc) => TimeZone != null
        ? TimeZoneInfo.ConvertTimeFromUtc(utc, TimeZone)
        : utc;

    public DateTime? ToChapterTime(DateTime? utc) => utc != null ? ToChapterTime(utc.Value) : null;
}
