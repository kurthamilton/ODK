namespace ODK.Web.Razor.Models.Admin.Chapters;

public class ChapterMigrationFormViewModel : ChapterMigrationFormSubmitViewModel
{
    /// <summary>
    /// Endpoint the rich text field posts its content to for the markup checks the browser cannot run.
    /// See <see cref="ChapterTextsFormViewModel.ValidateUrl"/>.
    /// </summary>
    public required string ValidateUrl { get; init; }
}
