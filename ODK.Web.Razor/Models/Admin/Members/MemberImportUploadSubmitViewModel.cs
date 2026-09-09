using System.ComponentModel.DataAnnotations;

namespace ODK.Web.Razor.Models.Admin.Members;

public class MemberImportUploadSubmitViewModel
{
    [Required]
    public IFormFile? File { get; init; }
}
