using ODK.Core.Chapters;

namespace ODK.Web.Razor.Models.Groups.Profile;

public class GroupProfileMenuViewModel
{
    public string? ActivePath { get; init; }

    public required Chapter Chapter { get; init; }
}
