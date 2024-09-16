using ODK.Core.Chapters;
using ODK.Core.Platforms;

namespace ODK.Web.Razor.Models.Components;

public class UpgradeButtonViewModel
{
    public required Chapter? Chapter { get; init; }

    public required PlatformType Platform { get; init; }
}
