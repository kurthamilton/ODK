namespace ODK.Web.Razor.Models.Sitemap;

public class SitemapNodeModel
{
    public SitemapFrequencyType? Frequency { get; init; }
    public DateTime? LastModified { get; init; }
    public double? Priority { get; init; }
    public required string Url { get; init; }
}
