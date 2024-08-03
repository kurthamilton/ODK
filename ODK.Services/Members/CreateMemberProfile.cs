namespace ODK.Services.Members;

public class CreateMemberProfile : UpdateMemberProfile
{
    public string EmailAddress { get; set; } = "";

    public required UpdateMemberImage Image { get; set; }

    public required MemberImageCropInfo ImageCropInfo { get; set; }
}
