using ODK.Core.Chapters;

namespace ODK.Web.Razor.Models.Groups;

/// <summary>
/// The moved page's way back in for somebody who was invited from the old platform and cannot find the
/// email. See <c>Groups/_GroupMovedInviteResend</c>.
/// </summary>
public class GroupMovedInviteResendViewModel
{
    public required Chapter Chapter { get; init; }

    /// <summary>
    /// Never populated on render. The form posts it, and nothing reads it back: the page answers the same
    /// way whatever address was submitted, so echoing one would be the only thing on the page that
    /// distinguished a hit from a miss.
    /// </summary>
    public string? EmailAddress { get; init; }
}
