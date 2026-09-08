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
    /// <param name="supersededToken">
    /// The staging this upload replaces, where it replaces one - a re-upload from the review step. Dropped
    /// once the new rows are staged, so an import can only ever run against the file on screen: the token
    /// lives in a rendered form, and a page held in browser history would otherwise still redeem it.
    /// </param>
    Task<ServiceResult<MemberImportStagedPreview>> Build(
        IMemberChapterAdminServiceRequest request,
        IFormFile? file,
        string? supersededToken = null);
}
