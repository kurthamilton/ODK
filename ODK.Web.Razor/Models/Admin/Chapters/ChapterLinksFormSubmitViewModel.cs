using System.ComponentModel;

namespace ODK.Web.Razor.Models.Admin.Chapters;

public class ChapterLinksFormSubmitViewModel
{
    public string? Facebook { get; set; }

    public string? Instagram { get; set; }

    public string? Twitter { get; set; }

    [DisplayName("Show Instagram feed")]
    public bool ShowInstagramFeed { get; set; }

    public string? WhatsApp { get; set; }
}