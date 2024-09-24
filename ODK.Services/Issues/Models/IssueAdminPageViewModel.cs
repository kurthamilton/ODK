using ODK.Core.Issues;
using ODK.Core.Members;

namespace ODK.Services.Issues.Models;

public class IssueAdminPageViewModel
{
    public required Member CurrentMember { get; init; }

    public required Issue Issue { get; init; }

    public required Member Member { get; init; }

    public required IReadOnlyCollection<IssueMessage> Messages { get; init; }
}
