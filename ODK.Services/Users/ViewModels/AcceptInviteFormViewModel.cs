namespace ODK.Services.Users.ViewModels;

public class AcceptInviteFormViewModel : AcceptInviteFormSubmitViewModel
{
    /// <summary>
    /// The address the invitation was sent to, shown so the member can see which of their addresses their group
    /// holds. Read-only rather than merely unbound - see the base for why it cannot be posted.
    /// </summary>
    public required string EmailAddress { get; init; }
}
