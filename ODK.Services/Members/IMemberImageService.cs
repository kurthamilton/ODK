using ODK.Core.Members;

namespace ODK.Services.Members;

public interface IMemberImageService
{
    void RotateMemberImage(
        MemberAvatar avatar);

    ServiceResult UpdateMemberImage(
        MemberAvatar avatar,
        byte[] imageData);

    /// <summary>
    /// Whether <paramref name="imageData"/> is an image this can process, without processing it. Lets a
    /// caller reject a submission before it writes anything, where <see cref="UpdateMemberImage"/> would
    /// resize and re-encode as well.
    /// </summary>
    ServiceResult ValidateImage(byte[] imageData);
}
