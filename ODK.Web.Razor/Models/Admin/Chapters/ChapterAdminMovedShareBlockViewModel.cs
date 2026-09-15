namespace ODK.Web.Razor.Models.Admin.Chapters;

/// <summary>
/// One block of ready-made wording on the moved page admin, with a button that copies it. See
/// <c>Admin/Chapter/_ChapterAdminMovedShareBlock</c>.
/// </summary>
public class ChapterAdminMovedShareBlockViewModel
{
    /// <summary>Where this wording is meant to go, which is what an organiser picks between.</summary>
    public required string Description { get; init; }

    /// <summary>The wording itself. Plain text - see <c>ChapterMigrationShareViewModel</c>.</summary>
    public required string Text { get; init; }

    public required string Title { get; init; }
}
