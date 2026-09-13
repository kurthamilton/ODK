using System.ComponentModel;
using ODK.Services.Chapters;

namespace ODK.Web.Razor.Models.Admin.Chapters;

public class ChapterMigrationFormSubmitViewModel
{
    [DisplayName(ChapterTextLabels.MovedMessage)]
    public string? MessageHtml { get; set; }

    [DisplayName("Publish the moved page")]
    public bool Moved { get; set; }

    [DisplayName("Where you moved from")]
    public string? PreviousPlatformName { get; set; }
}
