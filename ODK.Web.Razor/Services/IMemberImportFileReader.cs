using ODK.Services;
using ODK.Services.Members.Models;

namespace ODK.Web.Razor.Services;

/// <summary>
/// Turns an uploaded file into import rows, or says why it cannot. The web layer's whole share of an
/// upload: what the group then does with the rows is the import service's.
/// </summary>
public interface IMemberImportFileReader
{
    ServiceResult<IReadOnlyCollection<MemberImportCsvRow>> Read(IFormFile? file);
}
