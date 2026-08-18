using FluentAssertions;
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

    [Test]
    public static void ValidateImage_IsAnImage_Succeeds()
    {
        // Arrange
        var service = CreateService(CreateMockImageService(isImage: true));

        // Act
        var result = service.ValidateImage([1, 2, 3]);

        // Assert
        result.Success.Should().BeTrue();
    }

    [Test]
    public static void ValidateImage_IsNotAnImage_Fails()
    {
        // Arrange
        var service = CreateService(CreateMockImageService(isImage: false));

        // Act
        var result = service.ValidateImage([1, 2, 3]);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Invalid image");
    }

    [Test]
    public static void UpdateMemberImage_IsNotAnImage_FailsWithoutProcessing()
    {
        // Arrange - the same check, so a rejected submission never reaches the resize.
        var imageService = new Mock<IImageService>();
        imageService.Setup(x => x.IsImage(It.IsAny<byte[]>())).Returns(false);
        var service = CreateService(imageService.Object);

        // Act
        var result = service.UpdateMemberImage(new MemberAvatar(), [1, 2, 3]);

        // Assert
        result.Success.Should().BeFalse();
        imageService.Verify(
            x => x.Process(It.IsAny<byte[]>(), It.IsAny<ImageProcessingOptions>()),
            Times.Never);
    }

    private static IImageService CreateMockImageService(
        ImageSize? imageSize = null,
        bool isImage = true)
    {
        var mock = new Mock<IImageService>();

        mock.Setup(x => x.Size(It.IsAny<byte[]>()))
            .Returns(imageSize ?? new ImageSize());
        mock.Setup(x => x.IsImage(It.IsAny<byte[]>()))
            .Returns(isImage);

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
