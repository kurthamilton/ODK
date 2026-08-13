using ODK.Core.Platforms;

namespace ODK.Core.Emails;

public class SiteEmailSettings : IDatabaseEntity
{
    public string AdminTitle { get; set; } = string.Empty;

    public string FromEmailAddress { get; set; } = string.Empty;

    public string FromName { get; set; } = string.Empty;

    public Guid Id { get; set; }

    public string MemberTitle { get; set; } = string.Empty;

    public PlatformType Platform { get; set; }

    public string PlatformTitle { get; set; } = string.Empty;
}