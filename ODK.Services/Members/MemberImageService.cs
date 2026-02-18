using ODK.Core.Members;
using ODK.Services.Imaging;

namespace ODK.Services.Members;

public class MemberImageService : IMemberImageService
{
    private readonly IImageService _imageService;
    private readonly MemberImageServiceSettings _settings;

    public MemberImageService(IImageService imageService, MemberImageServiceSettings settings)
    {
        _imageService = imageService;
        _settings = settings;
    }

    public void RotateMemberImage(
        MemberAvatar avatar)
    {
        avatar.ImageData = _imageService.Rotate(avatar.ImageData, 90);
    }

    public ServiceResult UpdateMemberImage(
        MemberAvatar avatar,
        byte[] imageData)
    {
        if (!_imageService.IsImage(imageData))
        {
            return ServiceResult.Failure("Invalid image");
        }

        var mimeType = MemberAvatar.DefaultMimeType;

        avatar.ImageData = _imageService.Process(imageData, new ImageProcessingOptions
        {
            MaxWidth = _settings.MemberAvatarSize,
            MimeType = mimeType
        });
        avatar.MimeType = mimeType;

        return ServiceResult.Successful();
    }
}
