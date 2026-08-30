using ODK.Core.Platforms;

namespace ODK.Core.Web;

/// <summary>
/// A site-level FAQ entry, shown on the About page. Scoped to a platform rather than a chapter: the two
/// platforms are different products, so each keeps its own set and neither sees the other's.
/// </summary>
public class SiteQuestion : IVersioned, IDatabaseEntity
{
    public string AnswerHtml { get; set; } = string.Empty;

    public int DisplayOrder { get; set; }

    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public PlatformType Platform { get; set; }

    public byte[] Version { get; set; } = [];
}
