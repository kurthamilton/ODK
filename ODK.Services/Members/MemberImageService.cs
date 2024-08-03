using ODK.Core.Images;
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

    public ServiceResult ProcessMemberImage(
        MemberImage image, 
        MemberAvatar avatar, 
        UpdateMemberImage? model, 
        MemberImageCropInfo cropInfo)
    {        
        var validationResult = model != null 
            ? ValidateMemberImage(model.MimeType, model.ImageData) 
            : ServiceResult.Successful();

        if (!validationResult.Success)
        {
            return validationResult;
        }

        ImageSize? originalSize = null;

        if (model != null)
        {
            image.ImageData = model.ImageData;
            image.MimeType = model.MimeType;

            originalSize = _imageService.Size(image.ImageData);

            EnforceMaxSize(image);

            var processedSize = _imageService.Size(image.ImageData);
            if (processedSize.Width < originalSize.Value.Width || processedSize.Height < originalSize.Value.Height)
            {
                // re-scale the original crop info
                var xRatio = 1.0 * processedSize.Width / originalSize.Value.Width;
                var yRatio = 1.0 * processedSize.Height / originalSize.Value.Height;

                cropInfo.CropX = (int)Math.Floor(xRatio * cropInfo.CropX);
                cropInfo.CropY = (int)Math.Floor(yRatio * cropInfo.CropY);
                cropInfo.CropWidth = (int)Math.Floor(xRatio * cropInfo.CropWidth);
                cropInfo.CropHeight = (int)Math.Floor(yRatio * cropInfo.CropHeight);
            }
        }

        UpdateAvatar(image, avatar, cropInfo);                        

        return ServiceResult.Successful();
    }

    public void RotateMemberImage(
        MemberImage image,
        MemberAvatar avatar)
    {
        var imageSize = _imageService.Size(image.ImageData);

        if (avatar.ImageData.Length == 0)
        {            
            var shortSide = Math.Min(imageSize.Width, imageSize.Height);

            UpdateAvatar(image, avatar, new MemberImageCropInfo
            {
                CropHeight = shortSide,
                CropWidth = shortSide
            });
        }

        image.ImageData = _imageService.Rotate(image.ImageData, 90);
        avatar.ImageData = _imageService.Rotate(avatar.ImageData, 90);

        var newX = imageSize.Height - avatar.CropHeight - avatar.CropY;
        var newY = avatar.CropX;

        avatar.CropX = newX;
        avatar.CropY = newY;
    }

    private void EnforceMaxSize(MemberImage image)
    {
        var maxSize = _settings.MaxImageSize;

        var size = _imageService.Size(image.ImageData);
        if (size.Width <= maxSize && size.Height <= maxSize)
        {
            return;
        }

        image.ImageData = _imageService.Reduce(image.ImageData, maxSize, maxSize);
    }

    private void UpdateAvatar(MemberImage image, MemberAvatar avatar, MemberImageCropInfo cropInfo)
    {
        var imageData = image.ImageData;
        
        if (cropInfo.CropWidth > 0 && cropInfo.CropHeight > 0 && cropInfo.CropX >= 0 && cropInfo.CropY >= 0)
        {
            imageData = _imageService.Crop(imageData, cropInfo.CropWidth, cropInfo.CropHeight, cropInfo.CropX, cropInfo.CropY);
        }
        
        imageData = _imageService.Resize(imageData, _settings.MemberAvatarSize, _settings.MemberAvatarSize);

        var size = _imageService.Size(imageData);
        if (size.Width < _settings.MemberAvatarSize || size.Height < _settings.MemberAvatarSize)
        {
            imageData = _imageService.Pad(imageData, _settings.MemberAvatarSize, _settings.MemberAvatarSize);
        }        
        
        avatar.ImageData = imageData;
        avatar.MimeType = image.MimeType;

        avatar.CropX = cropInfo.CropX;
        avatar.CropY = cropInfo.CropY;
        avatar.CropWidth = cropInfo.CropWidth;
        avatar.CropHeight = cropInfo.CropHeight;
    }

    private ServiceResult ValidateMemberImage(string mimeType, byte[] data)
    {
        if (!ImageValidator.IsValidMimeType(mimeType) ||
            !_imageService.IsImage(data))
        {
            return ServiceResult.Failure("File is not a valid image");
        }

        return ServiceResult.Successful();
    }
}
