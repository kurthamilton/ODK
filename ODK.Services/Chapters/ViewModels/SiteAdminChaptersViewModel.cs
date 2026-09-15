namespace ODK.Services.Chapters.ViewModels;

public class SiteAdminChaptersViewModel
{
    public required IReadOnlyCollection<SiteAdminChaptersRowViewModel> Approved { get; init; }

    /// <summary>
    /// Groups still being set up, which have not asked to be looked at. Listed rather than hidden - a site
    /// admin can still open one or delete it - but not approvable, because approving a group its owner has
    /// not offered takes the decision about whether it is finished away from them.
    /// </summary>
    public required IReadOnlyCollection<SiteAdminChaptersRowViewModel> Drafts { get; init; }

    /// <summary>Groups whose owners have asked for them to be approved. The queue.</summary>
    public required IReadOnlyCollection<SiteAdminChaptersRowViewModel> Pending { get; init; }

    /// <summary>The viewing site admin's zone: a site-wide page has no chapter to fall back to.</summary>
    public required TimeZoneInfo TimeZone { get; init; }
}