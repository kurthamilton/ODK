using ODK.Core.Members;

namespace ODK.Services.Members;

public interface IMemberImageService
{
    ServiceResult ProcessMemberImage(
        MemberImage image, 
        MemberAvatar avatar,
        UpdateMemberImage? model,
        MemberImageCropInfo cropInfo);

    void RotateMemberImage(
        MemberImage image,
        MemberAvatar avatar);
}
