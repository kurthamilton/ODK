using ODK.Core.Issues;

namespace ODK.Services.Issues.Models;

public class IssueAdminPageViewModel
{
    public required Issue Issue { get; init; }

    public required IReadOnlyCollection<IssueMessage> Messages { get; init; }
}
