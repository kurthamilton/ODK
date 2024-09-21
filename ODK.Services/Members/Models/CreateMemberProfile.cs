namespace ODK.Services.Members.Models;

public class CreateMemberProfile : UpdateMemberChapterProfile
{
    public string EmailAddress { get; set; } = "";

    public bool? EmailOptIn { get; set; }

    public required string FirstName { get; set; }

    public required byte[] ImageData { get; set; }

    public required string LastName { get; set; }
}
