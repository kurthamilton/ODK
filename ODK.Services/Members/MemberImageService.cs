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
        MemberImage image,
        MemberAvatar avatar)
    {
        image.ImageData = _imageService.Rotate(image.ImageData, 90);
        avatar.ImageData = _imageService.Rotate(avatar.ImageData, 90);
    }

    public ServiceResult UpdateMemberImage(
        MemberImage image,
        MemberAvatar avatar,
        byte[] imageData)
    {
        if (!_imageService.IsImage(imageData))
        {
            return ServiceResult.Failure("Invalid image");
        }

        var mimeType = MemberImage.DefaultMimeType;

        image.ImageData = _imageService.Process(imageData, new ImageProcessingOptions
        {
            MaxWidth = _settings.MemberAvatarSize,
            MimeType = mimeType
        });
        image.MimeType = mimeType;

        avatar.ImageData = image.ImageData;
        avatar.MimeType = mimeType;

        return ServiceResult.Successful();
    }
}
