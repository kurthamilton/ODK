using ODK.Core.Chapters;
using ODK.Services.Members.Models;

namespace ODK.Web.Razor.Models.Admin.Members;

public class MemberImportReviewViewModel
{
    public required Chapter Chapter { get; init; }

    public required MemberImportPreview Preview { get; init; }

    public required string Token { get; init; }
}
