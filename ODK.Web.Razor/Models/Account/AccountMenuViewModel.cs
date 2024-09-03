using ODK.Core.Chapters;

namespace ODK.Web.Razor.Models.Account;

public class AccountMenuViewModel
{
    public string? Active { get; set; }

    public required Chapter? Chapter { get; init; }
}
