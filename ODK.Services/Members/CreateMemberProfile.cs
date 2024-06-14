namespace ODK.Services.Members;

public class CreateMemberProfile : UpdateMemberProfile
{
    public CreateMemberProfile(UpdateMemberImage image)
    {
        Image = image;
    }

    public string EmailAddress { get; set; } = "";

    public UpdateMemberImage Image { get; }
}
