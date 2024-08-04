using ODK.Core.Members;

namespace ODK.Services.Users.ViewModels;

public class AccountViewModel
{
    public required string ChapterName { get; init; }

    public required Member CurrentMember { get; init; }
}
