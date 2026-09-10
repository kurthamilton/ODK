using ODK.Core.Chapters;
using ODK.Core.Platforms;

namespace ODK.Web.Razor.Models.Account;

public class AccountMenuViewModel
{
    public string? Active { get; set; }

    public required Chapter? Chapter { get; init; }

    public required PlatformType Platform { get; init; }
}
