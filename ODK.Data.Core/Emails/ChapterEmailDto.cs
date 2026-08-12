using ODK.Core.Emails;

namespace ODK.Data.Core.Emails;

/// <summary>
/// The templates one send needs: the layout every email renders into, the site's email for the type being
/// sent, and the group's override of each where it has made one.
/// </summary>
/// <remarks>
/// Both overrides are null for a send with no chapter. Where the type asked for is the layout - which is
/// what a send carrying its own body does - <see cref="SiteEmail"/> is the layout row as well.
/// </remarks>
public class ChapterEmailDto
{
    public required ChapterEmail? ChapterEmail { get; init; }

    public required ChapterEmail? ChapterLayout { get; init; }

    public required Email SiteEmail { get; init; }

    public required Email SiteLayout { get; init; }
}
