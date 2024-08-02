namespace ODK.Services.Members;

public class CreateMemberProfile : UpdateMemberProfile
{
    public string EmailAddress { get; set; } = "";

    public required byte[] ImageData { get; set; }
}
