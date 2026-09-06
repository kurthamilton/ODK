namespace ODK.Services.Users.ViewModels;

public class RefuseInviteFormSubmitViewModel
{
    /// <summary>The token the invite link carried, posted back so the submit spends the same invite.</summary>
    public string Token { get; set; } = string.Empty;
}
