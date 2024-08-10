using ODK.Core.Platforms;

namespace ODK.Core.Emails;

public class SiteEmailSettings : IDatabaseEntity
{
    public string ContactEmailAddress { get; set; } = "";

    public string FromEmailAddress { get; set; } = "";

    public string FromName { get; set; } = "";

    public Guid Id { get; set; }

    public PlatformType Platform { get; set; }

    public string Title { get; set; } = "";
}
