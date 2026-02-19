using ODK.Core.Chapters;

namespace ODK.Web.Razor.Models.Admin;

public class AdminContextMenuViewModel
{
    public bool AlignRight { get; init; }

    public required Chapter Chapter { get; init; }
}