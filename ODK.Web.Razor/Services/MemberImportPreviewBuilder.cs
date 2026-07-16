using ODK.Services;
using ODK.Services.Members;
using ODK.Services.Members.Models;

namespace ODK.Web.Razor.Services;

/// <summary>
/// Shared logic for the member-import preview step, used by the equivalent page on each platform
/// so that both behave identically. Reads the uploaded CSV and classifies each row.
/// </summary>
public static class MemberImportPreviewBuilder
{
    public static async Task<ServiceResult<MemberImportPreview>> Build(
        IMemberAdminService memberAdminService,
        IMemberChapterAdminServiceRequest request,
        IFormFile? file)
    {
        var csvResult = CsvFileReader.Read<MemberImportModel>(file);
        if (!csvResult.Success || csvResult.Value == null)
        {
            return ServiceResult<MemberImportPreview>.Failure(csvResult.Message ?? "The file could not be read");
        }

        // Drop rows without an email address (e.g. a missing/blank email column) - they can't be matched
        // or imported, and a null email would otherwise throw when the preview is built.
        var members = csvResult.Value
            .Where(x => !string.IsNullOrWhiteSpace(x.EmailAddress))
            .ToArray();

        if (members.Length == 0)
        {
            return ServiceResult<MemberImportPreview>.Failure(
                "The uploaded file did not contain any rows with an email address");
        }

        var preview = await memberAdminService.GetMemberImportPreview(request, members);
        return ServiceResult<MemberImportPreview>.Successful(preview);
    }
}
