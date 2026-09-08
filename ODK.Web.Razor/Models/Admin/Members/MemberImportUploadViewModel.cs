using System.ComponentModel.DataAnnotations;
using ODK.Core.Chapters;

namespace ODK.Web.Razor.Models.Admin.Members;

public class MemberImportUploadViewModel
{
    public required Chapter Chapter { get; init; }

    [Required]
    public IFormFile? File { get; init; }

    /// <summary>
    /// The staged upload this one replaces, on a re-upload from the review step. Null on a first upload,
    /// where there is nothing to replace.
    /// </summary>
    public string? SupersededToken { get; init; }
}
