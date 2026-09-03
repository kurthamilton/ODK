using System;

namespace ODK.Web.Common.Sitemap;

/// <summary>
/// One entry of a sitemap: a path relative to the platform's base URL, and when what it points at last
/// changed where that is known.
/// </summary>
/// <remarks>
/// Carries no priority or change frequency. Both are in the sitemap schema and both are ignored by the
/// search engines that read it, so stating them would be describing the site to nobody.
/// </remarks>
public class SitemapNode
{
    public DateTime? LastModifiedUtc { get; init; }

    public required string Path { get; init; }
}
