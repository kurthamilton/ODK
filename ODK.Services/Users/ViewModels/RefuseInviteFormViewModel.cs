namespace ODK.Services.Users.ViewModels;

public class RefuseInviteFormViewModel : RefuseInviteFormSubmitViewModel
{
    public required string EmailAddress { get; init; }

    /// <summary>The name the group imported, so the page can say who it thinks is declining.</summary>
    public required string Name { get; init; }
}
