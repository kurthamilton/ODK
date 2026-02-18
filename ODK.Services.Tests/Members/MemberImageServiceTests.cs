using Moq;
using NUnit.Framework;
using ODK.Core.Images;
using ODK.Core.Members;
using ODK.Services.Imaging;
using ODK.Services.Members;

namespace ODK.Services.Tests.Members;

[Parallelizable]
public static class MemberImageServiceTests
{
    private const int MaxImageSize = 250;
    private const int MemberAvatarSize = 75;

    //// Rotate a 100x100 square 4 ways with a 50x50 crop area starting in top-left
    //[TestCase(100, 100, 0, 0, 50, 50, ExpectedResult = 50)]
    //[TestCase(100, 100, 50, 0, 50, 50, ExpectedResult = 50)]
    //[TestCase(100, 100, 50, 50, 50, 50, ExpectedResult = 0)]
    //[TestCase(100, 100, 0, 50, 50, 50, ExpectedResult = 0)]

    //// Rotate a 100x200 rectangle 4 ways with a 50x50 crop area starting in top-left
    //[TestCase(100, 200, 0, 0, 50, 50, ExpectedResult = 150)]
    //[TestCase(200, 100, 150, 0, 50, 50, ExpectedResult = 50)]
    //[TestCase(100, 200, 50, 150, 50, 50, ExpectedResult = 0)]
    //[TestCase(200, 100, 0, 50, 50, 50, ExpectedResult = 0)]
    //public static int RotateMemberImage_UpdatesCropX(
    //    int imageWidth, int imageHeight, 
    //    int cropX, int cropY, 
    //    int cropWidth, int cropHeight)
    //{
    //    // Arrange
    //    var imageSize = new ImageSize(imageWidth, imageHeight);
    //    var imageService = CreateMockImageService(imageSize: imageSize);

    //    var service = CreateService(imageService: imageService);

    //    var image = CreateMemberImage();
    //    var avatar = new MemberAvatar
    //    {
    //        CropX = cropX,
    //        CropY = cropY,
    //        CropWidth = cropWidth,
    //        CropHeight = cropHeight,
    //        ImageData = [1]
    //    };

    //    // Act
    //    service.RotateMemberImage(image, avatar);

    //    // Assert
    //    return avatar.CropX;
    //}

    //// Rotate a 100x100 square 4 ways with a 50x50 crop area starting in top-left
    //[TestCase(100, 100, 0, 0, 50, 50, ExpectedResult = 0)]
    //[TestCase(100, 100, 50, 0, 50, 50, ExpectedResult = 50)]
    //[TestCase(100, 100, 50, 50, 50, 50, ExpectedResult = 50)]
    //[TestCase(100, 100, 0, 50, 50, 50, ExpectedResult = 0)]

    //// Rotate a 100x200 rectangle 4 ways with a 50x50 crop area starting in top-left
    //[TestCase(100, 200, 0, 0, 50, 50, ExpectedResult = 0)]
    //[TestCase(200, 100, 150, 0, 50, 50, ExpectedResult = 150)]
    //[TestCase(100, 200, 50, 150, 50, 50, ExpectedResult = 50)]
    //[TestCase(200, 100, 0, 50, 50, 50, ExpectedResult = 0)]
    //public static int RotateMemberImage_UpdatesCropY(
    //    int imageWidth, int imageHeight,
    //    int cropX, int cropY,
    //    int cropWidth, int cropHeight)
    //{
    //    // Arrange
    //    var imageSize = new ImageSize(imageWidth, imageHeight);
    //    var imageService = CreateMockImageService(imageSize: imageSize);

    //    var service = CreateService(imageService: imageService);

    //    var image = CreateMemberImage();
    //    var avatar = new MemberAvatar
    //    {
    //        CropX = cropX,
    //        CropY = cropY,
    //        CropWidth = cropWidth,
    //        CropHeight = cropHeight,
    //        ImageData = [1]
    //    };

    //    // Act
    //    service.RotateMemberImage(image, avatar);

    //    // Assert
    //    return avatar.CropY;
    //}

    private static IImageService CreateMockImageService(
        ImageSize? imageSize = null)
    {
        var mock = new Mock<IImageService>();

        mock.Setup(x => x.Size(It.IsAny<byte[]>()))
            .Returns(imageSize ?? new ImageSize());

        return mock.Object;
    }

    private static MemberImageService CreateService(
        IImageService? imageService = null)
    {
        return new MemberImageService(
            imageService ?? CreateMockImageService(),
            new MemberImageServiceSettings
            {
                MaxImageSize = MaxImageSize,
                MemberAvatarSize = MemberAvatarSize
            });
    }
}
