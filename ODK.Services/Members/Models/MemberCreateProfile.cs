namespace ODK.Services.Members.Models;

public class MemberCreateProfile : MemberChapterProfileUpdateModel
{
    public string EmailAddress { get; set; } = string.Empty;

    public bool? EmailOptIn { get; set; }

    public required string FirstName { get; set; }

    public required byte[] ImageData { get; set; }

    /// <summary>
    /// The invitation token the sign-up arrived with, where it came from an invitation email. Trusted only when
    /// it belongs to an invitation held by the account the posted address resolves to.
    /// </summary>
    public string? InviteToken { get; set; }

    public required string LastName { get; set; }

    public string RecaptchaToken { get; set; } = string.Empty;
}
