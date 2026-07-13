using ODK.Core.Chapters;
using ODK.Services.Members.Models;

namespace ODK.Web.Razor.Models.Admin.Members;

public class MemberImportViewModel
{
    public required Chapter Chapter { get; init; }

    /// <summary>
    /// The parsed preview once a file has been uploaded; null on the initial upload screen.
    /// </summary>
    public MemberImportPreview? Preview { get; init; }
}
