using ODK.Core.Members;

namespace ODK.Web.Razor.Models.Account;

public class EmailPreferencesContentViewModel
{
    public required string ChapterName { get; init; }

    public required Member CurrentMember { get; init; }
}
