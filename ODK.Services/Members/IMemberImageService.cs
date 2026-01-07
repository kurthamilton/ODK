using ODK.Core.Members;

namespace ODK.Services.Members;

public interface IMemberImageService
{
    void RotateMemberImage(
        MemberImage image,
        MemberAvatar avatar);

    ServiceResult UpdateMemberImage(
        MemberImage image,
        MemberAvatar avatar,
        byte[] imageData);
}
