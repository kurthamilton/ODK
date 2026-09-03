namespace ODK.Services.Members.ViewModels;

public class SiteAdminMemberSearchRowViewModel
{
    /// <summary>
    /// Set when this is the member the searching admin is acting as. They cannot be signed out from here -
    /// doing so would switch or end the session the page is being read in.
    /// </summary>
    public required bool Current { get; init; }

    public required string EmailAddress { get; init; }

    public required string FullName { get; init; }

    public required Guid MemberId { get; init; }

    /// <summary>
    /// Set when this member is already signed in on the searching admin's cookie, so the row offers to
    /// sign them out rather than in.
    /// </summary>
    public required bool SignedIn { get; init; }

    public required bool SiteAdmin { get; init; }
}
