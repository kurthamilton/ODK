using ODK.Core.Platforms;

namespace ODK.Core.Chapters;

public class Chapter : IDatabaseEntity, ITimeZoneEntity
{
    public const string DefaultTimeZoneId = "GMT Standard Time";

    public const string DrunkenKnitwitsSuffix = " Drunken Knitwits";

    public static readonly TimeZoneInfo DefaultTimeZone = TimeZoneInfo.FindSystemTimeZoneById(DefaultTimeZoneId);

    public DateTime? ApprovedUtc { get; set; }

    public string? BannerImageUrl { get; set; }

    public Guid CountryId { get; set; }

    public DateTime CreatedUtc { get; set; }

    public int? DisplayOrder { get; set; }

    public string FullName => GetFullName(Platform, Name);

    public Guid Id { get; set; }

    public required string Name { get; set; }

    public Guid OwnerId { get; set; }

    public PlatformType Platform { get; set; }

    public DateTime? PublishedUtc { get; set; }

    public string? RedirectUrl { get; set; }

    public string ShortName => GetShortName(Platform, Name);

    public required string Slug { get; set; }

    public string? ThemeBackground { get; set; }

    public string? ThemeColor { get; set; }

    public TimeZoneInfo? TimeZone { get; set; }

    public static string CleanName(string name) => name.Trim();

    public static string GetDisplayName(
        PlatformType currentPlatform, PlatformType platform, string name)
    {
        if (platform != PlatformType.DrunkenKnitwits)
        {
            // default platform groups should just use the name as-is
            return name;
        }

        // Return short name for ODK chapters on ODK platform
        // Return full name for ODK chapters on default platform
        return currentPlatform == PlatformType.DrunkenKnitwits
            ? GetShortName(platform, name)
            : GetFullName(platform, name);
    }

    public static string GetFullName(
        PlatformType platform, string name)
    {
        if (platform != PlatformType.DrunkenKnitwits)
        {
            return name;
        }

        return !name.EndsWith(DrunkenKnitwitsSuffix, StringComparison.OrdinalIgnoreCase)
            ? name + DrunkenKnitwitsSuffix
            : name;
    }

    public static string GetShortName(
        PlatformType platform, string name)
    {
        if (platform != PlatformType.DrunkenKnitwits)
        {
            return name;
        }

        return name.EndsWith(DrunkenKnitwitsSuffix, StringComparison.OrdinalIgnoreCase)
            ? name.Substring(0, name.Length - DrunkenKnitwitsSuffix.Length)
            : name;
    }

    public bool Approved() => ApprovedUtc != null;

    public bool CanBeDeleted(int memberCount) => memberCount <= 1;

    public bool CanBePublished() => Approved() && !IsPublished();

    public string GetDisplayName(PlatformType currentPlatform)
        => GetDisplayName(currentPlatform, Platform, Name);

    public bool IsOpenForRegistration() => Approved() && IsPublished();

    public bool IsPublished() => PublishedUtc != null;

    public DateTime ToChapterTime(DateTime utc) => TimeZone != null
        ? TimeZoneInfo.ConvertTimeFromUtc(utc, TimeZone)
        : utc;

    public DateTime? ToChapterTime(DateTime? utc) => utc != null ? ToChapterTime(utc.Value) : null;
}