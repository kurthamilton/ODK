using ODK.Core.Chapters;

namespace ODK.Web.Razor.Models.Admin.Members;

public class MemberImportUploadViewModel : MemberImportUploadSubmitViewModel
{
    public required Chapter Chapter { get; init; }
}
