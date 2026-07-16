using ODK.Services;
using ODK.Web.Razor.Models.Admin.Members;

namespace ODK.Web.Razor.Services;

/// <summary>
/// Shared logic for the member-import preview step, used by the equivalent page on each platform
/// so that both behave identically. Reads the uploaded CSV, classifies each row, and stages the parsed
/// rows server-side so the confirm step only needs to post a token.
/// </summary>
public interface IMemberImportPreviewBuilder
{
    Task<ServiceResult<MemberImportStagedPreview>> Build(
        IMemberChapterAdminServiceRequest request,
        IFormFile? file);
}
