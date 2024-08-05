using ODK.Core.Members;

namespace ODK.Web.Razor.Models.Account;

public class ChangeEmailContentViewModel
{
    public required string? ChapterName { get; init; }

    public required Member CurrentMember { get; init; }
}
