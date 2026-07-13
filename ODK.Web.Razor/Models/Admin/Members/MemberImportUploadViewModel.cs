using System.ComponentModel.DataAnnotations;
using ODK.Core.Chapters;

namespace ODK.Web.Razor.Models.Admin.Members;

public class MemberImportUploadViewModel
{
    public required Chapter Chapter { get; init; }

    [Required]
    public IFormFile? File { get; init; }
}
