namespace ODK.Services.Members.Models;

public class MemberCreateProfile : MemberChapterProfileUpdateModel
{
    public string EmailAddress { get; set; } = string.Empty;

    public bool? EmailOptIn { get; set; }

    public required string FirstName { get; set; }

    public required byte[] ImageData { get; set; }

    public required string LastName { get; set; }
}
