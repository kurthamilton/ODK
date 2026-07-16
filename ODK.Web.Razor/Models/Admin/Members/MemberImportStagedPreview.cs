using ODK.Services.Members.Models;

namespace ODK.Web.Razor.Models.Admin.Members;

/// <summary>
/// The result of the member-import upload step: the preview to display, plus the token under which the
/// parsed rows have been staged server-side for the confirm step.
/// </summary>
public class MemberImportStagedPreview
{
    public required MemberImportPreview Preview { get; init; }

    public required string Token { get; init; }
}
