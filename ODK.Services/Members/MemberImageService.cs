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
        var validationResult = ValidateImage(imageData);
        if (!validationResult.Success)
        {
            return validationResult;
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

    public ServiceResult ValidateImage(byte[] imageData) => _imageService.IsImage(imageData)
        ? ServiceResult.Successful()
        : ServiceResult.Failure("Invalid image");
}
