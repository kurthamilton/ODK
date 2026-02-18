using ODK.Core.Members;

namespace ODK.Services.Members;

public interface IMemberImageService
{
    void RotateMemberImage(
        MemberAvatar avatar);

    ServiceResult UpdateMemberImage(
        MemberAvatar avatar,
        byte[] imageData);
}
