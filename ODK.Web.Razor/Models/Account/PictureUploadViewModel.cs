using ODK.Core.Members;

namespace ODK.Web.Razor.Models.Account;

public class PictureUploadViewModel
{
    /// <summary>
    /// Dummy form field
    /// </summary>
    public string? Image { get; set; }

    public string? ImageDataUrl { get; set; }

    public MemberAvatar? MemberAvatar { get; set; }
}
