namespace ODK.Web.Razor.Models.Admin.Members;

public class MemberImportSubmitViewModel
{
    /// <summary>
    /// Token identifying the parsed rows staged server-side during the upload/preview step.
    /// </summary>
    public string? Token { get; init; }
}
