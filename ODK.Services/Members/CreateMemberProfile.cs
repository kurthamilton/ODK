namespace ODK.Services.Members;

public class CreateMemberProfile : UpdateMemberChapterProfile
{
    public string EmailAddress { get; set; } = "";

    public required string FirstName { get; set; }

    public required UpdateMemberImage Image { get; set; }

    public required MemberImageCropInfo ImageCropInfo { get; set; }

    public required string LastName { get; set; }
}
